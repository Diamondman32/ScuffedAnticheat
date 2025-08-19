using Terraria.ModLoader;
using System.IO;
using Terraria;
using ScuffedAnticheatMod.Network;
using ReLogic.Content;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using System;
using ScuffedAnticheatMod.UI;
using System.Reflection;
using Terraria.ModLoader.Core;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour.HookGen;
using Terraria.Chat;
using Terraria.Localization;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using log4net.Repository.Hierarchy;

namespace ScuffedAnticheatMod
{
    public class ScuffedAnticheatMod : Mod
    {
        public static ScuffedAnticheatMod instance;
        private static List<byte[]> ModHashes = new();
        public static List<byte[]> modHashes => ModHashes;
        public static bool modsHashedSuccessfully { get; private set; }

        // Runs after all mods are loaded. Retrieves all hashes and if HerosMod is enabled, add a UI button which has SAM deletedItem UI functionality
        public override void PostSetupContent()
        {
            // Get mod hashes
            modsHashedSuccessfully = true;
            ModHashes.Clear();
            bool loadedTModLoader = false;

            foreach (Mod mod in ModLoader.Mods)
            {
                if (mod.Name == "ModLoader" && !loadedTModLoader)
                {
                    // Maybe check tmodloader.dll
                    loadedTModLoader = true;
                }
                else if (mod.Name == "ModLoader" && loadedTModLoader)
                {
                    modsHashedSuccessfully = false;
                }
                else
                {
                    PropertyInfo field = typeof(Mod).GetProperty("File", BindingFlags.Instance | BindingFlags.NonPublic);
                    TmodFile file = (TmodFile)field?.GetValue(mod);
                    byte[] hash = file?.Hash;

                    if (hash != null)
                        ModHashes.Add(hash);
                    else
                    {
                        modsHashedSuccessfully = false;

                        if (Main.dedServ)
                            Main.NewText("[ScuffedAnticheatMod] Mod hashing failed. Users may not be able to join!", Color.Red);
                    }
                }
            }

            // Add HerosMod UI button
            if (ModLoader.TryGetMod("HerosMod", out Mod herosMod))
            {
                // Add a permission
                string permissionName = "RecoverItemsFromVoid";
                herosMod.Call("AddPermission", permissionName, "Recover Items From Void", null);
                // Add a button
                Asset<Texture2D> texture = TextureAssets.Trash;
                Action buttonClicked = () => { ModContent.GetInstance<UIOverlay>().TogglePlayerList(); };
                Action<bool> groupUpdated = (bool b) => { };
                Func<string> tooltip = () => { return "Deleted Items"; };
                herosMod.Call("AddSimpleButton", permissionName, texture, buttonClicked, groupUpdated, tooltip);
            }
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            SAMNetwork.HandlePacket(reader, whoAmI);
        }

        // TODO: consider using steam id instead
        // On mod load, get an instance to use for creating packets and create save directory
        public override void Load()
        {
            instance = this;
            Directory.CreateDirectory(Main.SavePath);
            if (!Main.dedServ && !Guid.HasKey())
            {
                Guid.CreateKey();
            }
            else if (Main.dedServ)
            {
                SAMNetwork.DeserializeAll();
                UpdateItemSaveData.Autosave();

                Logger.InfoFormat("{0} Log", Name);

                // IL
                MethodInfo method = typeof(MessageBuffer).GetMethod("GetData");
                HookEndpointManager.Modify(method, GetData_ILEdit);
            }
        }

        private static void GetData_ILEdit(ILContext il)
        {
            ILCursor c = new ILCursor(il);


            ILLabel[] targets = null;
            while (c.TryGotoNext(i => i.MatchSwitch(out targets)))
            {
                // Compiler wants the starting case to be 0, so it will subtract away the lowest case and shift everything down
                // ldc.i4.1
                // sub
                // switch
                int offset = 0;
                if (c.Prev.MatchSub() && c.Prev.Previous.MatchLdcI4(out offset))
                {
                    ;
                }

                // Get the label for case 5: if it exists
                int case5Index = 5 - offset;
                if (case5Index < 0 || case5Index >= targets.Length || targets[case5Index] is not ILLabel target)
                {
                    continue;
                }

                // Move the cursor to case 5:
                c.GotoLabel(target);

                // Get all read variable indexes
                // ldarg
                // ldfld BinaryReader reader
                // callvirt int16 ReadInt16() OR callvirt uint8 ReadByte()
                // stloc
                int[] indexes = new int[5];
                for (int j = 0; j < indexes.Length; j++)
                {
                    if (!c.TryGotoNext(MoveType.After,
                         i => i.MatchLdarg(out _),
                         i => i.MatchLdfld(typeof(MessageBuffer).GetField("reader")),
                         i => i.MatchCallvirt(typeof(BinaryReader).GetMethod("ReadInt16")) || i.MatchCallvirt(typeof(BinaryReader).GetMethod("ReadByte")),
                         i => i.MatchStloc(out indexes[j])
                    ))
                    {
                        throw new Exception("Could not find byte read pattern");
                    }
                    else
                    {
                        c.Index--;
                        c.Emit(OpCodes.Dup);
                        c.Emit(OpCodes.Box, typeof(int));
                        c.EmitDelegate(DoSomething);
                    }
                }

                // Put all variables onto stack
                for (int j = 0; j < indexes.Length; j++)
                {
                    c.Emit(OpCodes.Ldloc, indexes[j]);
                    c.Emit(OpCodes.Box, typeof(int));
                }

                // Now pop all variables into delegate
                c.EmitDelegate(new Action<object, object, object, object, object>((playerID, slotType, stack, prefix, type) =>
                {
                    ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"playerID: {playerID}, slotType: {slotType}, stack: {stack}, prefix: {prefix}, type: {type}"), Color.Aqua);
                }));

                foreach (var instr in c.Instrs)
                {
                    instance.Logger.Info($"{instr.Offset:X4}: {instr.OpCode} {instr.Operand}");
                }

                // Hook applied successfully
                return;
            }

            // Couldn't find the right place to insert.
            throw new Exception("Hook location not found, switch(*) { case 5: ...");
        }

        public static void DoSomething(object num)
        {
            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"{num}"), Color.Red);
        }
    }
}
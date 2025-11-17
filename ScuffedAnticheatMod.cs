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
using Mono.Cecil;

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
                PlayerData.Initialize();
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
                
                // load argument 0 (this) onto stack and use it for hook
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<MessageBuffer, (int, int)>>((self) =>
                {
                    BinaryReader r = self.reader;
                    long start = r.BaseStream.Position;

                    // 1) bufferID (overriden), 2) slotType, 3) type (doesn't work idk), 4) prefix, 5) stack
                    _ = r.ReadByte();
                    int slotType = r.ReadInt16();
                    // ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"{slotType}"), Color.Green);

                    r.BaseStream.Position = start;

                    return (self.whoAmI, slotType);
                });
                TypeReference tupleType = il.Method.Module.ImportReference(typeof((int, int)));
                VariableDefinition localTuple = new VariableDefinition(tupleType);
                il.Body.Variables.Add(localTuple);
                c.Emit(OpCodes.Stloc, localTuple);

                c.GotoNext(i => i.MatchEndfinally());
                // c.GotoNext(i => i.MatchLeave(out _));
                // c.GotoNext(
                //     i => i.MatchLdcI4(out _),
                //     i => i.MatchLdcR4(out _),
                //     i => i.MatchLdcR4(out _),
                //     i => i.MatchLdcR4(out _),
                //     i => i.MatchLdcI4(out _),
                //     i => i.MatchLdcI4(out _),
                //     i => i.MatchLdcI4(out _),
                //     i => i.MatchCall(typeof(bool), "TrySendData")
                //     );
                c.Emit(OpCodes.Ldloc, localTuple);
                c.EmitDelegate<Action<(int, int)>>(x =>
                {
                    // ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"{x.Item1}, {x.Item2}"), Color.Beige);
                    UpdateInventory.OnInventoryChange(x.Item1, x.Item2);
                });

                // foreach (var instr in c.Instrs)
                // {
                //     instance.Logger.Info($"{instr.Offset:X4}: {instr.OpCode} {instr.Operand}");
                // }

                return;
            }

            throw new Exception("Hook location not found, switch(*) { case 5: ...");
        }
    }
}
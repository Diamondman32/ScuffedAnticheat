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

        // On mod load, get an instance to use for creating packets and create save directory
        public override void Load()
        {
            instance = this;
            Directory.CreateDirectory(Main.SavePath);
            if (!Main.dedServ && !Guid.HasKey())
            {
                Guid.CreateKey(); // Maybe use steam id and resort to guid if unavailible
            }
            else if (Main.dedServ)
            {
                SAMNetwork.DeserializeAll();
                UpdateItemSaveData.Autosave();

                // IL
                MethodInfo method = typeof(MessageBuffer).GetMethod("GetData");
                HookEndpointManager.Modify(method, GetData_ILEdit);
            }
        }

        private static void GetData_ILEdit(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            // Move right before messageType instructions
            if (!c.TryGotoNext(
                i => i.MatchLdfld(typeof(MessageBuffer).GetField("readBuffer")),
                i => i.MatchLdarg(1),
                i => i.MatchLdelemU1(),
                i => i.MatchDup(),
                i => i.MatchStloc(out _)
            ))
            {
                throw new Exception("Could not find byte read pattern");
            }

            // Go after ldelem.u1
            c.Index += 3;

            // Duplicate value and call my function
            c.Emit(OpCodes.Dup); // duplicate the byte value
            c.Emit(OpCodes.Call, typeof(ScuffedAnticheatMod).GetMethod(nameof(OnMessage)));
        }

        public static void OnMessage(int messageType)
        {
            if (messageType == 22)
                return;
            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"{messageType}"), Color.Aqua);
        }
    }
}
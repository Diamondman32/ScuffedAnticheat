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
            }
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            SAMNetwork.HandlePacket(reader, whoAmI);
        }
    }
}

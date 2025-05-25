using Terraria.ModLoader;
using System.IO;
using Terraria;
using ScuffedAnticheatMod.Network;
using ReLogic.Content;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using System;
using ScuffedAnticheatMod.UI;

namespace ScuffedAnticheatMod
{
    public class ScuffedAnticheatMod : Mod
	{
		public static ScuffedAnticheatMod instance;

        // Runs after all mods are loaded. If HerosMod is enabled, add a UI button which has SAM deletedItem UI functionality
        public override void PostSetupContent()
        {
            if(ModLoader.TryGetMod("HerosMod", out Mod herosMod))
            {
                // Add a permission
                string permissionName = "RecoverItemsFromVoid";
                herosMod.Call("AddPermission", permissionName, "Recover Items From Void", null);
                // Add a button
                Asset<Texture2D> texture = TextureAssets.Trash;
                Action buttonClicked = () => {ModContent.GetInstance<SAMModSystem>().TogglePlayerList();};
                Action<bool> groupUpdated = (bool b) => {};
                Func<string> tooltip = () => {return "Deleted Items";};
                herosMod.Call("AddSimpleButton", permissionName, texture, buttonClicked, groupUpdated, tooltip);
            }
        }

        // On mod load, get an instance to use for creating packets and create save directory
        public override void Load()
        {
			instance = this;
            Directory.CreateDirectory(Main.SavePath);
            if(!Main.dedServ && !Guid.HasKey())
            {
                Guid.CreateKey(); // Maybe use steam id and resort to guid if unavailible
            }
            else if(Main.dedServ)
            {
                SAMNetwork.DeserializeAll();
            }
        }
        public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
            SAMNetwork.HandlePacket(reader, whoAmI);
		}
    }
}

// TODO:
// Investigate unique identifiers for players
// Add per-world support in data 
// Check world data when swapping from origin world

// Error: Makes tons of dupe Playerinventoriers without guids ("uninitialized") and alongside a null one

//  Side Projects:
//  Consider tracking and updating player position

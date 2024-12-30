using Terraria.ModLoader;
using System.IO;
using Terraria;
using Terraria.ID;
using VIRUS.Network;

namespace VIRUS
{
    public class VIRUS : Mod
	{
		public static VIRUS instance;

        // Runs after all mods are loaded. If HerosMod is enabled, add a UI button which has SAM deletedItem UI functionality
        public override void PostSetupContent()
        {
            if(ModLoader.TryGetMod("HerosMod", out Mod herosMod))
            {
                // Add a permission
                // string permissionName = "RecoverItemsFromVoid";
                // herosMod.Call("AddPermission", permissionName, "Recover Items From Void", null);
                // // Add a button
                // Asset<Texture2D> texture = TextureAssets.Trash;
                // Action buttonClicked = () => {ModContent.GetInstance<MenuBarSystem>().TogglePlayerList();};
                // Action<bool> groupUpdated = (bool b) => {};
                // Func<string> tooltip = () => {return "This is a tooltip, obviously.";};
                // herosMod.Call("AddSimpleButton", permissionName, texture, buttonClicked, groupUpdated, tooltip);
            }
        }

        // On mod load, get an instance to use for creating packets and create save directory
        public override void Load()
        {
			instance = this;
            if(Main.netMode == NetmodeID.Server)
            {
                Directory.CreateDirectory(Main.SavePath);
            }
        }
        public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
            VIRUSNetwork.HandlePacket(reader, whoAmI);
		}
    }
}

// TODO:
// Investigate unique identifiers for players
// Add per-world support in data 

//  Side Projects:
//  Consider tracking and updating player position
//  Add an integrated in-game menu that can access "deleted" items <--
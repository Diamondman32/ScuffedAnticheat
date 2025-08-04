using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using ScuffedAnticheatMod.Network;
using ScuffedAnticheatMod.UI;

namespace ScuffedAnticheatMod
{
    public class SAMPlayer : ModPlayer
    {
        public PlayerInventory oldInventory = new PlayerInventory("uninitialized");

        // When player enters world, tell server to check their inventory
        public override void OnEnterWorld(Player player)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                CheckInventory.SendPacket();
                CheckMods.SendPacket();
                RequestLocation.SendPacket();
            }
            ModContent.GetInstance<UIOverlay>().DisablePlayerList();
        }

        // Every update (60 fps?) checks inventory for modifications
        // TODO: Maybe reduce the amount of inv checks (config?)
        // TODO: Maybe add ON hooks in terraria instead
        public override void PostUpdate()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                if (oldInventory.playerName == "uninitialized")
                    oldInventory = new PlayerInventory(Main.LocalPlayer);
                UpdateItemSaveData.UpdateInventoryDifferences(Main.myPlayer, oldInventory); // oldInventory is modified in function
            }
        }
    }
}
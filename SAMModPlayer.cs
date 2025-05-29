using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using ScuffedAnticheatMod.Network;

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
            }
        }

        // Every update (60 fps?) checks inventory for modifications
        public override void PostUpdate()
        {
            if(Main.netMode == NetmodeID.MultiplayerClient)
            {
                if(oldInventory.playerName == "uninitialized")
                    oldInventory = new PlayerInventory(Main.LocalPlayer);
                UpdateSaveData.UpdateInventoryDifferences(Main.myPlayer, oldInventory); // oldInventory is modified in function
            }
        }
    }
}
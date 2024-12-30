using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using ScuffedAnticheatMod.Network;

namespace ScuffedAnticheatMod
{
    public class SAMPlayer : ModPlayer
	{
        public PlayerInventory oldInventory = new("uninitialized");

        // When player enters world, tell server to check their inventory
        public override void OnEnterWorld(Player player)
        {
            if(Main.netMode == NetmodeID.MultiplayerClient)
            {
                var packet = ScuffedAnticheatMod.instance.GetPacket();
                packet.Write((byte)MessageType.CheckMyInventory);
                packet.Send(255); // Send to server
            }
        }

        // Every update (60 fps?) checks inventory for modifications
        public override void PostUpdate()
        {
            if(Main.netMode == NetmodeID.MultiplayerClient)
            {
                if(oldInventory.name == "uninitialized")
                    oldInventory = new(Main.LocalPlayer);
                UpdateSaveData.UpdateInventoryDifferences(Main.LocalPlayer, oldInventory, out oldInventory);
            }
        }
    }
}
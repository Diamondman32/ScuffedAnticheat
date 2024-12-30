using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using SAM.Network;

namespace SAM
{
    public class SAMPlayer : ModPlayer
	{
        public PlayerInventory oldInventory = new("uninitialized");

        // When player enters world, tell server to check their inventory
        public override void OnEnterWorld(Player player)
        {
            if(Main.netMode == NetmodeID.MultiplayerClient)
            {
                var packet = SAM.instance.GetPacket();
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
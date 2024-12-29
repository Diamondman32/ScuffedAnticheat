using Terraria.ModLoader;
using Terraria;
using Terraria.ID;

namespace VIRUS
{
    public class VIRUSPlayer : ModPlayer
	{
        public PlayerInventory oldInventory = new("uninitialized");

        public override void OnEnterWorld(Player player)
        {
            if(Main.netMode == NetmodeID.MultiplayerClient)
            {
                var packet = VIRUS.instance.GetPacket();
                packet.Write((byte)MessageType.CheckInventory);
                packet.Send(255); // Send to server
            }
        }

        public override void PostUpdate()
        {
            if(Main.netMode == NetmodeID.MultiplayerClient)
            {
                if(oldInventory.name == "uninitialized")
                    oldInventory = new(Main.LocalPlayer);
                Network.UpdateInventory(Main.LocalPlayer, oldInventory, MessageType.UpdateItem, out oldInventory);
            }
        }
    }
}
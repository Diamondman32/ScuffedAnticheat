using Terraria.ModLoader;
using Terraria;

namespace VIRUS
{
    public class VIRUSPlayer : ModPlayer
	{
        public PlayerInventory oldInventory = new("uninitialized");

        public override void OnEnterWorld(Player player)
        {
            var packet = VIRUS.instance.GetPacket();
            packet.Write((byte)MessageType.CheckInventory);
            packet.Send(255); // Send to server
        }

        public override void PostUpdate()
        {
            if(oldInventory.name == "uninitialized") oldInventory = new(Main.LocalPlayer);
            Network.UpdateInventory(Main.LocalPlayer, oldInventory, MessageType.UpdateItem, out PlayerInventory newInventory);
            oldInventory = newInventory;
        }
    }
}
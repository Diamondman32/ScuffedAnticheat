using Terraria.ModLoader;
using Terraria;
using Terraria.Chat;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace VIRUS
{
    public class VIRUSPlayer : ModPlayer
	{
        public PlayerInventory oldInventory = new(Main.LocalPlayer);

        public override void OnEnterWorld(Player player)
        {
            var packet = VIRUS.instance.GetPacket();
            packet.Write((byte)MessageType.CheckInventory);
            packet.Send(255); // Send to server
        }

        public override void PostUpdate()
        {
            Network.UpdateInventory(Main.LocalPlayer, oldInventory, MessageType.UpdateItem, out PlayerInventory newInventory);
            oldInventory = newInventory;
        }
    }
}
using Terraria.ModLoader;
using Terraria;

namespace VIRUS
{
    public class VIRUSPlayer : ModPlayer
	{
        // Adds a call to checkInventory
        public override void PlayerConnect(Player player)
        {
			base.PlayerConnect(player);
            if(player.whoAmI == Main.myPlayer) SendPacket(player, MessageType.CheckInventory);
        }

        // Adds a call to updateInventory
        public override void PlayerDisconnect(Player player)
        {
            base.PlayerDisconnect(player);
            if(player.whoAmI == Main.myPlayer) SendPacket(player, MessageType.UpdateInventory);
        }

        // Sends the inventory to the server with param check vs update
        public static void SendPacket(Player player, MessageType type)
        {
            // Create packet
            var packet = VIRUS.instance.GetPacket();

            // Add relevant identifiers
            packet.Write((byte)type);
            packet.Write((byte)player.whoAmI);

            // TODO: Check bank (Piggy Bank), bank2 (Safe), bank3 (Defender's Forge), and bank4 (Void Vault)

            // Add all inventory slots: [0-9 hold hotbar items, 10-49 hold remaining main inventory, 50-53 are coin slots, 54-57 are ammo slots, and index 58 is the mouse slot]
            for(int i=0;i<59;i++)
                Terraria.ModLoader.IO.ItemIO.Send(player.inventory[i], packet, true);

            // Add all nonmodded armor/accessory slots: [Indexes 0-2 hold head, chest, and legs armor while 10-12 hold the respective social armor items. Indexes 3-9 hold the accessories and 13-19 hold the social accessory items.]
            for(int i=0;i<20;i++)
                Terraria.ModLoader.IO.ItemIO.Send(player.armor[i], packet);

            // Add all nonmodded dye slots: [Indexes 0-2 hold armor, 3-9 hold accessories]
            for(int i=0;i<20;i++)
                Terraria.ModLoader.IO.ItemIO.Send(player.dye[i], packet);

            // TODO: Add Modded accesory check
            // if(ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            // {
            //     var calamityPlayer = Player.GetModPlayer()
            //     LoadData();
            //     calamity.
            // }

            packet.Send(255); // Send to server
            return;
        }
    }
}
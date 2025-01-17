using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.IO;


namespace ScuffedAnticheatMod.Network
{
    public class ModifyPlayerData : SAMNetwork
    {
            /* CLIENT & SERVER */
        // Receives packets and replaces designated item with the correct item
        // IF NETMODEID IS SERVER, FORWARD TO TARGET CLIENT
        public static void ProcessModifyItem(ref BinaryReader reader)
        {
            // SERVER
            if(Main.netMode == NetmodeID.Server)
            {
                int targetNum = reader.ReadByte();

                var packet = ScuffedAnticheatMod.instance.GetPacket();
                packet.Write((byte)MessageType.ReplaceItem);

                byte category = reader.ReadByte();
                packet.Write(category);
                if((ItemCategory)category != ItemCategory.FindFirstOpenInv)
                    packet.Write(reader.ReadByte());

                ItemIO.Send(ItemIO.Receive(reader, true, true), packet, true, true);
                packet.Send(targetNum);
                return;
            }

            // CLIENT
            ItemCategory itemCategory = (ItemCategory)reader.ReadByte();

            int itemIndex = 10000;
            if(itemCategory != ItemCategory.FindFirstOpenInv)
                itemIndex = reader.ReadByte();
            
            Item newItem = ItemIO.Receive(reader, true, true);

            switch(itemCategory)
            {
                case ItemCategory.Inventory:
                    Main.LocalPlayer.inventory[itemIndex] = newItem;
                    break;
                case ItemCategory.Bank1:
                    Main.LocalPlayer.bank.item[itemIndex] = newItem;
                    break;
                case ItemCategory.Bank2:
                    Main.LocalPlayer.bank2.item[itemIndex] = newItem;
                    break;
                case ItemCategory.Bank3:
                    Main.LocalPlayer.bank3.item[itemIndex] = newItem;
                    break;
                case ItemCategory.Bank4:
                    Main.LocalPlayer.bank4.item[itemIndex] = newItem;
                    break;
                case ItemCategory.Armor:
                    Main.LocalPlayer.armor[itemIndex] = newItem;
                    break;
                case ItemCategory.Dye:
                    Main.LocalPlayer.dye[itemIndex] = newItem;
                    break;
                case ItemCategory.MiscEquips:
                    Main.LocalPlayer.miscEquips[itemIndex] = newItem;
                    break;
                case ItemCategory.MiscDyes:
                    Main.LocalPlayer.miscDyes[itemIndex] = newItem;
                    break;
                case ItemCategory.Trash:
                    Main.LocalPlayer.trashItem = newItem;
                    break;
                case ItemCategory.FindFirstOpenInv:
                    ReplaceFirstOpenSlot(newItem);
                    break;
            }
        }

        /* CLIENT */
        public static void ReplaceFirstOpenSlot(Item newItem)
        {
            for(int i = 0; i < Main.LocalPlayer.inventory.Length; i++)
                if(Main.LocalPlayer.inventory[i].IsAir)
                {
                    Main.LocalPlayer.inventory[i] = newItem;
                    return;
                }
            
            // Inventory is full :( so drop on ground
            Main.LocalPlayer.QuickSpawnItem(newItem.GetSource_Misc("inv full"), newItem);
        }
    }
}
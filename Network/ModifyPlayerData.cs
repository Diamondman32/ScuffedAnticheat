using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.IO;


namespace ScuffedAnticheatMod.Network
{
    public class ModifyPlayerData : SAMNetwork
    {
        // Receives packets and replaces designated item with the correct item
        // If netModeID is server, then it is forwarded to specified client
        public static void ProcessRequest(ref BinaryReader reader)
        {
            // SERVER
            if(Main.netMode == NetmodeID.Server)
            {
                int targetNum = reader.ReadByte();

                var packet = ScuffedAnticheatMod.instance.GetPacket();
                packet.Write((byte)MessageType.ModifyPlayerData);
                packet.Write(reader.ReadByte());
                packet.Write(reader.ReadByte());
                ItemIO.Send(ItemIO.Receive(reader, true, true), packet, true, true);

                packet.Send(targetNum);
                return;
            }

            // CLIENT
            ItemCategory itemCategory = (ItemCategory)reader.ReadByte();
            int itemIndex = reader.ReadByte();
            Item newItem = ItemIO.Receive(reader, true, true);

            switch(itemCategory)
            {
                case ItemCategory.Inventory:
                    if (itemIndex == 58)
                        OnEnterWorld.SetMouseItem(newItem);
                    else
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
        // Sends packet with desired item to return to player
        public static void ReturnItemToPlayer(Item item, int targetNum)
        {
            if (targetNum == Main.myPlayer)
                ReplaceFirstOpenSlot(item);
            else
            {
                var packet = ScuffedAnticheatMod.instance.GetPacket();
                packet.Write((byte)MessageType.ModifyPlayerData);
                packet.Write((byte)targetNum);
                packet.Write((byte)ItemCategory.FindFirstOpenInv);
                packet.Write((byte)0); // not needed
                ItemIO.Send(item, packet, true, true);
                packet.Send();
            }
        }

        // Puts item in first air slot. If inv full drop item on ground
        public static void ReplaceFirstOpenSlot(Item newItem)
        {
            for (int i = 0; i < Main.LocalPlayer.inventory.Length; i++)
                if (Main.LocalPlayer.inventory[i].IsAir)
                {
                    Main.LocalPlayer.inventory[i] = newItem;
                    return;
                }

            Main.LocalPlayer.QuickSpawnItem(newItem.GetSource_Misc("inv full"), newItem);
        }
    }
}
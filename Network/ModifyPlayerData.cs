using System.IO;
using Terraria;

namespace VIRUS.Network
{
    public class ModifyPlayerData : VIRUSNetwork
    {
        // Receives packets and replaces designated item with the correct item
        public static void ProcessModifyItem(ref BinaryReader reader)
        {
            int playerNum = reader.ReadByte();
            if(Main.myPlayer == playerNum)
            {
                ItemCategory itemCategory = (ItemCategory)reader.ReadByte();
                int itemIndex = reader.ReadByte();
                Item newItem = Terraria.ModLoader.IO.ItemIO.Receive(reader, true, true);

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
                }
            }
        }
    }
}
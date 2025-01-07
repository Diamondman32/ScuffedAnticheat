using System.IO;
using Terraria;

namespace ScuffedAnticheatMod.Network
{
    public class ModifyPlayerData : SAMNetwork
    {
            /* CLIENT */
        // Receives packets and replaces designated item with the correct item
        public static void ProcessModifyItem(ref BinaryReader reader)
        {
            ItemCategory itemCategory = (ItemCategory)reader.ReadByte();
            int itemIndex = reader.ReadByte();
            Item newItem = Terraria.ModLoader.IO.ItemIO.Receive(reader, true, true);

            // So hover item isn't lost
            if(itemCategory == ItemCategory.Inventory && itemIndex == 58)
                Main.LocalPlayer.ToggleInv();

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
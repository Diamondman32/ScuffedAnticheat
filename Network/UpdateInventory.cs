using Terraria.ID;
using Terraria;

namespace ScuffedAnticheatMod.Network
{
    public class UpdateInventory : SAMNetwork
    {
        /*  SERVER  */
        // Called with hook in MessageBuffer case 5 (SyncEquipment)
        public static void OnInventoryChange(int playerID, int slotType)
        {
            Player player = Main.player[playerID];
            PlayerInventory savedInventory = playerInventories[playerID];
            int slotIndex;

            if (slotType >= PlayerItemSlotID.Bank4_0)
            {
                slotIndex = slotType - PlayerItemSlotID.Bank4_0;
                savedInventory.bank4[slotIndex] = new(player.bank4.item[slotIndex]);
                PlayerData.UpsertItem(player.name, guids[player.whoAmI], new EzItem(player.bank4.item[slotIndex]), slotType);
            }
            else if (slotType >= PlayerItemSlotID.Bank3_0)
            {
                slotIndex = slotType - PlayerItemSlotID.Bank3_0;
                savedInventory.bank3[slotIndex] = new(player.bank3.item[slotIndex]);
                PlayerData.UpsertItem(player.name, guids[player.whoAmI], new EzItem(player.bank3.item[slotIndex]), slotType);
            }
            else if (slotType >= PlayerItemSlotID.TrashItem)
            {
                slotIndex = slotType - PlayerItemSlotID.TrashItem;
                savedInventory.trash[slotIndex] = new(player.trashItem);
                PlayerData.UpsertItem(player.name, guids[player.whoAmI], new EzItem(player.trashItem), slotType);
            }
            else if (slotType >= PlayerItemSlotID.Bank2_0)
            {
                slotIndex = slotType - PlayerItemSlotID.Bank2_0;
                savedInventory.bank2[slotIndex] = new(player.bank2.item[slotIndex]);
                PlayerData.UpsertItem(player.name, guids[player.whoAmI], new EzItem(player.bank2.item[slotIndex]), slotType);
            }
            else if (slotType >= PlayerItemSlotID.Bank1_0)
            {
                slotIndex = slotType - PlayerItemSlotID.Bank1_0;
                savedInventory.bank1[slotIndex] = new(player.bank.item[slotIndex]);
                PlayerData.UpsertItem(player.name, guids[player.whoAmI], new EzItem(player.bank.item[slotIndex]), slotType);
            }
            else if (slotType >= PlayerItemSlotID.MiscDye0)
            {
                slotIndex = slotType - PlayerItemSlotID.MiscDye0;
                savedInventory.miscDyes[slotIndex] = new(player.miscDyes[slotIndex]);
                PlayerData.UpsertItem(player.name, guids[player.whoAmI], new EzItem(player.miscDyes[slotIndex]), slotType);
            }
            else if (slotType >= PlayerItemSlotID.Misc0)
            {
                slotIndex = slotType - PlayerItemSlotID.Misc0;
                savedInventory.miscEquips[slotIndex] = new(player.miscEquips[slotIndex]);
                PlayerData.UpsertItem(player.name, guids[player.whoAmI], new EzItem(player.miscEquips[slotIndex]), slotType);
            }
            else if (slotType >= PlayerItemSlotID.Dye0)
            {
                slotIndex = slotType - PlayerItemSlotID.Dye0;
                savedInventory.dye[slotIndex] = new(player.dye[slotIndex]);
                PlayerData.UpsertItem(player.name, guids[player.whoAmI], new EzItem(player.dye[slotIndex]), slotType);
            }
            else if (slotType >= PlayerItemSlotID.Armor0)
            {
                slotIndex = slotType - PlayerItemSlotID.Armor0;
                savedInventory.armor[slotIndex] = new(player.armor[slotIndex]);
                PlayerData.UpsertItem(player.name, guids[player.whoAmI], new EzItem(player.armor[slotIndex]), slotType);
            }
            else
            {
                slotIndex = slotType - PlayerItemSlotID.Inventory0;
                savedInventory.inventory[slotIndex] = new(player.inventory[slotIndex]);
                PlayerData.UpsertItem(player.name, guids[player.whoAmI], new EzItem(player.inventory[slotIndex]), slotType);
            }
        }

        /*  CLIENT  */
        public static void SendPacket()
        {
            var packet = ScuffedAnticheatMod.instance.GetPacket();
            packet.Write((byte)MessageType.CheckMods);

            packet.Send(255); // Send to server
        }
    }
}
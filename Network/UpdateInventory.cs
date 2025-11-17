using System;
using System.IO;
using Terraria.ID;
using Terraria;
using Terraria.Localization;
using System.Collections.Generic;
using Terraria.Chat;
using Microsoft.Xna.Framework;
using System.Threading;

namespace ScuffedAnticheatMod.Network
{
    public class UpdateInventory : SAMNetwork
    {
        /*  SERVER  */
        // Called with hook in MessageBuffer case 5 (SyncEquipment) 
        public static void OnInventoryChange(int playerID, int slotType)
        {
            // if (!Main.player[playerID].active)
            //     return;

            Player player = Main.player[playerID];
            PlayerInventory savedInventory = FindPlayerInventory(playerID);
            int slotIndex;

            if (slotType >= PlayerItemSlotID.Bank4_0)
            {
                slotIndex = slotType - PlayerItemSlotID.Bank4_0;
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(
                    $"Changing saved bank4 ({slotIndex}) from {savedInventory.bank4[slotIndex].itemName} to {player.bank4.item[slotIndex].Name}"
                    ), Color.AliceBlue);
                savedInventory.bank4[slotIndex] = new(player.bank4.item[slotIndex]);
            }
            else if (slotType >= PlayerItemSlotID.Bank3_0)
            {
                slotIndex = slotType - PlayerItemSlotID.Bank3_0;
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(
                    $"Changing saved bank3 ({slotIndex}) from {savedInventory.bank3[slotIndex].itemName} to {player.bank3.item[slotIndex].Name}"
                    ), Color.AliceBlue);
                savedInventory.bank3[slotIndex] = new(player.bank3.item[slotIndex]);
            }
            else if (slotType >= PlayerItemSlotID.TrashItem)
            {
                slotIndex = slotType - PlayerItemSlotID.TrashItem;
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(
                    $"Changing saved trash ({slotIndex}) from {savedInventory.trash[slotIndex].itemName} to {player.trashItem.Name}"
                    ), Color.AliceBlue);
                savedInventory.trash[slotIndex] = new(player.trashItem);
            }
            else if (slotType >= PlayerItemSlotID.Bank2_0)
            {
                slotIndex = slotType - PlayerItemSlotID.Bank2_0;
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(
                    $"Changing saved bank2 ({slotIndex}) from {savedInventory.bank2[slotIndex].itemName} to {player.bank2.item[slotIndex].Name}"
                    ), Color.AliceBlue);
                savedInventory.bank2[slotIndex] = new(player.bank2.item[slotIndex]);
            }
            else if (slotType >= PlayerItemSlotID.Bank1_0)
            {
                slotIndex = slotType - PlayerItemSlotID.Bank1_0;
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(
                    $"Changing saved bank1 ({slotIndex}) from {savedInventory.bank1[slotIndex].itemName} to {player.bank.item[slotIndex].Name}"
                    ), Color.AliceBlue);
                savedInventory.bank1[slotIndex] = new(player.bank.item[slotIndex]);
            }
            else if (slotType >= PlayerItemSlotID.MiscDye0)
            {
                slotIndex = slotType - PlayerItemSlotID.MiscDye0;
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(
                    $"Changing saved miscDyes ({slotIndex}) from {savedInventory.miscDyes[slotIndex].itemName} to {player.miscDyes[slotIndex].Name}"
                    ), Color.AliceBlue);
                savedInventory.miscDyes[slotIndex] = new(player.miscDyes[slotIndex]);
            }
            else if (slotType >= PlayerItemSlotID.Misc0)
            {
                slotIndex = slotType - PlayerItemSlotID.Misc0;
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(
                    $"Changing saved miscEquips ({slotIndex}) from {savedInventory.miscEquips[slotIndex].itemName} to {player.miscEquips[slotIndex].Name}"
                    ), Color.AliceBlue);
                savedInventory.miscEquips[slotIndex] = new(player.miscEquips[slotIndex]);
            }
            else if (slotType >= PlayerItemSlotID.Dye0)
            {
                slotIndex = slotType - PlayerItemSlotID.Dye0;
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(
                    $"Changing saved dye ({slotIndex}) from {savedInventory.dye[slotIndex].itemName} to {player.dye[slotIndex].Name}"
                    ), Color.AliceBlue);
                savedInventory.dye[slotIndex] = new(player.dye[slotIndex]);
            }
            else if (slotType >= PlayerItemSlotID.Armor0)
            {
                slotIndex = slotType - PlayerItemSlotID.Armor0;
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(
                    $"Changing saved armor ({slotIndex}) from {savedInventory.armor[slotIndex].itemName} to {player.armor[slotIndex].Name}"
                    ), Color.AliceBlue);
                savedInventory.armor[slotIndex] = new(player.armor[slotIndex]);
            }
            else
            {
                slotIndex = slotType - PlayerItemSlotID.Inventory0;
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(
                    $"Changing saved inventory ({slotIndex}) from {savedInventory.inventory[slotIndex].itemName} to {player.inventory[slotIndex].Name}"
                    ), Color.AliceBlue);
                savedInventory.inventory[slotIndex] = new(player.inventory[slotIndex]);
            }

            // TODO: Put on timer
            // UpdateItemSaveData.Serialize();
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
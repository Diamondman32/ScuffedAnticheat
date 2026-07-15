using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.Localization;
using System.Collections.Generic;
using MonoMod.Utils;

namespace ScuffedAnticheatMod.Network
{
    public class CheckInventory : SAMNetwork
    {
        /*  SERVER  */
        // Checks entire inventory against save data. Sends ReplaceItem packet if incorrect and adds the "deleted" item to its own save data
        public static void ProcessRequest(int playerNumber)
        {
            Player player = Main.player[playerNumber];
            PlayerInventory playerInventory = new(player);
            PlayerInventory savedInventory = playerInventories[playerNumber];

            List<EzItem> itemsDeleted = new List<EzItem>();
            List<EzItem> itemsAdded = new List<EzItem>();

            for (int i = 0; i < playerInventory.inventory.Length; i++)
                if (!IsIdentical(playerInventory.inventory[i], savedInventory.inventory[i]))
                {
                    itemsAdded.Add(savedInventory.inventory[i]);
                    itemsDeleted.Add(playerInventory.inventory[i]);
                    SendModifyPacket(playerNumber, ItemCategory.Inventory, i, savedInventory.inventory[i]);
                }

            for (int i = 0; i < playerInventory.bank1.Length; i++)
                if (!IsIdentical(playerInventory.bank1[i], savedInventory.bank1[i]))
                {
                    itemsAdded.Add(savedInventory.bank1[i]);
                    itemsDeleted.Add(playerInventory.bank1[i]);
                    SendModifyPacket(playerNumber, ItemCategory.Bank1, i, savedInventory.bank1[i]);
                }

            for (int i = 0; i < playerInventory.bank2.Length; i++)
                if (!IsIdentical(playerInventory.bank2[i], savedInventory.bank2[i]))
                {
                    itemsAdded.Add(savedInventory.bank2[i]);
                    itemsDeleted.Add(playerInventory.bank2[i]);
                    SendModifyPacket(playerNumber, ItemCategory.Bank2, i, savedInventory.bank2[i]);
                }

            for (int i = 0; i < playerInventory.bank3.Length; i++)
                if (!IsIdentical(playerInventory.bank3[i], savedInventory.bank3[i]))
                {
                    itemsAdded.Add(savedInventory.bank3[i]);
                    itemsDeleted.Add(playerInventory.bank3[i]);
                    SendModifyPacket(playerNumber, ItemCategory.Bank3, i, savedInventory.bank3[i]);
                }

            for (int i = 0; i < playerInventory.bank4.Length; i++)
                if (!IsIdentical(playerInventory.bank4[i], savedInventory.bank4[i]))
                {
                    itemsAdded.Add(savedInventory.bank4[i]);
                    itemsDeleted.Add(playerInventory.bank4[i]);
                    SendModifyPacket(playerNumber, ItemCategory.Bank4, i, savedInventory.bank4[i]);
                }

            for (int i = 0; i < playerInventory.armor.Length; i++)
                if (!IsIdentical(playerInventory.armor[i], savedInventory.armor[i]))
                {
                    itemsAdded.Add(savedInventory.armor[i]);
                    itemsDeleted.Add(playerInventory.armor[i]);
                    SendModifyPacket(playerNumber, ItemCategory.Armor, i, savedInventory.armor[i]);
                }

            for (int i = 0; i < playerInventory.dye.Length; i++)
                if (!IsIdentical(playerInventory.dye[i], savedInventory.dye[i]))
                {
                    itemsAdded.Add(savedInventory.dye[i]);
                    itemsDeleted.Add(playerInventory.dye[i]);
                    SendModifyPacket(playerNumber, ItemCategory.Dye, i, savedInventory.dye[i]);
                }

            for (int i = 0; i < playerInventory.miscEquips.Length; i++)
                if (!IsIdentical(playerInventory.miscEquips[i], savedInventory.miscEquips[i]))
                {
                    itemsAdded.Add(savedInventory.miscEquips[i]);
                    itemsDeleted.Add(playerInventory.miscEquips[i]);
                    SendModifyPacket(playerNumber, ItemCategory.MiscEquips, i, savedInventory.miscEquips[i]);
                }

            for (int i = 0; i < playerInventory.miscDyes.Length; i++)
                if (!IsIdentical(playerInventory.miscDyes[i], savedInventory.miscDyes[i]))
                {
                    itemsAdded.Add(savedInventory.miscDyes[i]);
                    itemsDeleted.Add(playerInventory.miscDyes[i]);
                    SendModifyPacket(playerNumber, ItemCategory.MiscDyes, i, savedInventory.miscDyes[i]);
                }

            if (!IsIdentical(playerInventory.trash[0], savedInventory.trash[0]))
            {
                itemsAdded.Add(savedInventory.trash[0]);
                itemsDeleted.Add(playerInventory.trash[0]);
                SendModifyPacket(playerNumber, ItemCategory.Trash, 0, savedInventory.trash[0]);
            }

            RemoveIdenticalIndexes(itemsDeleted, itemsAdded);
            AddToDiscardPile(player, itemsDeleted);
        }

        // Removes each Item in the second list nullifys at max one identical item in the first list
        private static void RemoveIdenticalIndexes(List<EzItem> sortedList, List<EzItem> otherList)
        {
            for(int i = 0; i < otherList.Count; i++)
            {
                int sortedIndex = sortedList.FindIndex(x => x.type == otherList[i].type);
                if(sortedIndex != -1)
                {
                    sortedList.RemoveAt(sortedIndex);
                    otherList.RemoveAt(i);
                    i--;
                }
            }
        }
        
        // Adds item to its own json file for storage. Displays notification in chat
        private static void AddToDiscardPile(Player player, List<EzItem> items)
        {
            for (int i = 0; i < items.Count; ++i)
                if (items[i].type == 0)
                    items.RemoveAt(i);
            if(items.Count == 0)
                return;

            string message = $"[ScuffedAnticheatMod] Player item mismatch with server.\n - Player: {player.name}\n - New Items: ";
            foreach(EzItem item in items)
            {
                if(item.type == 0)
                    continue;

                deletedItems.Add(new DeletedItem(item, player.name, guids[player.whoAmI]));
                message += $"{Lang.GetItemNameValue(item.type)} ({item.stack}), ";
            }
            if (message.Length != 0)
                message = message[..^2];

            deletedItems.ForEach(PlayerData.AddDeletedItem);

            // Send announcement to entire server
            // TODO: config
            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(message), Color.Purple);

            // Notify player when they are done joining
            SendDeletedInfoPacket(player.whoAmI, message);
        }

        // Sends packet to fix client inventory item
        protected static void SendModifyPacket(int target, ItemCategory category, int index, EzItem newItem)
        {
            var packet = ScuffedAnticheatMod.instance.GetPacket();
            packet.Write((byte)MessageType.ModifyPlayerData);
            packet.Write((byte)category);
            packet.Write((byte)index);
            Terraria.ModLoader.IO.ItemIO.Send(newItem.GetClone(), packet, true, true);
            packet.Send(target);
        }

        // Sends information about all removed items to player
        protected static void SendDeletedInfoPacket(int target, string message)
        {
            var packet = ScuffedAnticheatMod.instance.GetPacket();
            packet.Write((byte)MessageType.DeletedItemNotification);
            packet.WriteNullTerminatedString(message);
            packet.Send(target);
        }
    }
}
using System.IO;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Terraria;
using Terraria.Chat;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace ScuffedAnticheatMod.Network
{
    public class UpdateItemSaveData : SAMNetwork
    {
        /*  SERVER  */
        // Reads incoming UpdateSaveData packets and updates save data accordingly
        public static void ProcessRequest(ref BinaryReader reader, int playerNum)
        {
            Player player = (Player)Main.player[playerNum].Clone();
            ItemCategory itemCategory = (ItemCategory)reader.ReadByte();
            int itemIndex = reader.ReadByte();
            Item newItem = ItemIO.Receive(reader, true, true);

            // Get Index of player inventory if it exists
            int inventoryIndex = playerInventories.Count;
            for (int i = 0; i < playerInventories.Count; i++)
                if (playerInventories[i].playerName == player.name && playerInventories[i].guid == guids[playerNum] && playerInventories[i].worldID == Main.worldID)
                    inventoryIndex = i;

            IsAnythingNull();

            // if valid player found, add new index. Otherwise, modify existing inventory
            if (inventoryIndex == playerInventories.Count)
                throw new System.Exception("didn't find player inventory");
            else
            {
                switch (itemCategory)
                {
                    case ItemCategory.Inventory:
                        playerInventories[inventoryIndex].inventory[itemIndex] = new(newItem);
                        break;
                    case ItemCategory.Bank1:
                        playerInventories[inventoryIndex].bank1[itemIndex] = new(newItem);
                        break;
                    case ItemCategory.Bank2:
                        playerInventories[inventoryIndex].bank2[itemIndex] = new(newItem);
                        break;
                    case ItemCategory.Bank3:
                        playerInventories[inventoryIndex].bank3[itemIndex] = new(newItem);
                        break;
                    case ItemCategory.Bank4:
                        playerInventories[inventoryIndex].bank4[itemIndex] = new(newItem);
                        break;
                    case ItemCategory.Armor:
                        playerInventories[inventoryIndex].armor[itemIndex] = new(newItem);
                        break;
                    case ItemCategory.Dye:
                        playerInventories[inventoryIndex].dye[itemIndex] = new(newItem);
                        break;
                    case ItemCategory.MiscEquips:
                        playerInventories[inventoryIndex].miscEquips[itemIndex] = new(newItem);
                        break;
                    case ItemCategory.MiscDyes:
                        playerInventories[inventoryIndex].miscDyes[itemIndex] = new(newItem);
                        break;
                    case ItemCategory.Trash:
                        playerInventories[inventoryIndex].trash[0] = new(newItem);
                        break;
                }
            }

            // Serialize into json file
            string json = JsonConvert.SerializeObject(playerInventories);
            using StreamWriter outputFile = new(CharacterDataPath);
            outputFile.WriteLine(json);
            outputFile.Close();
        }

        /*  CLIENT  */
        // Check every inventory slot for differences. If so, send packet to server to update its save data.
        public static void UpdateInventoryDifferences(int whoAmI, PlayerInventory savedInventory) // Assuming savedInventory is used as a reference
        {
            Player player = Main.player[whoAmI];
            PlayerInventory playerInventory = new(player);

            for (int i = 0; i < playerInventory.inventory.Length; i++)
                if (!IsIdentical(playerInventory.inventory[i], savedInventory.inventory[i]))
                {
                    SendPacket(ItemCategory.Inventory, i, playerInventory.inventory[i]);
                    savedInventory.inventory[i] = playerInventory.inventory[i];
                }

            for (int i = 0; i < playerInventory.bank1.Length; i++)
                if (!IsIdentical(playerInventory.bank1[i], savedInventory.bank1[i]))
                {
                    SendPacket(ItemCategory.Bank1, i, playerInventory.bank1[i]);
                    savedInventory.bank1[i] = playerInventory.bank1[i];
                }

            for (int i = 0; i < playerInventory.bank2.Length; i++)
                if (!IsIdentical(playerInventory.bank2[i], savedInventory.bank2[i]))
                {
                    SendPacket(ItemCategory.Bank2, i, playerInventory.bank2[i]);
                    savedInventory.bank2[i] = playerInventory.bank2[i];
                }

            for (int i = 0; i < playerInventory.bank3.Length; i++)
                if (!IsIdentical(playerInventory.bank3[i], savedInventory.bank3[i]))
                {
                    SendPacket(ItemCategory.Bank3, i, playerInventory.bank3[i]);
                    savedInventory.bank3[i] = playerInventory.bank3[i];
                }

            for (int i = 0; i < playerInventory.bank4.Length; i++)
                if (!IsIdentical(playerInventory.bank4[i], savedInventory.bank4[i]))
                {
                    SendPacket(ItemCategory.Bank4, i, playerInventory.bank4[i]);
                    savedInventory.bank4[i] = playerInventory.bank4[i];
                }

            for (int i = 0; i < playerInventory.armor.Length; i++)
                if (!IsIdentical(playerInventory.armor[i], savedInventory.armor[i]))
                {
                    SendPacket(ItemCategory.Armor, i, playerInventory.armor[i]);
                    savedInventory.armor[i] = playerInventory.armor[i];
                }

            for (int i = 0; i < playerInventory.dye.Length; i++)
                if (!IsIdentical(playerInventory.dye[i], savedInventory.dye[i]))
                {
                    SendPacket(ItemCategory.Dye, i, playerInventory.dye[i]);
                    savedInventory.dye[i] = playerInventory.dye[i];
                }

            for (int i = 0; i < playerInventory.miscEquips.Length; i++)
                if (!IsIdentical(playerInventory.miscEquips[i], savedInventory.miscEquips[i]))
                {
                    SendPacket(ItemCategory.MiscEquips, i, playerInventory.miscEquips[i]);
                    savedInventory.miscEquips[i] = playerInventory.miscEquips[i];
                }

            for (int i = 0; i < playerInventory.miscDyes.Length; i++)
                if (!IsIdentical(playerInventory.miscDyes[i], savedInventory.miscDyes[i]))
                {
                    SendPacket(ItemCategory.MiscDyes, i, playerInventory.miscDyes[i]);
                    savedInventory.miscDyes[i] = playerInventory.miscDyes[i];
                }

            if (!IsIdentical(playerInventory.trash[0], savedInventory.trash[0]))
            {
                SendPacket(ItemCategory.Trash, 0, playerInventory.trash[0]);
                savedInventory.trash[0] = playerInventory.trash[0];
            }
        }

        // Sends packet to update server record of player inventory
        private static void SendPacket(ItemCategory category, int index, EzItem newItem)
        {
            var packet = ScuffedAnticheatMod.instance.GetPacket();
            packet.Write((byte)MessageType.UpdateSaveData);
            packet.Write((byte)category);
            packet.Write((byte)index);
            ItemIO.Send(newItem.GetClone(), packet, true, true);
            packet.Send(255); // Send to server
        }
    }
    public class UpdateDeletedItemSaveData : SAMNetwork
    {
        /*  SERVER  */
        // Removes matching player-item from saved items when instructed by client
        public static void ProcessRequest(ref BinaryReader reader, int senderNum)
        {
            int targetNum = reader.ReadByte();
            Item item = ItemIO.Receive(reader, true, true);
            string playerName = Main.player[targetNum].name;

            int index = deletedItems.FindIndex(x => x.owner == playerName && x.guid == guids[targetNum] && x.worldID == Main.worldID && IsIdentical(new(item), x.item));
            if (index != -1)
                deletedItems.RemoveAt(index);
            else
            {
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Deleted item removal attempt failed."), Color.Red);
                ScuffedAnticheatMod.instance.Logger.Error($"Deleted item removal attempt failed. Owner: {playerName}, Item: {item.Name}, guid: {guids[targetNum]}");
            }

            string json = JsonConvert.SerializeObject(deletedItems);
            using StreamWriter outputFile = new(DiscardItemDataPath);
            outputFile.WriteLine(json);
            outputFile.Close();

            SyncDeletedItems.SyncClients(senderNum);
        }

        /*  CLIENT  */
        // Remove matching player-item from saved items and tells server to do the same
        public static void DeleteItem(Item item, int targetNum)
        {
            ReceiveDeletedItems.RemoveElement(item);

            var packet = ScuffedAnticheatMod.instance.GetPacket();
            packet.Write((byte)MessageType.UpdateDeletedItemSaveData);
            packet.Write((byte)targetNum);
            ItemIO.Send(item, packet, true, true);
            packet.Send();
        }
    }
}
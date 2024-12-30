using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Terraria;

namespace ScuffedAnticheatMod.Network
{
    public class UpdateSaveData : SAMNetwork
    {
        // Reads incoming UpdateSaveData packets and updates save data accordingly
        public static void ProcessUpdateInventory(ref BinaryReader reader)
        {
            int playerNum = reader.ReadByte();
            Player player = (Player)Main.player[playerNum].Clone();
            ItemCategory itemCategory = (ItemCategory)reader.ReadByte();
            int itemIndex = reader.ReadByte();
            Item newItem = Terraria.ModLoader.IO.ItemIO.Receive(reader, true, true);
            List<PlayerInventory> savedInventories = Deserialize();

            // Get Index of player inventory if it exists
            int inventoryIndex = savedInventories.Count;
            for(int i=0;i<savedInventories.Count;i++)
                if(savedInventories[i].name == player.name)
                    inventoryIndex = i;

            // if valid player found, add new index. Otherwise, modify existing inventory
            if(inventoryIndex == savedInventories.Count)
                savedInventories.Add(new(player));
            else
            {
                switch(itemCategory)
                {
                    case ItemCategory.Inventory:
                        savedInventories[inventoryIndex].inventory[itemIndex] = new(newItem);
                        break;
                    case ItemCategory.Bank1:
                        savedInventories[inventoryIndex].bank1[itemIndex] = new(newItem);
                        break;
                    case ItemCategory.Bank2:
                        savedInventories[inventoryIndex].bank2[itemIndex] = new(newItem);
                        break;
                    case ItemCategory.Bank3:
                        savedInventories[inventoryIndex].bank3[itemIndex] = new(newItem);
                        break;
                    case ItemCategory.Bank4:
                        savedInventories[inventoryIndex].bank4[itemIndex] = new(newItem);
                        break;
                    case ItemCategory.Armor:
                        savedInventories[inventoryIndex].armor[itemIndex] = new(newItem);
                        break;
                    case ItemCategory.Dye:
                        savedInventories[inventoryIndex].dye[itemIndex] = new(newItem);
                        break;
                    case ItemCategory.MiscEquips:
                        savedInventories[inventoryIndex].miscEquips[itemIndex] = new(newItem);
                        break;
                    case ItemCategory.MiscDyes:
                        savedInventories[inventoryIndex].miscDyes[itemIndex] = new(newItem);
                        break;
                    case ItemCategory.Trash:
                        savedInventories[inventoryIndex].trash[0] = new(newItem);
                        break;
                }
            }

            // Serialize back into json file
            string json = JsonConvert.SerializeObject(savedInventories);
            using StreamWriter outputFile = new(CharacterDataPath);
            outputFile.WriteLine(json);
            outputFile.Close();
        }

        // Returns entire deserialized json data array
        private static List<PlayerInventory> Deserialize()
		{
            string json = null;
            if(File.Exists(CharacterDataPath))
            {
                using StreamReader r = new(CharacterDataPath);
                json = r.ReadToEnd();
                r.Close();
            }
            else
                File.Create(CharacterDataPath);
            return string.IsNullOrEmpty(json) ? new List<PlayerInventory>() : JsonConvert.DeserializeObject<List<PlayerInventory>>(json) ?? new List<PlayerInventory>();
        }

        // Summary: Check every inventory slot for differences. If so, send packet to server to update its save data.
        public static void UpdateInventoryDifferences(Player player, PlayerInventory savedInventory, out PlayerInventory newInventory)
        {
            PlayerInventory playerInventory = new(player);
            newInventory = savedInventory;

            for(int i=0;i<playerInventory.inventory.Length;i++)
                if(!IsIdentical(playerInventory.inventory[i], savedInventory.inventory[i]))
                {
                    SendPacket(player, MessageType.UpdateSaveData, ItemCategory.Inventory, i, playerInventory.inventory[i], true);
                    newInventory.inventory[i] = playerInventory.inventory[i]; // might be a race condition. shouldn't matter hopefully
                }
            
            for(int i=0;i<playerInventory.bank1.Length;i++)
                if(!IsIdentical(playerInventory.bank1[i], savedInventory.bank1[i]))
                {
                    SendPacket(player, MessageType.UpdateSaveData, ItemCategory.Bank1, i, playerInventory.bank1[i], true);
                    newInventory.bank1[i] = playerInventory.bank1[i]; // might be a race condition. shouldn't matter hopefully
                }
            
            for(int i=0;i<playerInventory.bank2.Length;i++)
                if(!IsIdentical(playerInventory.bank2[i], savedInventory.bank2[i]))
                {
                    SendPacket(player, MessageType.UpdateSaveData, ItemCategory.Bank2, i, playerInventory.bank2[i], true);
                    newInventory.bank2[i] = playerInventory.bank2[i]; // might be a race condition. shouldn't matter hopefully
                }
            
            for(int i=0;i<playerInventory.bank3.Length;i++)
                if(!IsIdentical(playerInventory.bank3[i], savedInventory.bank3[i]))
                {
                    SendPacket(player, MessageType.UpdateSaveData, ItemCategory.Bank3, i, playerInventory.bank3[i], true);
                    newInventory.bank3[i] = playerInventory.bank3[i]; // might be a race condition. shouldn't matter hopefully
                }
            
            for(int i=0;i<playerInventory.bank4.Length;i++)
                if(!IsIdentical(playerInventory.bank4[i], savedInventory.bank4[i]))
                {
                    SendPacket(player, MessageType.UpdateSaveData, ItemCategory.Bank4, i, playerInventory.bank4[i], true);
                    newInventory.bank4[i] = playerInventory.bank4[i]; // might be a race condition. shouldn't matter hopefully
                }
            
            for(int i=0;i<playerInventory.armor.Length;i++)
                if(!IsIdentical(playerInventory.armor[i], savedInventory.armor[i]))
                {
                    SendPacket(player, MessageType.UpdateSaveData, ItemCategory.Armor, i, playerInventory.armor[i], true);
                    newInventory.armor[i] = playerInventory.armor[i]; // might be a race condition. shouldn't matter hopefully
                }
            
            for(int i=0;i<playerInventory.dye.Length;i++)
                if(!IsIdentical(playerInventory.dye[i], savedInventory.dye[i]))
                {
                    SendPacket(player, MessageType.UpdateSaveData, ItemCategory.Dye, i, playerInventory.dye[i], true);
                    newInventory.dye[i] = playerInventory.dye[i]; // might be a race condition. shouldn't matter hopefully
                }
            
            for(int i=0;i<playerInventory.miscEquips.Length;i++)
                if(!IsIdentical(playerInventory.miscEquips[i], savedInventory.miscEquips[i]))
                {
                    SendPacket(player, MessageType.UpdateSaveData, ItemCategory.MiscEquips, i, playerInventory.miscEquips[i], true);
                    newInventory.miscEquips[i] = playerInventory.miscEquips[i]; // might be a race condition. shouldn't matter hopefully
                }
            
            for(int i=0;i<playerInventory.miscDyes.Length;i++)
                if(!IsIdentical(playerInventory.miscDyes[i], savedInventory.miscDyes[i]))
                {
                    SendPacket(player, MessageType.UpdateSaveData, ItemCategory.MiscDyes, i, playerInventory.miscDyes[i], true);
                    newInventory.miscDyes[i] = playerInventory.miscDyes[i]; // might be a race condition. shouldn't matter hopefully
                }

            if(!IsIdentical(playerInventory.trash[0], savedInventory.trash[0]))
            {
                SendPacket(player, MessageType.UpdateSaveData, ItemCategory.Trash, 0, playerInventory.trash[0], true);
                newInventory.trash[0] = playerInventory.trash[0]; // might be a race condition. shouldn't matter hopefully
            }
        }
    }
}
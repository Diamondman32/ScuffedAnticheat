using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;

namespace VIRUS.Network
{
    public class CheckInventory : VIRUSNetwork
    {
        // Checks entire inventory against json save data. Sends ReplaceItem packet if incorrect and adds the "deleted" item to its own save data
        public static void ProcessCheckInventory(int playerNumber)
        {
            Player player = Main.player[playerNumber];
            PlayerInventory savedInventory = Deserialize(Main.player[playerNumber].name);

            SearchInventory(player, savedInventory);
        }

        // Helper Method that searches inventory, sends packet, and saves delted item.
        public static void SearchInventory(Player player, PlayerInventory savedInventory)
        {
            PlayerInventory playerInventory = new(player);

            for(int i=0;i<playerInventory.inventory.Length;i++)
                if(!IsIdentical(playerInventory.inventory[i], savedInventory.inventory[i]))
                {
                    AddToDiscardPile(player, playerInventory.inventory[i]);
                    SendPacket(player, MessageType.ReplaceItem, ItemCategory.Inventory, i, savedInventory.inventory[i]);
                }
            
            for(int i=0;i<playerInventory.bank1.Length;i++)
                if(!IsIdentical(playerInventory.bank1[i], savedInventory.bank1[i]))
                {
                    AddToDiscardPile(player, playerInventory.bank1[i]);
                    SendPacket(player, MessageType.ReplaceItem, ItemCategory.Bank1, i, savedInventory.bank1[i]);
                }
            
            for(int i=0;i<playerInventory.bank2.Length;i++)
                if(!IsIdentical(playerInventory.bank2[i], savedInventory.bank2[i]))
                {
                    AddToDiscardPile(player, playerInventory.bank2[i]);
                    SendPacket(player, MessageType.ReplaceItem, ItemCategory.Bank2, i, savedInventory.bank2[i]);
                }
            
            for(int i=0;i<playerInventory.bank3.Length;i++)
                if(!IsIdentical(playerInventory.bank3[i], savedInventory.bank3[i]))
                {
                    AddToDiscardPile(player, playerInventory.bank3[i]);
                    SendPacket(player, MessageType.ReplaceItem, ItemCategory.Bank3, i, savedInventory.bank3[i]);
                }
            
            for(int i=0;i<playerInventory.bank4.Length;i++)
                if(!IsIdentical(playerInventory.bank4[i], savedInventory.bank4[i]))
                {
                    AddToDiscardPile(player, playerInventory.bank4[i]);
                    SendPacket(player, MessageType.ReplaceItem, ItemCategory.Bank4, i, savedInventory.bank4[i]);
                }
            
            for(int i=0;i<playerInventory.armor.Length;i++)
                if(!IsIdentical(playerInventory.armor[i], savedInventory.armor[i]))
                {
                    AddToDiscardPile(player, playerInventory.armor[i]);
                    SendPacket(player, MessageType.ReplaceItem, ItemCategory.Armor, i, savedInventory.armor[i]);
                }
            
            for(int i=0;i<playerInventory.dye.Length;i++)
                if(!IsIdentical(playerInventory.dye[i], savedInventory.dye[i]))
                {
                    AddToDiscardPile(player, playerInventory.dye[i]);
                    SendPacket(player, MessageType.ReplaceItem, ItemCategory.Dye, i, savedInventory.dye[i]);
                }
            
            for(int i=0;i<playerInventory.miscEquips.Length;i++)
                if(!IsIdentical(playerInventory.miscEquips[i], savedInventory.miscEquips[i]))
                {
                    AddToDiscardPile(player, playerInventory.miscEquips[i]);
                    SendPacket(player, MessageType.ReplaceItem, ItemCategory.MiscEquips, i, savedInventory.miscEquips[i]);
                }
            
            for(int i=0;i<playerInventory.miscDyes.Length;i++)
                if(!IsIdentical(playerInventory.miscDyes[i], savedInventory.miscDyes[i]))
                {
                    AddToDiscardPile(player, playerInventory.miscDyes[i]);
                    SendPacket(player, MessageType.ReplaceItem, ItemCategory.MiscDyes, i, savedInventory.miscDyes[i]);
                }

            if(!IsIdentical(playerInventory.trash[0], savedInventory.trash[0]))
            {
                    AddToDiscardPile(player, playerInventory.trash[0]);
                    SendPacket(player, MessageType.ReplaceItem, ItemCategory.Trash, 0, savedInventory.trash[0]);
            }
        }

        // Deserializes json and looks for an entry with the desired name, if not found, return new default inventory
        public static PlayerInventory Deserialize(string playerName)
		{
            if(File.Exists(CharacterDataPath))
            {
                using StreamReader r = new(CharacterDataPath);
                string json = r.ReadToEnd();
                r.Close();
                if(!string.IsNullOrEmpty(json))
                {
                    List<PlayerInventory> inventories = JsonConvert.DeserializeObject<List<PlayerInventory>>(json) ?? new List<PlayerInventory>();
                    for (int i=0;i<inventories.Count;i++)
                        if (inventories[i].name == playerName) // TODO: Make an unique identifier (e.g. two players with same name will break this)
                            return inventories[i];
                }
            }
            else
                File.Create(CharacterDataPath);
            return new PlayerInventory(playerName);
        }
        
        // Adds item to its own json file for storage. Makes async function that waits till player join before breaking the news
        public static void AddToDiscardPile(Player player, EzItem item)
        {
            if(Main.netMode != NetmodeID.Server)
                return;
            if(item.type == 0)
                return;

            // Async function that waits until player join for a max of 60 seconds to send message (so it is not sent before the player joins)
            Func<Task> WaitThenSendMessage = async () => {
                await Task.Run(() => {
                    bool timeHasRunOut = false;
                    var EndTime = (bool b) => {b = true;};
                    Timer timer = new((Object stateInfo) => {}, EndTime, 60000, Timeout.Infinite);
                    while(player.active != true || player == null || timeHasRunOut) {}
                    if(!timeHasRunOut && player != null)
                        ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"{player.name}'s {item.name} broke causality and has left our plane of existance."), Color.Purple);
                    timer.Dispose();
                });
            };
            WaitThenSendMessage();

            string json = null;
            if(File.Exists(DiscardItemDataPath))
            {
                using StreamReader r = new(DiscardItemDataPath);
                json = r.ReadToEnd();
                r.Close();
            }
            else
                File.Create(DiscardItemDataPath);

            List<DeletedItem> items = string.IsNullOrEmpty(json) ? new List<DeletedItem>() : JsonConvert.DeserializeObject<List<DeletedItem>>(json) ?? new List<DeletedItem>();            

            DeletedItem deletedItem = new(item, player.name);
            items.Add(deletedItem);

            json = JsonConvert.SerializeObject(items);
            using StreamWriter outputFile = new(DiscardItemDataPath);
            outputFile.WriteLine(json);
            outputFile.Close();
        }
    }
}
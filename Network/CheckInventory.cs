using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Terraria.ID;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using Newtonsoft.Json;
using Terraria;
using Terraria.Chat;
using Terraria.Localization;
using System.Collections.Generic;

namespace ScuffedAnticheatMod.Network
{
    public class CheckInventory : SAMNetwork
    {
            /* CLIENT */
        // Sends a packet to server to check inventory and gives guid
        public static void SendPacket()
        {
            var packet = ScuffedAnticheatMod.instance.GetPacket();
            packet.Write((byte)MessageType.CheckMyInventory);
            packet.WriteNullTerminatedString(Guid.guid);
            packet.Send(255); // Send to server
        }

            /* SERVER */
        // Checks entire inventory against json save data. Sends ReplaceItem packet if incorrect and adds the "deleted" item to its own save data
        public static void ProcessCheckInventory(ref BinaryReader reader, int playerNumber)
        {
            Player player = Main.player[playerNumber];
            guids[playerNumber] = reader.ReadNullTerminatedString();
            PlayerInventory savedInventory = FindPlayerInventory(player.name, guids[playerNumber], Main.worldID);

            SearchInventory(player, savedInventory);
        }

        // Helper Method that searches inventory, sends packet, and saves delted item.
        private static void SearchInventory(Player player, PlayerInventory savedInventory)
        {
            PlayerInventory playerInventory = new(player);
            List<EzItem> itemsDeleted = new List<EzItem>();
            List<EzItem> itemsAdded = new List<EzItem>();

            for(int i = 0; i < playerInventory.inventory.Length; i++)
                if(i == 58)
                    {} // inst = "packet that open inventory etc";
                else if(IsIdentical(playerInventory.inventory[i], savedInventory.inventory[i]))
                {
                    if(savedInventory.inventory[58].type != ItemID.None && savedInventory.inventory[i].type == ItemID.None)
                    {
                        savedInventory.inventory[i] = savedInventory.inventory[58];
                        savedInventory.inventory[58] = new EzItem(new Item(ItemID.None));
                    }
                }
                else
                {
                    if(savedInventory.inventory[58].type != ItemID.None && savedInventory.inventory[i].type == ItemID.None)
                    {
                        if(!IsIdentical(playerInventory.inventory[i], savedInventory.inventory[58]))
                            itemsDeleted.Add(playerInventory.inventory[i]);
                        SendPacket(player.whoAmI, ItemCategory.Inventory, i, savedInventory.inventory[58]);
                        savedInventory.inventory[i] = savedInventory.inventory[58];
                        savedInventory.inventory[58] = new EzItem(new Item(ItemID.None));
                    }
                    else
                    {
                        itemsAdded.Add(savedInventory.inventory[i]);
                        itemsDeleted.Add(playerInventory.inventory[i]);
                        SendPacket(player.whoAmI, ItemCategory.Inventory, i, savedInventory.inventory[i]);
                    }
                }

            for(int i = 0; i < playerInventory.bank1.Length; i++)
                if(!IsIdentical(playerInventory.bank1[i], savedInventory.bank1[i]))
                {
                    itemsAdded.Add(savedInventory.bank1[i]);
                    itemsDeleted.Add(playerInventory.bank1[i]);
                    SendPacket(player.whoAmI, ItemCategory.Bank1, i, savedInventory.bank1[i]);
                }

            for(int i = 0; i < playerInventory.bank2.Length; i++)
                if(!IsIdentical(playerInventory.bank2[i], savedInventory.bank2[i]))
                {
                    itemsAdded.Add(savedInventory.bank2[i]);
                    itemsDeleted.Add(playerInventory.bank2[i]);
                    SendPacket(player.whoAmI, ItemCategory.Bank2, i, savedInventory.bank2[i]);
                }

            for(int i = 0; i < playerInventory.bank3.Length; i++)
                if(!IsIdentical(playerInventory.bank3[i], savedInventory.bank3[i]))
                {
                    itemsAdded.Add(savedInventory.bank3[i]);
                    itemsDeleted.Add(playerInventory.bank3[i]);
                    SendPacket(player.whoAmI, ItemCategory.Bank3, i, savedInventory.bank3[i]);
                }

            for(int i = 0; i < playerInventory.bank4.Length; i++)
                if(!IsIdentical(playerInventory.bank4[i], savedInventory.bank4[i]))
                {
                    itemsAdded.Add(savedInventory.bank4[i]);
                    itemsDeleted.Add(playerInventory.bank4[i]);
                    SendPacket(player.whoAmI, ItemCategory.Bank4, i, savedInventory.bank4[i]);
                }

            for(int i = 0; i < playerInventory.armor.Length; i++)
                if(!IsIdentical(playerInventory.armor[i], savedInventory.armor[i]))
                {
                    itemsAdded.Add(savedInventory.armor[i]);
                    itemsDeleted.Add(playerInventory.armor[i]);
                    SendPacket(player.whoAmI, ItemCategory.Armor, i, savedInventory.armor[i]);
                }

            for(int i = 0; i < playerInventory.dye.Length; i++)
                if(!IsIdentical(playerInventory.dye[i], savedInventory.dye[i]))
                {
                    itemsAdded.Add(savedInventory.dye[i]);
                    itemsDeleted.Add(playerInventory.dye[i]);
                    SendPacket(player.whoAmI, ItemCategory.Dye, i, savedInventory.dye[i]);
                }

            for(int i = 0; i < playerInventory.miscEquips.Length; i++)
                if(!IsIdentical(playerInventory.miscEquips[i], savedInventory.miscEquips[i]))
                {
                    itemsAdded.Add(savedInventory.miscEquips[i]);
                    itemsDeleted.Add(playerInventory.miscEquips[i]);
                    SendPacket(player.whoAmI, ItemCategory.MiscEquips, i, savedInventory.miscEquips[i]);
                }

            for(int i = 0; i < playerInventory.miscDyes.Length; i++)
                if(!IsIdentical(playerInventory.miscDyes[i], savedInventory.miscDyes[i]))
                {
                    itemsAdded.Add(savedInventory.miscDyes[i]);
                    itemsDeleted.Add(playerInventory.miscDyes[i]);
                    SendPacket(player.whoAmI, ItemCategory.MiscDyes, i, savedInventory.miscDyes[i]);
                }

            if(!IsIdentical(playerInventory.trash[0], savedInventory.trash[0]))
            {
                itemsAdded.Add(savedInventory.trash[0]);
                itemsDeleted.Add(playerInventory.trash[0]);
                SendPacket(player.whoAmI, ItemCategory.Trash, 0, savedInventory.trash[0]);
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
        
        // Adds item to its own json file for storage. Makes async function that waits till player join before breaking the news
        private static void AddToDiscardPile(Player player, List<EzItem> items)
        {
            if(items.Count == 0)
                return;

            string message = "";
            foreach(EzItem item in items)
            {
                if(item.type == 0)
                    continue;

                deletedItems.Add(new DeletedItem(item, player.name, guids[player.whoAmI]));
                message += $"{player.name}'s {item.itemName} broke causality and has left our plane of existance.\n";
            }
            if(message.EndsWith('\n'))
                message = message.Remove(message.Length - 1);

            string json = JsonConvert.SerializeObject(deletedItems);
            using StreamWriter outputFile = new(DiscardItemDataPath);
            outputFile.WriteLine(json);
            outputFile.Close();

            // Async function that waits until player join for a max of 60 seconds to send message (so msg is not sent before the player joins)
            Func<Task> WaitThenSendMessage = async () =>
            {
                await Task.Run(() =>
                {
                    bool timeHasRunOut = false;
                    var EndTime = (bool b) => {timeHasRunOut = true;};
                    Timer timer = new((Object stateInfo) => {}, EndTime, 60000, Timeout.Infinite);

                    // check for player every 1 sec. Gives up after 60 secs
                    while((player == null || player.active == false) && !timeHasRunOut) {Task.Delay(1000);}

                    if(!timeHasRunOut && player != null)
                        ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(message), Color.Purple);
                    timer.Dispose();
                });
            };
            WaitThenSendMessage();
        }

        // Sends packet to fix client inventory item
        protected static void SendPacket(int target, ItemCategory category, int index, EzItem newItem)
        {
            var packet = ScuffedAnticheatMod.instance.GetPacket();
            packet.Write((byte)MessageType.ReplaceItem);
            packet.Write((byte)category);
            packet.Write((byte)index);
            Terraria.ModLoader.IO.ItemIO.Send(newItem.GetClone(), packet, true, true);
            packet.Send(target);
        }
    }
}
using System.IO;
using System.Collections.Generic;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.Chat;
using Terraria.Localization;
using Terraria.ID;
using Terraria.ModLoader;
using Newtonsoft.Json;

namespace VIRUS
{
    // Enums
    public enum MessageType { CheckInventory, UpdateItem, ModifyItem }
    public enum ItemCategory { Inventory, Bank1, Bank2, Bank3, Bank4, Armor, Dye, MiscEquips, MiscDyes, Trash }

    // Structs
    public struct EzItem
    {
        [JsonIgnore]
        private readonly Item clone;
        public string name;
        public int type;
        public int stack;
        public int prefix;
        public bool favorited;

        // Method
        public readonly Item GetClone()
        {
            return clone ?? new(type, stack, prefix) { favorited = favorited };
        }

        // Constructor
        public EzItem(Item item)
        {
            clone = item.Clone();
            name = item.Name;
            type = item.type;
            stack = item.stack;
            prefix = item.prefix;
            favorited = item.favorited;
        }
    }
    public struct PlayerInventory
    {
        // member variables
		public string name;
		public EzItem[] inventory;
        public EzItem[] bank1;
        public EzItem[] bank2;
        public EzItem[] bank3;
        public EzItem[] bank4;
        public EzItem[] armor;
        public EzItem[] dye;
        public EzItem[] miscEquips;
        public EzItem[] miscDyes;
        public EzItem[] trash;

        // Public Method
        private readonly void Initialize()
        {
            // Starter Items
            inventory[0] = new(new(ItemID.CopperShortsword));
            inventory[1] = new(new(ItemID.CopperPickaxe));
            inventory[2] = new(new(ItemID.CopperAxe));
            if(ModLoader.TryGetMod("CalamityMod", out Mod calamityMod))
            {
                if(ModContent.TryFind("CalamityMod", "StarterBag", out ModItem item))
                    inventory[3] = new(item.Item);
            } else
                inventory[3] = new(new(ItemID.None));

            for(int i=4;i<inventory.Length;i++)
                inventory[i] = new(new(ItemID.None));
            for(int i=0;i<bank1.Length;i++)
                bank1[i] = new(new(ItemID.None));
            for(int i=0;i<bank2.Length;i++)
                bank2[i] = new(new(ItemID.None));
            for(int i=0;i<bank3.Length;i++)
                bank3[i] = new(new(ItemID.None));
            for(int i=0;i<bank4.Length;i++)
                bank4[i] = new(new(ItemID.None));
            for(int i=0;i<armor.Length;i++)
                armor[i] = new(new(ItemID.None));
            for(int i=0;i<dye.Length;i++)
                dye[i] = new(new(ItemID.None));
            for(int i=0;i<miscEquips.Length;i++)
                miscEquips[i] = new(new(ItemID.None));
            for(int i=0;i<miscDyes.Length;i++)
                miscDyes[i] = new(new(ItemID.None));
            trash[0] = new(new(ItemID.None));
        }

        // Constructors
        public PlayerInventory()
        {
            name = "uninitialized";
            inventory = new EzItem[59];
            bank1 = new EzItem[40];
            bank2 = new EzItem[40];
            bank3 = new EzItem[40];
            bank4 = new EzItem[40];
            armor = new EzItem[20];
            dye = new EzItem[10];
            miscEquips = new EzItem[5];
            miscDyes = new EzItem[5];
            trash = new EzItem[1];
        }
        public PlayerInventory(string name) : this()
        {
            this.name = name;
            Initialize();
        }
        public PlayerInventory(Player player) : this()
        {
            name = player.name;
            for(int i=0;i<inventory.Length;i++)
                inventory[i] = new(player.inventory[i]);
            for(int i=0;i<bank1.Length;i++)
                bank1[i] = new(player.bank.item[i]);
            for(int i=0;i<bank2.Length;i++)
                bank2[i] = new(player.bank2.item[i]);
            for(int i=0;i<bank3.Length;i++)
                bank3[i] = new(player.bank3.item[i]);
            for(int i=0;i<bank4.Length;i++)
                bank4[i] = new(player.bank4.item[i]);
            for(int i=0;i<armor.Length;i++)
                armor[i] = new(player.armor[i]);
            for(int i=0;i<dye.Length;i++)
                dye[i] = new(player.dye[i]);
            for(int i=0;i<miscEquips.Length;i++)
                miscEquips[i] = new(player.miscEquips[i]);
            for(int i=0;i<miscDyes.Length;i++)
                miscDyes[i] = new(player.miscDyes[i]);
            trash[0] = new(player.trashItem);
        }
    }
    public struct DeletedItem
    {
        public EzItem item;
        public string owner;
        public DeletedItem(EzItem ezItem, string name)
        {
            item = ezItem;
            owner = name;
        }
    }

    public class Network
    {
        public static string CharacterDataPath { get; } = string.Concat(new object[]
                {
                        Main.SavePath,
                        Path.DirectorySeparatorChar,
                        "AnticheatCharacterData",
                        ".json"
                });
        public static string DiscardItemDataPath { get; } = string.Concat(new object[]
                {
                        Main.SavePath,
                        Path.DirectorySeparatorChar,
                        "AnticheatDiscardData",
                        ".json"
                });

        // Network
        public static void VIRUSMessaged(BinaryReader reader, int playerNumber)
		{
            //We found a VIRUS only message
            MessageType msgType = (MessageType)reader.ReadByte();
            switch(msgType)
            {
                case MessageType.CheckInventory:
                    ProcessCheckInventory(playerNumber);
                    break;
                case MessageType.UpdateItem:
                    ProcessUpdateInventory(ref reader);
                    break;
                case MessageType.ModifyItem:
                    ProcessModifyItem(ref reader);
                    break;
            }
		}

        // General Messages
        public static void ProcessCheckInventory(int playerNumber)
        {
            Player player = Main.player[playerNumber];
            PlayerInventory savedInventory = Deserialize(Main.player[playerNumber].name);

            UpdateInventory(player, savedInventory, MessageType.ModifyItem, out PlayerInventory disregard);
        }
        public static void UpdateInventory(Player player, PlayerInventory savedInventory, MessageType type, out PlayerInventory newInventory)
        {
            PlayerInventory playerInventory = new(player);
            newInventory = savedInventory;

            for(int i=0;i<playerInventory.inventory.Length;i++)
            {
                if(!IsIdentical(playerInventory.inventory[i], savedInventory.inventory[i]))
                {
                    if(type == MessageType.ModifyItem) {
                        AddToDiscardPile(player, playerInventory.inventory[i]);
                        SendPacket(player, type, ItemCategory.Inventory, i, savedInventory.inventory[i]);
                    }
                    else
                        SendPacket(player, type, ItemCategory.Inventory, i, playerInventory.inventory[i], true);
                    newInventory.inventory[i] = playerInventory.inventory[i]; // might be a race condition. shouldn't matter hopefully
                }
            }
            
            for(int i=0;i<playerInventory.bank1.Length;i++)
            {
                if(!IsIdentical(playerInventory.bank1[i], savedInventory.bank1[i]))
                {
                    if(type == MessageType.ModifyItem) {
                        AddToDiscardPile(player, playerInventory.inventory[i]);
                        SendPacket(player, type, ItemCategory.Bank1, i, savedInventory.bank1[i]);
                    }
                    else
                        SendPacket(player, type, ItemCategory.Bank1, i, playerInventory.bank1[i], true);
                    newInventory.bank1[i] = playerInventory.bank1[i]; // might be a race condition. shouldn't matter hopefully
                }
            }
            
            for(int i=0;i<playerInventory.bank2.Length;i++)
            {
                if(!IsIdentical(playerInventory.bank2[i], savedInventory.bank2[i]))
                {
                    if(type == MessageType.ModifyItem) {
                        AddToDiscardPile(player, playerInventory.inventory[i]);
                        SendPacket(player, type, ItemCategory.Bank2, i, savedInventory.bank2[i]);
                    }
                    else
                        SendPacket(player, type, ItemCategory.Bank2, i, playerInventory.bank2[i], true);
                    newInventory.bank2[i] = playerInventory.bank2[i]; // might be a race condition. shouldn't matter hopefully
                }
            }
            
            for(int i=0;i<playerInventory.bank3.Length;i++)
            {
                if(!IsIdentical(playerInventory.bank3[i], savedInventory.bank3[i]))
                {
                    if(type == MessageType.ModifyItem) {
                        AddToDiscardPile(player, playerInventory.inventory[i]);
                        SendPacket(player, type, ItemCategory.Bank3, i, savedInventory.bank3[i]);
                    }
                    else
                        SendPacket(player, type, ItemCategory.Bank3, i, playerInventory.bank3[i], true);
                    newInventory.bank3[i] = playerInventory.bank3[i]; // might be a race condition. shouldn't matter hopefully
                }
            }
            
            for(int i=0;i<playerInventory.bank4.Length;i++)
            {
                if(!IsIdentical(playerInventory.bank4[i], savedInventory.bank4[i]))
                {
                    if(type == MessageType.ModifyItem) {
                        AddToDiscardPile(player, playerInventory.inventory[i]);
                        SendPacket(player, type, ItemCategory.Bank4, i, savedInventory.bank4[i]);
                    }
                    else
                        SendPacket(player, type, ItemCategory.Bank4, i, playerInventory.bank4[i], true);
                    newInventory.bank4[i] = playerInventory.bank4[i]; // might be a race condition. shouldn't matter hopefully
                }
            }
            
            for(int i=0;i<playerInventory.armor.Length;i++)
            {
                if(!IsIdentical(playerInventory.armor[i], savedInventory.armor[i]))
                {
                    if(type == MessageType.ModifyItem) {
                        AddToDiscardPile(player, playerInventory.inventory[i]);
                        SendPacket(player, type, ItemCategory.Armor, i, savedInventory.armor[i]);
                    }
                    else
                        SendPacket(player, type, ItemCategory.Armor, i, playerInventory.armor[i], true);
                    newInventory.armor[i] = playerInventory.armor[i]; // might be a race condition. shouldn't matter hopefully
                }
            }
            
            for(int i=0;i<playerInventory.dye.Length;i++)
            {
                if(!IsIdentical(playerInventory.dye[i], savedInventory.dye[i]))
                {
                    if(type == MessageType.ModifyItem) {
                        AddToDiscardPile(player, playerInventory.inventory[i]);
                        SendPacket(player, type, ItemCategory.Dye, i, savedInventory.dye[i]);
                    }
                    else
                        SendPacket(player, type, ItemCategory.Dye, i, playerInventory.dye[i], true);
                    newInventory.dye[i] = playerInventory.dye[i]; // might be a race condition. shouldn't matter hopefully
                }
            }
            
            for(int i=0;i<playerInventory.miscEquips.Length;i++)
            {
                if(!IsIdentical(playerInventory.miscEquips[i], savedInventory.miscEquips[i]))
                {
                    if(type == MessageType.ModifyItem) {
                        AddToDiscardPile(player, playerInventory.inventory[i]);
                        SendPacket(player, type, ItemCategory.MiscEquips, i, savedInventory.miscEquips[i]);
                    }
                    else
                        SendPacket(player, type, ItemCategory.MiscEquips, i, playerInventory.miscEquips[i], true);
                    newInventory.miscEquips[i] = playerInventory.miscEquips[i]; // might be a race condition. shouldn't matter hopefully
                }
            }
            
            for(int i=0;i<playerInventory.miscDyes.Length;i++)
            {
                if(!IsIdentical(playerInventory.miscDyes[i], savedInventory.miscDyes[i]))
                {
                    if(type == MessageType.ModifyItem) {
                        AddToDiscardPile(player, playerInventory.inventory[i]);
                        SendPacket(player, type, ItemCategory.MiscDyes, i, savedInventory.miscDyes[i]);
                    }
                    else
                        SendPacket(player, type, ItemCategory.MiscDyes, i, playerInventory.miscDyes[i], true);
                    newInventory.miscDyes[i] = playerInventory.miscDyes[i]; // might be a race condition. shouldn't matter hopefully
                }
            }

            if(!IsIdentical(playerInventory.trash[0], savedInventory.trash[0]))
            {
                if(type == MessageType.ModifyItem) {
                    AddToDiscardPile(player, playerInventory.trash[0]);
                    SendPacket(player, type, ItemCategory.Trash, 0, savedInventory.trash[0]);
                }
                else
                    SendPacket(player, type, ItemCategory.Trash, 0, playerInventory.trash[0], true);
                newInventory.trash[0] = playerInventory.trash[0]; // might be a race condition. shouldn't matter hopefully
            }
        }
        public static bool IsIdentical(EzItem item1, EzItem item2)
        {
            if(item1.type == item2.type && item1.prefix == item2.prefix && item1.stack == item2.stack)
                return true;
            else
                return false;
        }
        public static void AddToDiscardPile(Player player, EzItem item)
        {
            if(Main.netMode != NetmodeID.Server)
                return;
            if(item.type == 0)
                return;

            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"{player.name}'s {item.name} broke causality and has left our plane of existance."), Color.Purple);

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
        public static void ProcessUpdateInventory(ref BinaryReader reader)
        {
            int playerNum = reader.ReadByte();
            Player player = (Player)Main.player[playerNum].Clone();
            // Variables
            ItemCategory itemCategory = (ItemCategory)reader.ReadByte();
            int itemIndex = reader.ReadByte();
            Item newItem = Terraria.ModLoader.IO.ItemIO.Receive(reader, true, true);
            List<PlayerInventory> savedInventories = Deserialize();

            // Get Index of player inventory if it exists, otherwise create new player entry
            int inventoryIndex = savedInventories.Count;
            for(int i=0;i<savedInventories.Count;i++)
                if(savedInventories[i].name == player.name)
                    inventoryIndex = i;
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

            string json = JsonConvert.SerializeObject(savedInventories);
            using StreamWriter outputFile = new(CharacterDataPath);
            outputFile.WriteLine(json);
            outputFile.Close();
        }
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
        public static List<PlayerInventory> Deserialize()
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
        public static void SendPacket(Player player, MessageType type, ItemCategory category, int index, EzItem newItem, bool toServer=false)
        {
            // Create packet
            var packet = VIRUS.instance.GetPacket();

            // Add relevant identifiers
            packet.Write((byte)type);
            packet.Write((byte)player.whoAmI);
            packet.Write((byte)category);
            packet.Write((byte)index);
            Terraria.ModLoader.IO.ItemIO.Send(newItem.GetClone(), packet, true, true);

            if(toServer)
                packet.Send(255); // Send to server
            else
                packet.Send(player.whoAmI); // Send to specific player
            return;
        }
    }
}
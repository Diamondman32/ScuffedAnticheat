using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.Chat;
using Terraria.Localization;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace VIRUS
{
    // Enums
    public enum MessageType { CheckInventory, UpdateItem, ModifyItem }
    public enum ItemCategory { Inventory, Bank1, Bank2, Bank3, Bank4, Armor, Dye, MiscEquips, MiscDyes }
    //

    // Struct
    public struct PlayerInventory
    {
        // member variables
		public string name;
		public Item[] inventory;
        public Item[] bank1;
        public Item[] bank2;
        public Item[] bank3;
        public Item[] bank4;
		public Item[] armor;
		public Item[] dye;
        public Item[] miscEquips;
        public Item[] miscDyes;

        // Public Method
        private void Initialize()
        {
            // Starter Items
            inventory[0] = new Item(ItemID.CopperShortsword);
            inventory[1] = new Item(ItemID.CopperPickaxe);
            inventory[2] = new Item(ItemID.CopperAxe);
            if(ModLoader.TryGetMod("CalamityMod", out Mod calamityMod))
            {
                if(ModContent.TryFind("CalamityMod", "StarterBag", out ModItem item))
                    inventory[3] = item.Item;
            } else
                inventory[3] = new Item(0);

            for(int i=4;i<inventory.Length;i++)
                inventory[i] = new Item(0);
            for(int i=0;i<bank1.Length;i++)
                bank1[i] = new Item(0);
            for(int i=0;i<bank2.Length;i++)
                bank2[i] = new Item(0);
            for(int i=0;i<bank3.Length;i++)
                bank3[i] = new Item(0);
            for(int i=0;i<bank4.Length;i++)
                bank4[i] = new Item(0);
            for(int i=0;i<armor.Length;i++)
                armor[i] = new Item(0);
            for(int i=0;i<dye.Length;i++)
                dye[i] = new Item(0);
            for(int i=0;i<miscEquips.Length;i++)
                miscEquips[i] = new Item(0);
            for(int i=0;i<miscDyes.Length;i++)
                miscDyes[i] = new Item(0);
        }

        // Constructors
        public PlayerInventory()
        {
            name = "";
            inventory = new Item[59];
            bank1 = new Item[40];
            bank2 = new Item[40];
            bank3 = new Item[40];
            bank4 = new Item[40];
            armor = new Item[20];
            dye = new Item[59];
            miscEquips = new Item[5];
            miscDyes = new Item[5];
            Initialize();
        }
        public PlayerInventory(string name)
        {
            this.name = name;
            inventory = new Item[59];
            bank1 = new Item[40];
            bank2 = new Item[40];
            bank3 = new Item[40];
            bank4 = new Item[40];
            armor = new Item[20];
            dye = new Item[59];
            miscEquips = new Item[5];
            miscDyes = new Item[5];
            Initialize();
        }
        public PlayerInventory(Player player)
        {
            name = player.name;
            inventory = player.inventory;
            bank1 = player.bank.item;
            bank2 = player.bank2.item;
            bank3 = player.bank3.item;
            bank4 = player.bank4.item;
            armor = player.armor;
            dye = player.dye;
            miscEquips = player.miscEquips;
            miscDyes = player.miscDyes;
        }
        public PlayerInventory(string name, Item[] inventory, Item[] bank1, Item[] bank2, Item[] bank3, Item[] bank4, Item[] armor, Item[] dye, Item[] miscEquips, Item[] miscDyes)
        {
            this.name = name;
            this.inventory = inventory;
            this.bank1 = bank1;
            this.bank2 = bank2;
            this.bank3 = bank3;
            this.bank4 = bank4;
            this.armor = armor;
            this.dye = dye;
            this.miscEquips = miscEquips;
            this.miscDyes = miscDyes;
        }
    }
    //

    public class Network
    {
        public static string FilePath { get; } = string.Concat(new object[]
                {
                        Main.SavePath,
                        Path.DirectorySeparatorChar,
                        "AnticheatCharacterData",
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
            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Checking Inventory"), Color.Green);

            Player player = Main.player[playerNumber];
            // PlayerInventory currentInventory = new(player);
            PlayerInventory savedInventory = Deserialize(Main.player[playerNumber].name);

            UpdateInventory(player, savedInventory, MessageType.ModifyItem, out PlayerInventory disregard);
            
            

            // if(ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            // {
            //     calamity.GetType().Assembly.GetType()
            //     var calamityModPlayerType = calamity.GetType().Assembly.GetType("CalamityMod.CalamityPlayer");

            //     if(calamityModPlayerType != null)
            //     {
            //         var modPlayerInstance = player.GetModPlayer(calamityModPlayerType);
            //     }

            //     if(player.TryGetModPlayer(calamity.GetType().Assembly.GetType("CalamityMod.CalamityPlayer"), out var calPLayer))
            //     {
                    
            //     }
            // }
        }
        public static void UpdateInventory(Player player, PlayerInventory savedInventory, MessageType type, out PlayerInventory newInventory)
        {
            PlayerInventory playerInventory = new(player);
            newInventory = savedInventory;

            for(int i=0;i<playerInventory.inventory.Length;i++)
            {
                if(!IsIdentical(playerInventory.inventory[i], savedInventory.inventory[i]))
                {
                    if(type == MessageType.ModifyItem)
                        SendPacket(player, type, ItemCategory.Inventory, i, savedInventory.inventory[i]);
                    else
                        SendPacket(player, type, ItemCategory.Inventory, i, playerInventory.inventory[i], true);
                    newInventory.inventory[i] = playerInventory.inventory[i];
                }
            }
            
            for(int i=0;i<playerInventory.bank1.Length;i++)
            {
                if(!IsIdentical(playerInventory.bank1[i], savedInventory.bank1[i]))
                {
                    if(type == MessageType.ModifyItem)
                        SendPacket(player, type, ItemCategory.Bank1, i, savedInventory.bank1[i]);
                    else
                        SendPacket(player, type, ItemCategory.Bank1, i, playerInventory.bank1[i], true);
                    newInventory.bank1[i] = playerInventory.bank1[i];
                }
            }
            
            for(int i=0;i<playerInventory.bank2.Length;i++)
            {
                if(!IsIdentical(playerInventory.bank2[i], savedInventory.bank2[i]))
                {
                    if(type == MessageType.ModifyItem)
                        SendPacket(player, type, ItemCategory.Bank2, i, savedInventory.bank2[i]);
                    else
                        SendPacket(player, type, ItemCategory.Bank2, i, playerInventory.bank2[i], true);
                    newInventory.bank2[i] = playerInventory.bank2[i];
                }
            }
            
            for(int i=0;i<playerInventory.bank3.Length;i++)
            {
                if(!IsIdentical(playerInventory.bank3[i], savedInventory.bank3[i]))
                {
                    if(type == MessageType.ModifyItem)
                        SendPacket(player, type, ItemCategory.Bank3, i, savedInventory.bank3[i]);
                    else
                        SendPacket(player, type, ItemCategory.Bank3, i, playerInventory.bank3[i], true);
                    newInventory.bank3[i] = playerInventory.bank3[i];
                }
            }
            
            for(int i=0;i<playerInventory.bank4.Length;i++)
            {
                if(!IsIdentical(playerInventory.bank4[i], savedInventory.bank4[i]))
                {
                    if(type == MessageType.ModifyItem)
                        SendPacket(player, type, ItemCategory.Bank4, i, savedInventory.bank4[i]);
                    else
                        SendPacket(player, type, ItemCategory.Bank4, i, playerInventory.bank4[i], true);
                    newInventory.bank4[i] = playerInventory.bank4[i];
                }
            }
            
            for(int i=0;i<playerInventory.armor.Length;i++)
            {
                if(!IsIdentical(playerInventory.armor[i], savedInventory.armor[i]))
                {
                    if(type == MessageType.ModifyItem)
                        SendPacket(player, type, ItemCategory.Armor, i, savedInventory.armor[i]);
                    else
                        SendPacket(player, type, ItemCategory.Armor, i, playerInventory.armor[i], true);
                    newInventory.armor[i] = playerInventory.armor[i];
                }
            }
            
            for(int i=0;i<playerInventory.dye.Length;i++)
            {
                if(!IsIdentical(playerInventory.dye[i], savedInventory.dye[i]))
                {
                    if(type == MessageType.ModifyItem)
                        SendPacket(player, type, ItemCategory.Dye, i, savedInventory.dye[i]);
                    else
                        SendPacket(player, type, ItemCategory.Dye, i, playerInventory.dye[i], true);
                    newInventory.dye[i] = playerInventory.dye[i];
                }
            }
            
            for(int i=0;i<playerInventory.miscEquips.Length;i++)
            {
                if(!IsIdentical(playerInventory.miscEquips[i], savedInventory.miscEquips[i]))
                {
                    if(type == MessageType.ModifyItem)
                        SendPacket(player, type, ItemCategory.MiscEquips, i, savedInventory.miscEquips[i]);
                    else
                        SendPacket(player, type, ItemCategory.MiscEquips, i, playerInventory.miscEquips[i], true);
                    newInventory.miscEquips[i] = playerInventory.miscEquips[i];
                }
            }
            
            for(int i=0;i<playerInventory.miscDyes.Length;i++)
            {
                if(!IsIdentical(playerInventory.miscDyes[i], savedInventory.miscDyes[i]))
                {
                    if(type == MessageType.ModifyItem)
                        SendPacket(player, type, ItemCategory.MiscDyes, i, savedInventory.miscDyes[i]);
                    else
                        SendPacket(player, type, ItemCategory.MiscDyes, i, playerInventory.miscDyes[i], true);
                    newInventory.miscDyes[i] = playerInventory.miscDyes[i];
                }
            }
        }
        public static bool IsIdentical(Item item1, Item item2)
        {
            if(item1.type == item2.type && item1.prefix == item2.prefix && item1.stack == item2.stack)
                return true;
            else
                return false;
        }
        public static void ProcessUpdateInventory(ref BinaryReader reader)
        {
            int playerNum = reader.ReadByte();
            Player player = Main.player[playerNum];
            // Variables
            ItemCategory itemCategory = (ItemCategory)reader.ReadByte();
            int itemIndex = reader.ReadByte();
            Item newItem = Terraria.ModLoader.IO.ItemIO.Receive(reader, true, true);
            List<PlayerInventory> savedInventories = Deserialize();

            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"Modifying Saved Inventory array\n\tItem: {newItem.Name} at i={itemIndex}"), Color.Green);

            // Get Index of player inventory if it exists, otherwise create new player entry
            int inventoryIndex = savedInventories.Count;
            for(int i=0;i<savedInventories.Count;i++)
                if(savedInventories[i].name == player.name)
                    inventoryIndex = i;
            if(inventoryIndex == savedInventories.Count)
                savedInventories.Add(new(player));

            switch(itemCategory)
            {
                case ItemCategory.Inventory:
                    savedInventories[inventoryIndex].inventory[itemIndex] = newItem;
                    break;
                case ItemCategory.Bank1:
                    savedInventories[inventoryIndex].bank1[itemIndex] = newItem;
                    break;
                case ItemCategory.Bank2:
                    savedInventories[inventoryIndex].bank2[itemIndex] = newItem;
                    break;
                case ItemCategory.Bank3:
                    savedInventories[inventoryIndex].bank3[itemIndex] = newItem;
                    break;
                case ItemCategory.Bank4:
                    savedInventories[inventoryIndex].bank4[itemIndex] = newItem;
                    break;
                case ItemCategory.Armor:
                    savedInventories[inventoryIndex].armor[itemIndex] = newItem;
                    break;
                case ItemCategory.Dye:
                    savedInventories[inventoryIndex].dye[itemIndex] = newItem;
                    break;
                case ItemCategory.MiscEquips:
                    savedInventories[inventoryIndex].miscEquips[itemIndex] = newItem;
                    break;
                case ItemCategory.MiscDyes:
                    savedInventories[inventoryIndex].miscDyes[itemIndex] = newItem;
                    break;
            }

            string json = JsonConvert.SerializeObject(savedInventories, Formatting.Indented);
            using StreamWriter outputFile = new(FilePath);
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

                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"Incorrect Item Found At i={itemIndex}:\n\tCorrect: {newItem.Name}\tActual: {Main.LocalPlayer.inventory[itemIndex].Name}"), Color.Green);

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
                }
            }
        }
        public static PlayerInventory Deserialize(string playerName)
		{
            if(File.Exists(FilePath))
            {
                using StreamReader r = new(FilePath);
                string json = r.ReadToEnd();
                r.Close();
                if(!string.IsNullOrEmpty(json))
                {
                    List<PlayerInventory> inventories = System.Text.Json.JsonSerializer.Deserialize<List<PlayerInventory>>(json);
                    for (int i=0;i<inventories.Count;i++)
                        if (inventories[i].name == playerName) // TODO: Make an unique identifier (e.g. two players with same name will break this)
                            return inventories[i];
                }
            }
            else
                File.Create(FilePath);
            return new PlayerInventory(playerName);
        }
        public static List<PlayerInventory> Deserialize()
		{
            string json = null;
            if(File.Exists(FilePath))
            {
                using StreamReader r = new(FilePath);
                json = r.ReadToEnd();
                r.Close();
            }
            else
                File.Create(FilePath);
            return string.IsNullOrEmpty(json) ? new List<PlayerInventory>() : System.Text.Json.JsonSerializer.Deserialize<List<PlayerInventory>>(json);
        }
        public static void SendPacket(Player player, MessageType type, ItemCategory category, int index, Item newItem, bool toServer=false)
        {
            // Create packet
            var packet = VIRUS.instance.GetPacket();

            // Add relevant identifiers
            packet.Write((byte)type);
            packet.Write((byte)player.whoAmI);
            packet.Write((byte)category);
            packet.Write((byte)index);
            Terraria.ModLoader.IO.ItemIO.Send(newItem, packet, true, true);

            if(toServer)
                packet.Send(255); // Send to server
            else
                packet.Send(player.whoAmI); // Send to specific player
            return;
        }
    }
}
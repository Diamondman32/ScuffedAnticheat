using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Newtonsoft.Json;
using System.Collections.Generic;
using MonoMod.Utils;

namespace ScuffedAnticheatMod.Network
{
    // Enums
    public enum MessageType { SendHandshake, ReceiveHandshake, CheckInventory, CheckMods, UpdateSaveData, ModifyPlayerData, DeletedItemRequest, ReceiveDeletedItems,
        UpdateDeletedItemSaveData, SyncDeletedItems, ReceiveLocation }
    public enum ItemCategory { Inventory, Bank1, Bank2, Bank3, Bank4, Armor, Dye, MiscEquips, MiscDyes, Trash, FindFirstOpenInv }

    // Structs
    public class EzItem
    {
        [JsonIgnore]
        private Item clone { get; }
        public int type { get; }
        public int stack { get; }
        public int prefix { get; }
        public bool favorited { get; }

        // Method
        public Item GetClone()
        {
            return clone ?? new(type, stack, prefix) { favorited = favorited };
        }

        // Constructors
        public EzItem(Item item)
        {
            clone = item.Clone();
            type = item.type;
            stack = item.stack;
            prefix = item.prefix;
            favorited = item.favorited;
        }
        [JsonConstructor]
        public EzItem(int type, int stack, int prefix, bool favorited)
        {
            this.type = type;
            this.stack = stack;
            this.prefix = prefix;
            this.favorited = favorited;
        }
    }
    // TODO: Maybe track sqlite player row id so it doesn't have to be rediscovered each time
    public class PlayerInventory
    {
		public string playerName { get; }
        public string guid { get; }
        public int worldID { get; }
        public float xPos { get; private set; }
        public float yPos { get; private set; }
		public EzItem[] inventory { get; }
        public EzItem[] bank1 { get; }
        public EzItem[] bank2 { get; }
        public EzItem[] bank3 { get; }
        public EzItem[] bank4 { get; }
        public EzItem[] armor { get; }
        public EzItem[] dye { get; }
        public EzItem[] miscEquips { get; }
        public EzItem[] miscDyes { get; }
        public EzItem[] trash { get; }

        private void SetInventoryDefaults()
        {
            List<Item> startingItems = PlayerLoader.GetStartingItems(new Player(), [ new(ItemID.CopperShortsword), new(ItemID.CopperPickaxe), new(ItemID.CopperAxe) ]);

            for (int i = 0; i < inventory.Length; ++i)
            {
                if (i < startingItems.Count)
                    inventory[i] = new(startingItems[i]);
                else
                    inventory[i] = new(new(ItemID.None));
            }
            for(int i = 4; i < inventory.Length; ++i)
                inventory[i] = new(new(ItemID.None));
            for(int i = 0; i < bank1.Length; ++i)
                bank1[i] = new(new(ItemID.None));
            for(int i = 0; i < bank2.Length; ++i)
                bank2[i] = new(new(ItemID.None));
            for(int i = 0; i < bank3.Length; ++i)
                bank3[i] = new(new(ItemID.None));
            for(int i = 0; i < bank4.Length; ++i)
                bank4[i] = new(new(ItemID.None));
            for(int i = 0; i < armor.Length; ++i)
                armor[i] = new(new(ItemID.None));
            for(int i = 0; i < dye.Length; ++i)
                dye[i] = new(new(ItemID.None));
            for(int i = 0; i < miscEquips.Length; ++i)
                miscEquips[i] = new(new(ItemID.None));
            for(int i = 0; i < miscDyes.Length; ++i)
                miscDyes[i] = new(new(ItemID.None));
            trash[0] = new(new(ItemID.None));
        }

        // Constructors
        public PlayerInventory()
        {
            playerName = "uninitialized";
            guid = "uninitialized";
            worldID = Main.worldID;
            xPos = Main.spawnTileX * 16;
            yPos = (Main.spawnTileY - 3) * 16;
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
        public PlayerInventory(string playerName) : this()
        {
            this.playerName = playerName;
            SetInventoryDefaults();
        }
        public PlayerInventory(string playerName, string guid) : this()
        {
            this.playerName = playerName;
            this.guid = guid;
            SetInventoryDefaults();
        }
        public PlayerInventory(Player player) : this()
        {
            playerName = player.name;
            for(int i = 0 ; i < inventory.Length; ++i)
                inventory[i] = new(player.inventory[i]);
            for(int i = 0 ; i < bank1.Length; ++i)
                bank1[i] = new(player.bank.item[i]);
            for(int i = 0 ; i < bank2.Length; ++i)
                bank2[i] = new(player.bank2.item[i]);
            for(int i = 0 ; i < bank3.Length; ++i)
                bank3[i] = new(player.bank3.item[i]);
            for(int i = 0 ; i < bank4.Length; ++i)
                bank4[i] = new(player.bank4.item[i]);
            for(int i = 0 ; i < armor.Length; ++i)
                armor[i] = new(player.armor[i]);
            for(int i = 0 ; i < dye.Length; ++i)
                dye[i] = new(player.dye[i]);
            for(int i = 0 ; i < miscEquips.Length; ++i)
                miscEquips[i] = new(player.miscEquips[i]);
            for(int i = 0 ; i < miscDyes.Length; ++i)
                miscDyes[i] = new(player.miscDyes[i]);
            trash[0] = new(player.trashItem);
        }
        [JsonConstructor]
        public PlayerInventory(string playerName, string guid, int worldID, float xPos, float yPos, EzItem[] inventory, EzItem[] bank1, EzItem[] bank2, EzItem[] bank3,
            EzItem[] bank4, EzItem[] armor, EzItem[] dye, EzItem[] miscEquips, EzItem[] miscDyes, EzItem[] trash)
        {
            this.playerName = playerName;
            this.guid = guid;
            this.worldID = worldID;
            this.xPos = xPos;
            this.yPos = yPos;
            this.inventory = inventory;
            this.bank1 = bank1;
            this.bank2 = bank2;
            this.bank3 = bank3;
            this.bank4 = bank4;
            this.armor = armor;
            this.dye = dye;
            this.miscEquips = miscEquips;
            this.miscDyes = miscDyes;
            this.trash = trash;
        }
    }
    public readonly struct DeletedItem
    {
        public EzItem item { get; }
        public string owner { get; }
        public string guid { get; }
        public int worldID { get; }
        public DeletedItem(EzItem ezItem, string name, string guid)
        {
            item = ezItem;
            owner = name;
            this.guid = guid;
            worldID = Main.worldID;
        }
        [JsonConstructor]
        public DeletedItem(EzItem item, string owner, string guid, int worldID)
        {
            this.item = item;
            this.owner = owner;
            this.guid = guid;
            this.worldID = worldID;
        }
    }

    public class SAMNetwork
    {
        protected static string CharacterDataPath { get; } = Main.SavePath + Path.DirectorySeparatorChar + "AnticheatCharacterData.json";
        protected static string DiscardItemDataPath { get; } = Main.SavePath + Path.DirectorySeparatorChar + "AnticheatDiscardData.json";
        public static string[] guids { get; protected set; } = new string[255]; // Parallel to Main.player[]; doesn't remove inactive players
        public static PlayerInventory[] playerInventories { get; protected set; } = new PlayerInventory[255];
        public static bool[] allowedPlayers { get; protected set; } = new bool[255];
        public static List<DeletedItem> deletedItems { get; protected set; } = new List<DeletedItem>();

        // TODO: No checks for after initial join or packet spoofing. Not sure if that's really feasible
        // Sorts SAM packets based off of their message type
        public static void HandlePacket(BinaryReader reader, int playerNumber)
        {
            MessageType msgType = (MessageType)reader.ReadByte();
            switch (msgType)
            {
                case MessageType.SendHandshake:
                    CheckMods.SendPacket();
                    break;
                case MessageType.ReceiveHandshake:
                    SetupPlayer(ref reader, playerNumber);
                    CheckInventory.ProcessRequest(playerNumber);
                    CheckMods.ProcessRequest(ref reader, playerNumber);
                    UpdateLocation.SendPacket(playerNumber);
                    allowedPlayers[playerNumber] = true;
                    break;
                case MessageType.UpdateDeletedItemSaveData:
                    UpdateDeletedItemSaveData.ProcessRequest(ref reader, playerNumber);
                    break;
                case MessageType.ModifyPlayerData:
                    ModifyPlayerData.ProcessRequest(ref reader);
                    break;
                case MessageType.DeletedItemRequest:
                    RequestDeletedItems.ProcessRequest(ref reader, playerNumber);
                    break;
                case MessageType.ReceiveDeletedItems:
                    ReceiveDeletedItems.ProcessRequest(ref reader);
                    break;
                case MessageType.SyncDeletedItems:
                    SyncDeletedItems.ProcessRequest();
                    break;
                case MessageType.ReceiveLocation:
                    UpdateLocation.ProcessRequest(ref reader);
                    break;
            }
        }

        public static void InitiateHandshake(int playerNumber)
        {
            allowedPlayers[playerNumber] = false;
            var packet = ScuffedAnticheatMod.instance.GetPacket();
            packet.Write((byte)MessageType.SendHandshake);
            packet.Send(playerNumber);
        }

        public static void SetupPlayer(ref BinaryReader reader, int playerNumber)
        {
            guids[playerNumber] = reader.ReadNullTerminatedString();
            playerInventories[playerNumber] = PlayerData.LoadPlayer(Main.player[playerNumber].name, guids[playerNumber]);
        }

        // Evaluates item equality based off of type, prefix, and stack
        protected static bool IsIdentical(EzItem item1, EzItem item2)
        {
            return item1.type == item2.type && item1.prefix == item2.prefix && item1.stack == item2.stack;
        }

        // Return all deleted items in player inventory
        public static List<Item> GetPlayerItems(int whoAmI)
        {
            List<Item> matchingItems = new List<Item>();
            foreach (DeletedItem item in deletedItems)
                if (item.owner == Main.player[whoAmI].name && item.guid == guids[whoAmI] && item.worldID == Main.worldID)
                    matchingItems.Add(item.item.GetClone());
            return matchingItems;
        }

        // Find matching guid and return playerNumber; if none found return -1
        public static int GetPlayerNumberFromID(string id)
        {
            string targetguid = id[^36..];
            for (int i = 0; i < guids.Length; i++)
                if (targetguid == guids[i])
                    return i;
            return -1;
        }
    }
}
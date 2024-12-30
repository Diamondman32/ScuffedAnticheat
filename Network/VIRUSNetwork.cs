using System.IO;
using System.Collections.Generic;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.Chat;
using Terraria.Localization;
using Terraria.ID;
using Terraria.ModLoader;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace VIRUS.Network
{
    // Enums
    public enum MessageType { CheckMyInventory, UpdateSaveData, ReplaceItem }
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

    public class VIRUSNetwork
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

        // Sorts SAM messages based off of their message type
        public static void HandlePacket(BinaryReader reader, int playerNumber)
		{
            //We found a VIRUS only message
            MessageType msgType = (MessageType)reader.ReadByte();
            switch(msgType)
            {
                case MessageType.CheckMyInventory:
                    CheckInventory.ProcessCheckInventory(playerNumber);
                    break;
                case MessageType.UpdateSaveData:
                    UpdateSaveData.ProcessUpdateInventory(ref reader);
                    break;
                case MessageType.ReplaceItem:
                    ModifyPlayerData.ProcessModifyItem(ref reader);
                    break;
            }
		}

        // Evaluates item equality based off of type, prefix, and stack
        public static bool IsIdentical(EzItem item1, EzItem item2)
        {
            if(item1.type == item2.type && item1.prefix == item2.prefix && item1.stack == item2.stack)
                return true;
            else
                return false;
        }

        // Sends packet based off of parameters
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
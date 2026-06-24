using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using ScuffedAnticheatMod.Network;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

// TODO: maybe make seperate files for worlds
namespace ScuffedAnticheatMod
{
    public class PlayerData
    {
        private static string dbPath { get; } = Path.Combine(Main.SavePath, "ScuffedAnticheatMod", "PlayerData.db");
        private static SqliteConnection connection;

        private static SqliteConnection GetConnection()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath));
            SqliteConnection connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            return connection;
        }

        public static void Initialize()
        {
            connection = GetConnection();

            SqliteCommand playerTable = connection.CreateCommand();
            playerTable.CommandText =
            @"
                CREATE TABLE IF NOT EXISTS Players (
                    PlayerID INTEGER PRIMARY KEY NOT NULL,
                    ClientID TEXT NOT NULL,
                    PlayerName TEXT NOT NULL,
                    WorldID INTEGER NOT NULL,
                    XPos REAL NOT NULL,
                    YPos REAL NOT NULL
                )
            ";
            playerTable.ExecuteNonQuery();

            SqliteCommand itemTable = connection.CreateCommand();
            itemTable.CommandText =
            @"
                CREATE TABLE IF NOT EXISTS Items (
                    Slot INTEGER NOT NULL,
                    Type INTEGER NOT NULL,
                    Stack INTEGER NOT NULL,
                    Prefix INTEGER NOT NULL,
                    Favorited INTEGER NOT NULL,
                    PlayerID INTEGER NOT NULL,

                    UNIQUE(PlayerID, Slot),

                    FOREIGN KEY(PlayerID) 
                        REFERENCES Players(PlayerID)
                        ON DELETE CASCADE
                )
            ";
            itemTable.ExecuteNonQuery();

            SqliteCommand deletedItemTable = connection.CreateCommand();
            deletedItemTable.CommandText =
            @"
                CREATE TABLE IF NOT EXISTS DeletedItems (
                    Type INTEGER NOT NULL,
                    Stack INTEGER NOT NULL,
                    Prefix INTEGER NOT NULL,
                    Favorited INTEGER NOT NULL,
                    PlayerName TEXT NOT NULL,
                    ClientID TEXT NOT NULL,
                    WorldID INTEGER NOT NULL
                )
            ";
            deletedItemTable.ExecuteNonQuery();
        }

        public static PlayerInventory LoadPlayer(string name, string guid)
        {
            // Get playerID to find items and get location
            SqliteCommand loadPlayer = connection.CreateCommand();
            loadPlayer.Parameters.AddWithValue("$guid", guid);
            loadPlayer.Parameters.AddWithValue("$name", name);
            loadPlayer.Parameters.AddWithValue("$worldID", Main.worldID);
            loadPlayer.CommandText =
            @"
                SELECT 
                    PlayerID,
                    XPos,
                    YPos
                FROM
                    Players
                WHERE 
                    ClientID = $guid
                    AND PlayerName = $name
                    AND WorldID = $worldID;
            ";

            using SqliteDataReader loadPlayerReader = loadPlayer.ExecuteReader();
            if (loadPlayerReader.Read())
            {
                float xPos = loadPlayerReader.GetFloat(1);
                float yPos = loadPlayerReader.GetFloat(2);

                // Use playerID to get items
                SqliteCommand loadItems = connection.CreateCommand();
                loadItems.Parameters.AddWithValue("$playerID", loadPlayerReader.GetInt32(0));
                loadItems.CommandText =
                @"
                    SELECT 
                        Slot,
                        Type,
                        Stack,
                        Prefix,
                        Favorited
                    FROM
                        Items
                    WHERE 
                        PlayerID = $playerID
                    ORDER BY 
                        Slot
                ";

                EzItem[] inventory = InitializeArray(59);
                EzItem[] bank1 = InitializeArray(40);
                EzItem[] bank2 = InitializeArray(40);
                EzItem[] bank3 = InitializeArray(40);
                EzItem[] bank4 = InitializeArray(40);
                EzItem[] armor = InitializeArray(20);
                EzItem[] dye = InitializeArray(10);
                EzItem[] miscEquips = InitializeArray(5);
                EzItem[] miscDyes = InitializeArray(5);
                EzItem[] trash = InitializeArray(1);

                using SqliteDataReader r = loadItems.ExecuteReader();
                while (r.Read())
                {
                    int slotType = r.GetInt32(0);
                    if (slotType >= PlayerItemSlotID.Bank4_0)
                    {
                        bank4[slotType - PlayerItemSlotID.Bank4_0] = new(r.GetInt32(1), r.GetInt32(2), r.GetInt32(3), r.GetBoolean(4));
                    }
                    else if (slotType >= PlayerItemSlotID.Bank3_0)
                    {
                        bank3[slotType - PlayerItemSlotID.Bank3_0] = new(r.GetInt32(1), r.GetInt32(2), r.GetInt32(3), r.GetBoolean(4));
                    }
                    else if (slotType >= PlayerItemSlotID.TrashItem)
                    {
                        trash[slotType - PlayerItemSlotID.TrashItem] = new(r.GetInt32(1), r.GetInt32(2), r.GetInt32(3), r.GetBoolean(4));
                    }
                    else if (slotType >= PlayerItemSlotID.Bank2_0)
                    {
                        bank2[slotType - PlayerItemSlotID.Bank2_0] = new(r.GetInt32(1), r.GetInt32(2), r.GetInt32(3), r.GetBoolean(4));
                    }
                    else if (slotType >= PlayerItemSlotID.Bank1_0)
                    {
                        bank1[slotType - PlayerItemSlotID.Bank1_0] = new(r.GetInt32(1), r.GetInt32(2), r.GetInt32(3), r.GetBoolean(4));
                    }
                    else if (slotType >= PlayerItemSlotID.MiscDye0)
                    {
                        miscDyes[slotType - PlayerItemSlotID.MiscDye0] = new(r.GetInt32(1), r.GetInt32(2), r.GetInt32(3), r.GetBoolean(4));
                    }
                    else if (slotType >= PlayerItemSlotID.Misc0)
                    {
                        miscEquips[slotType - PlayerItemSlotID.Misc0] = new(r.GetInt32(1), r.GetInt32(2), r.GetInt32(3), r.GetBoolean(4));
                    }
                    else if (slotType >= PlayerItemSlotID.Dye0)
                    {
                        dye[slotType - PlayerItemSlotID.Dye0] = new(r.GetInt32(1), r.GetInt32(2), r.GetInt32(3), r.GetBoolean(4));
                    }
                    else if (slotType >= PlayerItemSlotID.Armor0)
                    {
                        armor[slotType - PlayerItemSlotID.Armor0] = new(r.GetInt32(1), r.GetInt32(2), r.GetInt32(3), r.GetBoolean(4));
                    }
                    else
                    {
                        inventory[slotType - PlayerItemSlotID.Inventory0] = new(r.GetInt32(1), r.GetInt32(2), r.GetInt32(3), r.GetBoolean(4));
                    }
                }

                return new PlayerInventory(name, guid, Main.worldID, xPos, yPos, inventory, bank1, bank2, bank3, bank4, armor, dye, miscEquips, miscDyes, trash);
            }
            // If no read then a new character is joining (or data corruption lol)
            CreateNewPlayer(name, guid);
            return new PlayerInventory(name, guid);
        }

        private static EzItem[] InitializeArray(int size)
        {
            EzItem[] arr = new EzItem[size];
            for (int i = 0; i < size; i++)
                arr[i] = new EzItem(new Item(ItemID.None));
            return arr;
        }

        private static void CreateNewPlayer(string name, string guid)
        {
            using SqliteTransaction transaction = connection.BeginTransaction();

            // Add new player
            SqliteCommand cmd = connection.CreateCommand();
            cmd.Transaction = transaction;
            cmd.Parameters.AddWithValue("$guid", guid);
            cmd.Parameters.AddWithValue("$name", name);
            cmd.Parameters.AddWithValue("$worldID", Main.worldID);
            cmd.Parameters.AddWithValue("$xPos", Main.spawnTileX * 16);
            cmd.Parameters.AddWithValue("$yPos", (Main.spawnTileY - 3) * 16);
            cmd.CommandText =
            @"
                INSERT INTO Players (ClientID, PlayerName, WorldID, XPos, YPos)
                Values($guid, $name, $worldID, $xPos, $yPos)
            ";
            cmd.ExecuteNonQuery();

            List<Item> startingItems = PlayerLoader.GetStartingItems(new Player(), [ new(ItemID.CopperShortsword), new(ItemID.CopperPickaxe), new(ItemID.CopperAxe) ]);
            int playerID = GetPlayerID(name, guid, transaction);

            for (int i = 0; i < startingItems.Count; i++)
            {
                if (startingItems[i].type != ItemID.None)
                    UpsertItem(playerID, new EzItem(startingItems[i]), i, transaction);
            }

            transaction.Commit();
        }

        private static int GetPlayerID(string name, string guid, SqliteTransaction transaction)
        {
            SqliteCommand cmd = connection.CreateCommand();
            cmd.Transaction = transaction;
            cmd.Parameters.AddWithValue("$name", name);
            cmd.Parameters.AddWithValue("$guid", guid);
            cmd.Parameters.AddWithValue("$worldID", Main.worldID);
            cmd.CommandText = 
            @"
                SELECT PlayerID FROM Players
                WHERE
                    PlayerName = $name
                    AND ClientID = $guid
                    AND WorldID = $worldID
            ";

            using SqliteDataReader reader = cmd.ExecuteReader();
            if (!reader.Read())
                throw new Exception("Player doesn't exist.");
            return reader.GetInt32(0);
        }

        public static void UpsertItem(string name, string guid, EzItem item, int slot)
        {
            using SqliteTransaction transaction = connection.BeginTransaction();
            int playerID = GetPlayerID(name, guid, transaction);
            UpsertItem(playerID, item, slot, transaction);
            transaction.Commit();
        }

        private static void UpsertItem(int playerID, EzItem item, int slot, SqliteTransaction transaction)
        {
            // Update or remove row
            SqliteCommand cmd = connection.CreateCommand();
            cmd.Transaction = transaction;
            cmd.Parameters.AddWithValue("$slot", slot);
            cmd.Parameters.AddWithValue("$playerID", playerID);

            // Update item row
            if (item.type != ItemID.None)
            {
                cmd.Parameters.AddWithValue("$type", item.type);
                cmd.Parameters.AddWithValue("$stack", item.stack);
                cmd.Parameters.AddWithValue("$prefix", item.prefix);
                cmd.Parameters.AddWithValue("$favorited", item.favorited);
                cmd.CommandText = 
                @"
                    INSERT INTO Items(Slot, Type, Stack, Prefix, Favorited, PlayerID)
                    VALUES($slot, $type, $stack, $prefix, $favorited, $playerID)
                    ON CONFLICT(PlayerID, Slot) DO UPDATE SET
                        Type = excluded.Type,
                        Stack = excluded.Stack,
                        Prefix = excluded.Prefix,
                        Favorited = excluded.Favorited,
                        PlayerID = excluded.PlayerID
                ";
            }
            // Delete item row
            else
            {
                cmd.CommandText = 
                @"
                    DELETE FROM Items
                    WHERE
                        PlayerID = $playerID
                        AND Slot = $slot
                ";
            }
            cmd.ExecuteNonQuery();
        }

        public static void AddDeletedItem(DeletedItem dItem)
        {
            using SqliteTransaction transaction = connection.BeginTransaction();
            SqliteCommand cmd = connection.CreateCommand();
            cmd.Transaction = transaction;
            cmd.Parameters.AddWithValue("$type", dItem.item.type);
            cmd.Parameters.AddWithValue("$stack", dItem.item.stack);
            cmd.Parameters.AddWithValue("$prefix", dItem.item.prefix);
            cmd.Parameters.AddWithValue("$favorited", dItem.item.favorited);
            cmd.Parameters.AddWithValue("$name", dItem.owner);
            cmd.Parameters.AddWithValue("$guid", dItem.guid);
            cmd.Parameters.AddWithValue("$worldID", dItem.worldID);
            cmd.CommandText = 
            @"
                INSERT INTO DeletedItems(Type, Stack, Prefix, Favorited, PlayerName, ClientID, WorldID)
                VALUES($slot, $type, $stack, $prefix, $favorited, $name, $guid, $worldID)
            ";
            transaction.Commit();
        }

        public static void RemoveDeletedItem(DeletedItem dItem)
        {
            using SqliteTransaction transaction = connection.BeginTransaction();
            SqliteCommand cmd = connection.CreateCommand();
            cmd.Transaction = transaction;
            cmd.Parameters.AddWithValue("$type", dItem.item.type);
            cmd.Parameters.AddWithValue("$stack", dItem.item.stack);
            cmd.Parameters.AddWithValue("$prefix", dItem.item.prefix);
            cmd.Parameters.AddWithValue("$favorited", dItem.item.favorited);
            cmd.Parameters.AddWithValue("$name", dItem.owner);
            cmd.Parameters.AddWithValue("$guid", dItem.guid);
            cmd.Parameters.AddWithValue("$worldID", dItem.worldID);
            cmd.CommandText = 
            @"
                DELETE FROM DeletedItems
                WHERE
                    ClientID = $guid
                    AND PlayerName = $name
                    AND WorldID = $worldID
                    AND Type = $type
                    AND Stack = $stack
                    AND Prefix = $prefix
                    AND Favorited = $favorited
            ";
            transaction.Commit();
        }
    }
}
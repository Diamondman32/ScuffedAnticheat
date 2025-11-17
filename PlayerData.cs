using System.IO;
using Microsoft.Data.Sqlite;
using Terraria;

namespace ScuffedAnticheatMod
{
    public class PlayerData
    {
        private static string dbPath { get; } = Main.SavePath + Path.DirectorySeparatorChar + "SAM_PlayerData" + ".db";

        public static SqliteConnection GetConnection()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath));

            SqliteConnection connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            return connection;
        }

        public static void Initialize()
        {
            using SqliteConnection connection = GetConnection();
            using SqliteCommand cmd = connection.CreateCommand();

            cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Players (
                PlayerID TEXT PRIMARY KEY,
                PlayerName TEXT,
                WorldID INTEGER,
                XPos REAL,
                YPos REAL
            );

            CREATE TABLE IF NOT EXISTS Items (
                ItemID INTEGER PRIMARY KEY AUTOINCREMENT,
                PlayerID TEXT,
                Container TEXT,
                Slot INTEGER,
                Type INTEGER,
                Stack INTEGER,
                Prefix INTEGER,
                Favorited INTEGER,
                FOREIGN KEY(PlayerID) REFERENCES Players(PlayerID)
            );
            ";

            // LOAD
            string a = @"
            SELECT * FROM Items
            HAVING 
                $PlayerID
            ORDER BY 
                slot

            SELECT * FROM Players
            ";

            cmd.ExecuteNonQuery();
        }
    }
}
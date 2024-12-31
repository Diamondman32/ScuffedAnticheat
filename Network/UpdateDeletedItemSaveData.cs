using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Terraria;

namespace ScuffedAnticheatMod.Network
{
    public class UpdateDeletedItemSaveData : SAMNetwork
    {
        // Reads incoming UpdateSaveData packets and updates save data accordingly
        public static void ProcessUpdateItemSaveData(ref BinaryReader reader)
        {
            int playerNum = reader.ReadByte();
            Item newItem = Terraria.ModLoader.IO.ItemIO.Receive(reader, true, true);

            string playerName = Main.player[playerNum].name;
            List<DeletedItem> savedItems = Deserialize();

            for(int i=0;i<savedItems.Count;i++)
                if(savedItems[i].owner == playerName && IsIdentical(new(newItem), savedItems[i].item))
                {
                    savedItems.RemoveAt(i);
                    i--;
                }

            string json = JsonConvert.SerializeObject(savedItems);
            using StreamWriter outputFile = new(DiscardItemDataPath);
            outputFile.WriteLine(json);
            outputFile.Close();
        }

        // Returns entire deserialized json data array
        private static List<DeletedItem> Deserialize()
        {
            List<DeletedItem> yeah = new();
            if(File.Exists(DiscardItemDataPath))
            {
                using StreamReader r = new(DiscardItemDataPath);
                string json = r.ReadToEnd();
                r.Close();
                if(!string.IsNullOrEmpty(json))
                    yeah = JsonConvert.DeserializeObject<List<DeletedItem>>(json) ?? new List<DeletedItem>();
            }
            else
                File.Create(DiscardItemDataPath);
            return yeah;
        }
    }
}
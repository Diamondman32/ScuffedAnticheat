using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Terraria;

namespace ScuffedAnticheatMod.Network
{
        /* SERVER */
    public class UpdateDeletedItemSaveData : SAMNetwork
    {
        // Reads incoming UpdateSaveData packets and updates save data accordingly
        public static void ProcessUpdateItemSaveData(ref BinaryReader reader, int playerNum)
        {
            Item newItem = Terraria.ModLoader.IO.ItemIO.Receive(reader, true, true);

            string playerName = Main.player[playerNum].name;

            for(int i=0;i<deletedItems.Count;i++)
                if(deletedItems[i].owner == playerName && deletedItems[i].guid == guids[playerNum] && deletedItems[i].worldID == Main.worldID && IsIdentical(new(newItem), deletedItems[i].item))
                {
                    deletedItems.RemoveAt(i);
                    i--;
                }

            string json = JsonConvert.SerializeObject(deletedItems);
            using StreamWriter outputFile = new(DiscardItemDataPath);
            outputFile.WriteLine(json);
            outputFile.Close();
        }
    }
}
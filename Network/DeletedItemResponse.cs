using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader.IO;

namespace ScuffedAnticheatMod.Network
{
    public class DeletedItemReponse : SAMNetwork
    {
        private static List<Item> deletedItems;
        public static List<Item> GetDeletedItems() { return deletedItems; }

        // Receives packets and replaces designated item with the correct item
        public static void ProcessResponse(ref BinaryReader reader)
        {
            int numItems = reader.ReadByte();

            List<Item> deletedItemsTemp = new();
            for(int i=0;i<numItems;i++)
                deletedItems.Add(ItemIO.Receive(reader, true, true));

            deletedItems = deletedItemsTemp;
        }
    }
}
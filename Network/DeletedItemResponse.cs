using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader.IO;

namespace ScuffedAnticheatMod.Network
{
    public class DeletedItemReponse : SAMNetwork
    {
            /* CLIENT */
        public static List<Item> targetDeletedItems { get; private set; } = new List<Item>();
        public static bool itemsReceived { get; private set; } = false;

        // Receives packets and replaces designated item with the correct item
        public static void ProcessResponse(ref BinaryReader reader)
        {
            int numItems = reader.ReadByte();

            for(int i=0;i<numItems;i++)
                targetDeletedItems.Add(ItemIO.Receive(reader, true, true));
            itemsReceived = true;
        }

        // Set client-side deletedItem array to null
        public static void ResetItemArray()
        {
            targetDeletedItems.Clear();
            itemsReceived = false;
        }

        // Remove element from array
        public static void RemoveElement(Item item)
        {
            targetDeletedItems.Remove(item);
        }
    }
}
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader.IO;

namespace ScuffedAnticheatMod.Network
{
    public class DeletedItemReponse : SAMNetwork
    {
            /* CLIENT */
        public static List<Item> targetDeletedItems { get; protected set; }

        // Receives packets and replaces designated item with the correct item
        public static void ProcessResponse(ref BinaryReader reader)
        {
            int numItems = reader.ReadByte();

            targetDeletedItems = null;
            for(int i=0;i<numItems;i++)
                targetDeletedItems.Add(ItemIO.Receive(reader, true, true));
        }

        // Set client-side deletedItem array to null
        public static void ResetItemArray()
        {
            targetDeletedItems = null;
        }
    }
}
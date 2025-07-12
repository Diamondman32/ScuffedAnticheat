using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ScuffedAnticheatMod.Network
{
    public class RequestDeletedItems : SAMNetwork
    {
        /*  CLIENT  */
        // Sends packet to server asking for a specific players deleted items
        public static void AskNicelyForPlayersDeletedItems(int targetPlayerNum)
        {
            ReceiveDeletedItems.ResetItemArray();
            var packet = ScuffedAnticheatMod.instance.GetPacket();
            packet.Write((byte)MessageType.DeletedItemRequest);
            packet.Write((byte)targetPlayerNum);
            packet.Send(255); // To server
        }
        
        /*  SERVER  */
        // Receives packets and sends target's deleted items
        public static void ProcessRequest(ref BinaryReader reader, int playerNum)
        {
            int targetNum = reader.ReadByte();
            List<Item> items = GetPlayerItems(targetNum);

            var packet = ScuffedAnticheatMod.instance.GetPacket();
            packet.Write((byte)MessageType.ReceiveDeletedItems);
            packet.Write((byte)items.Count);
            foreach (Item item in items)
                ItemIO.Send(item, packet, true, true);
            packet.Send(playerNum);
        }
    }
    public class ReceiveDeletedItems : SAMNetwork
    {
        /*  CLIENT  */
        public static List<Item> playerDeletedItems { get; private set; } = new List<Item>();
        public static bool itemsReceived { get; private set; } = false;

        // Receives packets and replaces designated item with the correct item
        public static void ProcessRequest(ref BinaryReader reader)
        {
            int numItems = reader.ReadByte();

            for (int i = 0; i < numItems; i++)
                playerDeletedItems.Add(ItemIO.Receive(reader, true, true));
            itemsReceived = true;
        }

        // Set client-side deletedItem array to null
        public static void ResetItemArray()
        {
            playerDeletedItems.Clear();
            itemsReceived = false;
        }

        // Remove element from array
        public static void RemoveElement(Item item)
        {
            playerDeletedItems.Remove(item);
        }
    }
    public class SyncDeletedItems : SAMNetwork
    {
        /*  CLIENT  */
        // Refreshes item list UI if it is open
        public static void ProcessRequest()
        {
            ModContent.GetInstance<UI.SAMModSystem>()?.playerListWindow?.playerListWindow?.playerItemWindow?.itemListPanel?.RefreshItemList();
        }

        /*  SERVER  */
        // Sends a packet to the server which refreshes all other clients' local copies of deleted items
        public static void SyncClients(int senderNum)
        {
            var packet = ScuffedAnticheatMod.instance.GetPacket();
            packet.Write((byte)MessageType.SyncDeletedItems);
            packet.Send(ignoreClient: senderNum);
        }
    }
}
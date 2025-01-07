using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader.IO;

namespace ScuffedAnticheatMod.Network
{
    public class RequestDeletedItems : SAMNetwork
    {
            /* SERVER */
        // Receives packets and sends target's deleted items
        public static void ProcessDeletedItemRequest(ref BinaryReader reader, int playerNum)
        {
            int targetNum = reader.ReadByte();
            List<Item> items = GetPlayerItems(targetNum);

            var packet = ScuffedAnticheatMod.instance.GetPacket();
            packet.Write((byte)MessageType.DeletedItemResponse);
            packet.Write((byte)items.Count);
            foreach(Item item in items)
                ItemIO.Send(item, packet, true, true);
            packet.Send(playerNum);
        }

            /* CLIENT */
        // Sends packet to server asking for a specific players deleted items
        public static void AskNicelyForPlayersDeletedItems(int targetPlayerNum)
        {
            DeletedItemReponse.ResetItemArray();
            var packet = ScuffedAnticheatMod.instance.GetPacket();
            packet.Write((byte)MessageType.DeletedItemRequest);
            packet.Write((byte)targetPlayerNum);
            packet.Send(255); // To server
        }
    }
}
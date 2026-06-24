using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace ScuffedAnticheatMod.Network
{
    public class UpdateDeletedItemSaveData : SAMNetwork
    {
        /*  SERVER  */
        // Removes matching player-item from saved items when instructed by client
        public static void ProcessRequest(ref BinaryReader reader, int senderNum)
        {
            int targetNum = reader.ReadByte();
            Item item = ItemIO.Receive(reader, true, true);
            string playerName = Main.player[targetNum].name;

            int index = deletedItems.FindIndex(x => x.owner == playerName && x.guid == guids[targetNum] && x.worldID == Main.worldID && IsIdentical(new(item), x.item));
            if (index != -1)
            {
                PlayerData.RemoveDeletedItem(deletedItems[index]);
                deletedItems.RemoveAt(index);
            }
            else
            {
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Deleted item removal attempt failed."), Color.Red);
                ScuffedAnticheatMod.instance.Logger.Error($"Deleted item removal attempt failed. Owner: {playerName}, Item: {item.Name}");
            }

            SyncDeletedItems.SyncClients(senderNum);
        }

        /*  CLIENT  */
        // Remove matching player-item from saved items and tells server to do the same
        public static void DeleteItem(Item item, int targetNum)
        {
            ReceiveDeletedItems.RemoveElement(item);

            var packet = ScuffedAnticheatMod.instance.GetPacket();
            packet.Write((byte)MessageType.UpdateDeletedItemSaveData);
            packet.Write((byte)targetNum);
            ItemIO.Send(item, packet, true, true);
            packet.Send();
        }
    }
}
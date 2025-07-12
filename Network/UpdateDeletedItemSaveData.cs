using System.IO;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Terraria;
using Terraria.Chat;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace ScuffedAnticheatMod.Network
{
    public class UpdateDeletedItemSaveData : SAMNetwork
    {
        /*  SERVER  */
        // Reads incoming UpdateSaveData packets and updates save data accordingly
        public static void ProcessUpdateItemSaveData(ref BinaryReader reader, int senderNum)
        {
            int targetNum = reader.ReadByte();
            Item item = ItemIO.Receive(reader, true, true);
            string playerName = Main.player[targetNum].name;

            int index = deletedItems.FindIndex(x => x.owner == playerName && x.guid == guids[targetNum] && x.worldID == Main.worldID && IsIdentical(new(item), x.item));
            if (index != -1)
                deletedItems.RemoveAt(index);
            else
            {
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Deleted item removal attempt failed."), Color.Red);
                ScuffedAnticheatMod.instance.Logger.Error($"Deleted item removal attempt failed. Owner: {playerName}, Item: {item.Name}, guid: {guids[targetNum]}");
            }

            string json = JsonConvert.SerializeObject(deletedItems);
            using StreamWriter outputFile = new(DiscardItemDataPath);
            outputFile.WriteLine(json);
            outputFile.Close();

            SyncDeletedItems.SyncClients(senderNum);
        }

        /*  CLIENT  */
        // TODO move this somewhere else
        // Sends packet with desired item to return to player
        public static void ReturnItemToPlayer(Item item, int targetNum)
        {
            if (targetNum == Main.myPlayer)
                ModifyPlayerData.ReplaceFirstOpenSlot(item);
            else
            {
                var packet = ScuffedAnticheatMod.instance.GetPacket();
                packet.Write((byte)MessageType.ReplaceItem);
                packet.Write((byte)targetNum);
                packet.Write((byte)ItemCategory.FindFirstOpenInv);
                ItemIO.Send(item, packet, true, true);
                packet.Send();
            }
        }

        // Remove matching player-item from saved items
        public static void DeleteItem(Item item, int targetNum)
        {
            var packet = ScuffedAnticheatMod.instance.GetPacket();
            packet.Write((byte)MessageType.UpdateDeletedItemSaveData);
            packet.Write((byte)targetNum);
            ItemIO.Send(item, packet, true, true);
            packet.Send();
        }
    }
}
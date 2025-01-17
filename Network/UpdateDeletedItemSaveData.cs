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
            /* SERVER */
        // Reads incoming UpdateSaveData packets and updates save data accordingly
        public static void ProcessUpdateItemSaveData(ref BinaryReader reader, int senderNum)
        {
            int targetNum = reader.ReadByte();
            Item newItem = ItemIO.Receive(reader, true, true);
            string playerName = Main.player[targetNum].name;
            
            int index = deletedItems.FindIndex(x => x.owner == playerName && x.guid == guids[targetNum] && x.worldID == Main.worldID && IsIdentical(new(newItem), x.item));
            if(index != -1)
                deletedItems.RemoveAt(index);
            else
            {
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Deleted item removal attempt failed."), Color.Red);
                ScuffedAnticheatMod.instance.Logger.Error($"Deleted item removal attempt failed. Owner: {playerName}, Item: {newItem.Name}, guid: {guids[targetNum]}");
            }

            string json = JsonConvert.SerializeObject(deletedItems);
            using StreamWriter outputFile = new(DiscardItemDataPath);
            outputFile.WriteLine(json);
            outputFile.Close();

            SyncDeletedItems.SyncClients(senderNum);
        }

            /* CLIENT */
        // Sends packet with desired item to return to player and removes item from save data
        public static void ReturnItemToPlayer(Item item, int targetNum)
        {
            // Give item to player
            if(targetNum == Main.myPlayer)
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

            // Remove item from saved items
            var packet2 = ScuffedAnticheatMod.instance.GetPacket();
            packet2.Write((byte)MessageType.UpdateDeletedItemSaveData);
            packet2.Write((byte)targetNum);
            ItemIO.Send(item, packet2, true, true);
            packet2.Send();
        }
    }
}
using Terraria.ModLoader;

namespace ScuffedAnticheatMod.Network
{
    public class SyncDeletedItems : SAMNetwork
    {
            /* CLIENT */
        // Refreshes item list UI
        public static void ProcessSync()
        {
            ModContent.GetInstance<UI.SAMModSystem>()?.playerListWindow?.playerListWindow?.playerItemWindow?.itemListPanel?.RefreshItemList();
        }

            /* SERVER */
        // Sends a packet to the server which refreshes all other clients' local copies of deleted items
        public static void SyncClients(int senderNum)
        {
            var packet = ScuffedAnticheatMod.instance.GetPacket();
            packet.Write((byte)MessageType.SyncDeletedItems);
            packet.Send(ignoreClient: senderNum);
        }
    }
}
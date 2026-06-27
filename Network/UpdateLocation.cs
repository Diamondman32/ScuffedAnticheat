using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;

namespace ScuffedAnticheatMod.Network
{
    public class UpdateLocation : SAMNetwork
    {
        /*  SERVER  */
        // Sends spawn location to player
        public static void SendPacket(int playerNumber)
        {
            var packet = ScuffedAnticheatMod.instance.GetPacket();
            packet.Write((byte)MessageType.ReceiveLocation);

            PlayerInventory player = playerInventories[playerNumber];
            packet.WriteVector2(new Vector2(player.xPos, player.yPos));
            
            packet.Send(playerNumber);
        }

        /*  CLIENT  */
        // Teleports player to last location
        // TODO: doubly need to get rid of waiting logic so there is no screen pull on world join
        public static void ProcessRequest(ref BinaryReader reader)
        {
            _ = WaitForActivePlayer(reader.ReadVector2());
        }

        private static async Task WaitForActivePlayer(Vector2 position)
        {
            Player player = Main.LocalPlayer;
            _ = Task.Run(async () =>
            {
                int timeout = 60000;
                int interval = 1000;
                int elapsed = 0;

                while ((player == null || !player.active) && elapsed < timeout)
                {
                    await Task.Delay(interval);
                    elapsed += interval;
                }

                if (elapsed < timeout && player != null)
                {
                    Main.LocalPlayer.Teleport(position, -1);
                }
            });
        }
    }
}
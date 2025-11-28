using System.IO;
using Microsoft.Xna.Framework;
using Terraria;


namespace ScuffedAnticheatMod.Network
{
    public class ReceiveLocation : SAMNetwork
    {
        /*  CLIENT  */
        // Teleports player to last location
        public static void ProcessRequest(ref BinaryReader reader)
        {
            Main.LocalPlayer.Teleport(reader.ReadVector2(), -1);
        }
    }

    public class RequestLocation : SAMNetwork
    {
        /*  SERVER  */
        // Finds location and sends it back
        public static void ProcessRequest(int playerNumber)
        {
            var packet = ScuffedAnticheatMod.instance.GetPacket();
            packet.Write((byte)MessageType.ReceiveLocation);

            PlayerInventory player = playerInventories[playerNumber];
            packet.WriteVector2(new Vector2(player.xPos, player.yPos));
            
            packet.Send(playerNumber);
        }

        // Requests correct location to move player
        public static void SendPacket()
        {
            var packet = ScuffedAnticheatMod.instance.GetPacket();
            packet.Write((byte)MessageType.RequestLocation);
            packet.Send(255); // Send to server
        }
    }
}
using System.IO;
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
        public static void ProcessRequest(ref BinaryReader reader)
        {
            Main.LocalPlayer.Teleport(reader.ReadVector2(), -1);
        }
    }
}
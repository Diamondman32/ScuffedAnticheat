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
        public static void ProcessRequest(ref BinaryReader reader)
        {
            OnEnterWorld.AddEnterWorldAction(OnEnterWorld.ActionTypes.UpdateLocation, reader.ReadVector2());
        }
    }
}
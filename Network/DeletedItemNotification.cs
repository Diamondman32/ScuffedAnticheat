using System.IO;
using Microsoft.Xna.Framework;
using MonoMod.Utils;

namespace ScuffedAnticheatMod.Network
{
    public class DeletedItemNotification : SAMNetwork
    {
        /*  CLIENT  */
        // Adds chat notification to queue
        public static void ProcessRequest(ref BinaryReader reader)
        {
            OnEnterWorld.AddChatNotification(reader.ReadNullTerminatedString(), Color.Purple);
        }
    }
}
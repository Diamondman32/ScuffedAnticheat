using System;
using System.IO;
using Terraria.ID;
using Terraria;
using Terraria.Localization;
using System.Collections.Generic;
using MonoMod.Utils;

namespace ScuffedAnticheatMod.Network
{
    public class CheckMods : SAMNetwork
    {
        private static List<string> serverHashList = null;

        /*  CLIENT  */
        // Sends a hash of all enabled mods to server
        public static void SendPacket()
        {
            var packet = ScuffedAnticheatMod.instance.GetPacket();
            packet.Write((byte)MessageType.ReceiveHandshake);
            packet.WriteNullTerminatedString(Guid.guid);
            packet.Write((byte)ScuffedAnticheatMod.modsHashedSuccessfully.ToInt());
            if (ScuffedAnticheatMod.modsHashedSuccessfully)
            {
                packet.Write((byte)ScuffedAnticheatMod.modHashes.Count);
                foreach (byte[] hash in ScuffedAnticheatMod.modHashes)
                {
                    if (hash == null) continue;
                    packet.Write((byte)hash.Length);
                    packet.Write(hash);
                }
            }
            packet.Send(255); // Send to server
        }

        /*  SERVER  */
        // Checks each user installed mod for a matching mod on the serverlist
        public static void ProcessRequest(ref BinaryReader reader, int playerNumber)
        {
            List<string> clientHashList = new();
            serverHashList ??= BytesToString(ScuffedAnticheatMod.modHashes);

            bool modsHashedSuccessfully = reader.ReadByte() == 1;
            if (modsHashedSuccessfully)
            {
                int numMods = reader.ReadByte();
                for (int i = 0; i < numMods; i++)
                {
                    int numBytes = reader.ReadByte();
                    string hash = BytesToString(reader.ReadBytes(numBytes));
                    clientHashList.Add(hash);
                }
            }
            else
            {
                NetMessage.SendData(MessageID.Kick, playerNumber, -1, NetworkText.FromLiteral("Mod hashing failed"));
                return;
            }

            foreach (string hash in clientHashList)
            {
                if (!serverHashList.Contains(hash))
                {
                    NetMessage.SendData(MessageID.Kick, playerNumber, -1, NetworkText.FromLiteral("Mods are incompatible with the server. Please confirm mods with server host"));
                    return;
                }
            }
        }

        private static string BytesToString(byte[] hashBytes)
        {
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        private static List<string> BytesToString(List<byte[]> hashBytesList)
        {
            List<string> hashes = new();
            foreach (byte[] hashBytes in hashBytesList)
                hashes.Add(BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant());
            return hashes;
        }
    }
}
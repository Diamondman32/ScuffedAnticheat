using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria;

namespace VIRUS
{
    // Enums
    public enum MessageType { CheckInventory, UpdateInventory }
    //

    public class Network
    {
        // Network
        public static void HEROsModMessaged(BinaryReader reader, int playerNumber)
		{
            //We found a VIRUS only message
            MessageType msgType = (MessageType)reader.ReadByte();
            switch(msgType)
            {
                case MessageType.CheckInventory:
                    ProcessCheckInventory(ref reader, playerNumber);
                    break;
                case MessageType.UpdateInventory:
                    ProcessUpdateInventory(ref reader, playerNumber);
                    break;
            }
		}

        // General Messages
        public static void ProcessCheckInventory(ref BinaryReader reader, int playerNumber)
        {
            // Receive Data
            int senderNum = reader.ReadByte();

            List<Item> inventory = [];
            for(int i=0;i<59;i++)
            {
                inventory.Add(Terraria.ModLoader.IO.ItemIO.Receive(reader, true));
            }
        }
        public static void ProcessUpdateInventory(ref BinaryReader reader, int playerNumber)
        {

        }
        public List<PlayerInventory> Deserialize()
		{
			using(StreamReader r = new StreamReader("aFile.json"))
			{
				string json = r.ReadToEnd();
				return JsonSerializer.Deserialize<List<PlayerInventory>>(json);
			}
		}
    }
}
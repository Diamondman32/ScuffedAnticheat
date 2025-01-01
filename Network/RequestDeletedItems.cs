using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Terraria;
using Terraria.ModLoader.IO;

namespace ScuffedAnticheatMod.Network
{
    public class RequestDeletedItems : SAMNetwork
    {
        // Receives packets and replaces designated item with the correct item
        public static void ProcessDeletedItemRequest(ref BinaryReader reader)
        {
            int playerNum = reader.ReadByte();
            int targetNum = reader.ReadByte();

            // Player recipient = Main.player[playerNum];
            Player target = Main.player[targetNum];

            List<Item> items = Deserialize(target.name);

            var packet = ScuffedAnticheatMod.instance.GetPacket();
            packet.Write((byte)MessageType.DeletedItemResponse);
            packet.Write((byte)items.Count);
            foreach(Item item in items)
            {
                ItemIO.Send(item, packet, true, true);
            }
            packet.Send(playerNum);
        }
        private static List<Item> Deserialize(string name)
        {
            List<Item> yeah = new List<Item>();
            if(File.Exists(DiscardItemDataPath))
            {
                using StreamReader r = new(DiscardItemDataPath);
                string json = r.ReadToEnd();
                r.Close();
                if(!string.IsNullOrEmpty(json))
                {
                    List<DeletedItem> allItems = JsonConvert.DeserializeObject<List<DeletedItem>>(json) ?? new List<DeletedItem>();
                    foreach(DeletedItem deletedItem in allItems)
                        if(deletedItem.owner == name) // TODO: Make an unique identifier (e.g. two players with same name will break this)
                            yeah.Add(deletedItem.item.GetClone());
                }
            }
            else
                File.Create(DiscardItemDataPath);
            return yeah;
        }
        public static void AskNicelyForPlayersDeletedItems(int targetPlayerNum)
        {
            DeletedItemReponse.ResetItemArray();
            var packet = ScuffedAnticheatMod.instance.GetPacket();
            packet.Write((byte)MessageType.DeletedItemRequest);
            packet.Write((byte)Main.myPlayer);
            packet.Write((byte)targetPlayerNum);
            packet.Send(255); // To server
        }
    }
}
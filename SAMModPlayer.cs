using Terraria.ModLoader;
using Terraria;
using ScuffedAnticheatMod.UI;
using System.Collections.Generic;

namespace ScuffedAnticheatMod
{
    public class SAMPlayer : ModPlayer
    {
        public IReadOnlyDictionary<string, List<Item>> startingItems { get; private set; }
        public override void ModifyStartingInventory(IReadOnlyDictionary<string, List<Item>> itemsByMod, bool mediumCoreDeath)
        {
            startingItems = itemsByMod;
        }
        // When player enters world, disable possible previously enabled UI
        public override void OnEnterWorld(Player player)
        {
            ModContent.GetInstance<UIOverlay>().DisablePlayerList();
        }

        // Called after item data is synced with server on player join
        // TODO: I don't know if I removed this function or plan to add it
        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            if (Main.dedServ)
            {
                ;
            }
        }
    }
}
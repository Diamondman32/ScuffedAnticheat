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
    }
}
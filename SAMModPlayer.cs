using Terraria.ModLoader;
using Terraria;
using ScuffedAnticheatMod.UI;

namespace ScuffedAnticheatMod
{
    public class SAMPlayer : ModPlayer
    {
        // When player enters world, disable possible previously enabled UI
        public override void OnEnterWorld(Player player)
        {
            ModContent.GetInstance<UIOverlay>().DisablePlayerList();
        }
    }
}
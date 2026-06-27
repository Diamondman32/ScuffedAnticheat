using Terraria.ModLoader;
using Terraria;
using ScuffedAnticheatMod.UI;
using ScuffedAnticheatMod.Network;

namespace ScuffedAnticheatMod
{
    public class SAMPlayer : ModPlayer
    {
        // When player enters world, disable possible previously enabled UI, send to saved spawn location, and possibly insert a mouse item
        public override void OnEnterWorld(Player player)
        {
            ModContent.GetInstance<UIOverlay>().DisablePlayerList();
        }
    }
}
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;

namespace ScuffedAnticheatMod.UI.UIHelpers
{
    public class UIPanelNoClickthrough : UIPanel
    {
        public override void Update(GameTime gameTime) {
			base.Update(gameTime);

			// This causes clicks on this UIElement to not cause the player to use current items
			if (ContainsPoint(Main.MouseScreen)) {
				Main.LocalPlayer.mouseInterface = true;
			}
		}
    }
}
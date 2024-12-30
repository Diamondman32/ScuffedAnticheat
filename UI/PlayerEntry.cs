using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace VIRUS.UI
{
    public class PlayerEntry : UIPanel
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
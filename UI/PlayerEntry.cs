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

namespace SAM.UI
{
    public class PlayerEntry : UIPanel
    {
		private Player player;
		public PlayerEntry(Player player)
		{
			this.player = player;
		}

        public override void OnInitialize()
        {
            base.OnInitialize();
        }
        public override void Update(GameTime gameTime) {
			base.Update(gameTime);

			// This causes clicks on this UIElement to not cause the player to use current items
			if (ContainsPoint(Main.MouseScreen)) {
				Main.LocalPlayer.mouseInterface = true;
			}
		}
        public override void Draw(SpriteBatch spriteBatch)
        {
			Main.MapPlayerRenderer.DrawPlayerHead(Main.Camera, player, new Vector2(0,0), 1f, 0.8f, Color.White);
			// Draw(spriteBatch);
        }
    }
}
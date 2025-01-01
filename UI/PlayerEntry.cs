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

namespace ScuffedAnticheatMod.UI
{
    public class PlayerEntry : UIPanel
    {
		public UIText text;
		private Player player;
		public Player GetPlayer() { return player; }
		public PlayerEntry(Player player)
		{
			this.player = player;
		}

        public override void OnInitialize()
        {
            text = new UIText(player.name, 1f);
			text.Left.Set(0f, 0.2f);
			text.Top.Set(0f, 0f);
			Append(text);
        }
        // public override void Draw(SpriteBatch spriteBatch)
        // {
		// 	// Main.MapPlayerRenderer.DrawPlayerHead(Main.Camera, player, new Vector2(0,0), 1f, 0.8f, Color.White);
		// 	// Draw(spriteBatch);
        // }
    }
}
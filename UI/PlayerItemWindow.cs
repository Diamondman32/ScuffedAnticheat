using Microsoft.Xna.Framework;
using Terraria;
using Terraria.UI;
using ScuffedAnticheatMod.UI.UIHelpers;

namespace ScuffedAnticheatMod.UI
{
    public class PlayerItemWindow : UIPanelNoClickthrough
    {
		public Player player { get; private set; }
		public ItemPanel itemListPanel;
		public const int MAX_ITEMS_IN_ROW = 8;

		public PlayerItemWindow(Player player)
		{
			this.player = player;
			Init();
		}

		// Is called on panel creation
		private void Init()
		{
			// Panel positioning
			BackgroundColor = new Color(0, 100, 0) * 0.5f;
            Left = StyleDimension.FromPixelsAndPercent(0f, 0.50f);
            Top = StyleDimension.FromPixelsAndPercent(0f, 0.30f);
            Width = StyleDimension.FromPixelsAndPercent(0f, 0.20f);
            Height = StyleDimension.FromPixelsAndPercent(0f, 0.40f);
			SetPadding(0);

			// Item List Panel
			itemListPanel = new ItemPanel(player);
			Append(itemListPanel);
		}
    }
}
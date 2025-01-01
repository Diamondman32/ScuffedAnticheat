using IL.Terraria.Localization;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Chat;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace ScuffedAnticheatMod.UI
{
    class PlayerWindow : UIState
    {
        public UIPanelNoClickthrough playerWindow;
        public UIScrollbar scrollBar;
        public UIList playerList;
        public List<PlayerEntry> playerEntries;
        public DeletedItemWindow deletedItemWindow;
        private static float spacing = 20f;
        private Player target;

        public override void OnInitialize()
        {
            deletedItemWindow = null;

            // Entire player window
            playerWindow = new UIPanelNoClickthrough();
            playerWindow.SetPadding(0);
            playerWindow.BackgroundColor = new Color(73, 94, 171);
             playerWindow.Left.Set(0f, 0.30f);
			 playerWindow.Top.Set(0f, 0.30f);
			 playerWindow.Width.Set(0f, 0.20f);
			 playerWindow.Height.Set(0f, 0.40f);

            // Scroll Bar
            scrollBar = new UIScrollbar();
             scrollBar.Left.Set(-30f, 1f);
             scrollBar.Top.Set(0f, 0f);
             scrollBar.Height.Set(0f, 1f);
            playerWindow.Append(scrollBar);

            // Player List
            playerList = new UIList();
             playerList.Left.Set(20f, 0f);
			 playerList.Top.Set(0f, 0f);
			 playerList.Width.Set(0f, 0.90f);
			 playerList.Height.Set(0f, 1f);
            playerList.SetScrollbar(scrollBar);
            playerWindow.Append(playerList);

            // Window with items
            if(deletedItemWindow != null)
            {
                deletedItemWindow.BackgroundColor = new Color(73, 94, 171);
                deletedItemWindow.Left.Set(0f, 0.30f);
                deletedItemWindow.Top.Set(0f, 0.30f);
                deletedItemWindow.Width.Set(0f, 0.20f);
                deletedItemWindow.Height.Set(0f, 0.40f);
                Append(deletedItemWindow);
            }

            // Add to UIState (entire screen)
            Append(playerWindow);
        }
        public override void OnActivate()
        {
            UpdatePlayerList();
        }
        private void UpdatePlayerList()
        {
            playerList.RemoveAllChildren();
            playerEntries = new(); // wipe

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (!player.active) continue;

                playerEntries.Add(new PlayerEntry(player));
                int index = playerEntries.Count - 1;
                target = player;

                playerEntries[index].BackgroundColor = new Color(255, 255, 255);
                playerEntries[index].Top.Set(spacing*index, 0f);
                playerEntries[index].Width.Set(0f, 70f);
                playerEntries[index].Height.Set(30f, 0f);
                playerEntries[index].OnClick += playerEntries_onClick;

                playerList.Add(playerEntries[index]);
            }
        }
        private void playerEntries_onClick(UIMouseEvent mouseEvent, UIElement element)
        {
            Main.NewText("Button Clicked.", Color.DarkKhaki);
            if(deletedItemWindow == null)
            {
                if(target != null)
                {
                    Network.RequestDeletedItems.AskNicelyForPlayersDeletedItems(target.whoAmI);
                    deletedItemWindow = new DeletedItemWindow(target);
                }
                else
                {
                    // Is this even possible
                }
            }
            else
            {
                deletedItemWindow = null;
            }
        }
    }

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
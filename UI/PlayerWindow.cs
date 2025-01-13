using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ScuffedAnticheatMod.UI.UIHelpers;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.UI.Chat;

namespace ScuffedAnticheatMod.UI
{
    class PlayerWindow : UIPanelNoClickthrough
    {
        private UIPanel panel;
        private UIUnderline underline;
        private UIColorScrollbar scrollBar;
        private UIColorList playerList;
        private UIText text;
        private List<PlayerEntry> playerEntries;
        public static DeletedItemWindow deletedItemWindow;
        private Player selected;
        private Player hovering;

        // Initializes element and its children
        public void Init()
        {
            // Player Window
            BackgroundColor = new Color(0, 100, 0) * 0.5f;
            Left.Set(0f, 0.30f);
			Top.Set(0f, 0.30f);
			Width.Set(0f, 0.20f);
			Height.Set(0f, 0.40f);
            SetPadding(0);

            // Text
            const string textString = "Player List";
            const float textScale = 1.25f;
            float textWidth = ChatManager.GetStringSize(FontAssets.MouseText.Value, textString, new Vector2(textScale)).X;
            text = new UIText(textString, textScale)
            {
                Left = StyleDimension.FromPixelsAndPercent(-textWidth/2, 0.5f),
                Top = StyleDimension.FromPixelsAndPercent(0f, 0.025f)
            };
            text.SetPadding(0);
            Append(text);

            // Underline
            underline = new UIUnderline()
            {
                Color = new Color(255, 255, 255) * 0.7f,
                Left = StyleDimension.FromPixelsAndPercent(0f, 0.05f),
                Top = StyleDimension.FromPixelsAndPercent(0f, 0.085f),
                Width = StyleDimension.FromPixelsAndPercent(0f, 0.9f),
                Height = StyleDimension.FromPixelsAndPercent(5f, 0f)
            };
            underline.SetPadding(0);
            Append(underline);

            // Panel for List and Scrollbar shading
            panel = new UIPanel()
            {
                BackgroundColor = new Color(255, 255, 255) * 0.04f,
                BorderColor = new Color(0, 0, 0) * 0.4f,
                Left = StyleDimension.FromPixelsAndPercent(0f, 0.025f),
                Top = StyleDimension.FromPixelsAndPercent(0f, 0.1f),
                Width = StyleDimension.FromPixelsAndPercent(0f, 0.95f),
                Height = StyleDimension.FromPixelsAndPercent(0f, 0.875f)
            };
            panel.SetPadding(0);
            Append(panel);

            // Scroll Bar
            scrollBar = new UIColorScrollbar()
            {
                borderAndBackgroundColor = new Color(255, 255, 255) * 0.05f,
                PaddingLeft = 0,
                PaddingRight = 0,
                Top = StyleDimension.FromPixelsAndPercent(0f, 0.05f),
                Left = StyleDimension.FromPixelsAndPercent(-25f, 1f),
                Height = StyleDimension.FromPixelsAndPercent(0f, 0.9f)
            };
            panel.Append(scrollBar);

            // Player List
            playerEntries = new List<PlayerEntry>();
            playerList = new UIColorList()
            {
                Left = StyleDimension.FromPixelsAndPercent(0f, 0.015f),
                Top = StyleDimension.FromPixelsAndPercent(0f, 0.015f),
                Width = StyleDimension.FromPixelsAndPercent(-25f, 0.975f),
                Height = StyleDimension.FromPixelsAndPercent(0f, 0.985f)
            };
            playerList.SetPadding(0);
            playerList.SetScrollbar(scrollBar);
            panel.Append(playerList);
        }

        // Refreshes player list
        public void UpdatePlayerList()
        {
            // If selected of deletedItemWindow left the game then close window
            if(Parent.HasChild(deletedItemWindow) && (!selected?.active ?? false))
                Parent.RemoveChild(deletedItemWindow);

            // Reset List
            playerEntries.Clear();
            playerList.Clear();

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (!player.active) continue;

                playerEntries.Add(new PlayerEntry(player)
                {
                    BackgroundColor = new Color(0, 0, 0, 125),
                    BorderColor = (player == selected) ? new Color(255, 255, 255) * 0.7f : (player == hovering) ? new Color(255, 255, 255) * 0.35f : new Color(0, 0, 0) * 0.7f,
                    Top = StyleDimension.FromPixelsAndPercent(20f*playerEntries.Count, 0f),
                    Width = StyleDimension.FromPixelsAndPercent(0f, 1f),
                    Height = StyleDimension.FromPixelsAndPercent(30f, 0f)
                });
                playerEntries[^1].Init();
                playerEntries[^1].SetPadding(0);
                playerEntries[^1].OnClick += playerEntries_onClick;
                playerEntries[^1].OnMouseOver += playerEntries_onHover;
                playerEntries[^1].OnMouseOut += playerEntries_onStopHover;
                playerList.Add(playerEntries[^1]);
            }
        }

        private void playerEntries_onClick(UIMouseEvent evt, UIElement element)
        {
            PlayerEntry playerEntry = (PlayerEntry)element;

            Player prevselected = selected;
            selected = playerEntry.player;

            // Player not listed
            if(selected.active == false)
                return;

            if(!Parent.HasChild(deletedItemWindow))
            {
                Network.RequestDeletedItems.AskNicelyForPlayersDeletedItems(selected.whoAmI);
                deletedItemWindow = new DeletedItemWindow(selected);
                deletedItemWindow.Init();
                Parent.Append(deletedItemWindow);
            }
            else if(prevselected != selected)
            {
                Parent.RemoveChild(deletedItemWindow);
                Network.RequestDeletedItems.AskNicelyForPlayersDeletedItems(selected.whoAmI);
                deletedItemWindow = new DeletedItemWindow(selected);
                deletedItemWindow.Init();
                Parent.Append(deletedItemWindow);
            }
            else
            {
                Parent.RemoveChild(deletedItemWindow);
                selected = null;
            }

            // Update "selected" entry UI
            UpdatePlayerList();
        }

        private void playerEntries_onHover(UIMouseEvent evt, UIElement element)
        {
            PlayerEntry playerEntry = (PlayerEntry)element;
            if(playerEntry.player != selected)
            {
                hovering = playerEntry.player;
                playerEntry.BorderColor = new Color(255, 255, 255) * 0.35f;
            }
        }

        private void playerEntries_onStopHover(UIMouseEvent evt, UIElement element)
        {
            PlayerEntry playerEntry = (PlayerEntry)element;
            if(playerEntry.player != selected)
            {
                hovering = null;
                playerEntry.BorderColor = new Color(0, 0, 0) * 0.7f;
            }
        }

        public void Reset()
        {
            if(Parent.HasChild(deletedItemWindow))
                Parent.RemoveChild(deletedItemWindow);
            selected = null;
        }
    }
    public class PlayerEntry : UIPanel
    {
		private UIText text;
		public Player player { get; private set; }
		public PlayerEntry(Player player)
		{
			this.player = player;
		}

        public void Init()
        {
            text = new UIText(player.name, 1f);
			text.Left.Set(0f, 0.1f);
			text.Top.Set(0f, 0.25f);
			Append(text);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
			// Main.MapPlayerRenderer.DrawPlayerHead(Main.Camera, player, new Vector2(0,0));
        }
    }
}
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
    class PlayerWindow : UIState
    {
        public UIPanel entireWindow;
        public UIScrollbar scrollBar;
        public UIList playerList;

        public override void OnInitialize()
        {
            // Entire window
            entireWindow = new UIPanel();
            entireWindow.SetPadding(0);
            SetRectangle(entireWindow, 15f, 20f, 300f, 100f);
            entireWindow.BackgroundColor = new Color(73, 94, 171);

            // Scroll Bar
            scrollBar = new UIScrollbar();
            scrollBar.Left.Set(280f, 0f);
            scrollBar.Top.Set(2f, 0f);
            scrollBar.Height.Set(80f, 0f);

            // Player List
            playerList = new UIList();
            playerList.SetScrollbar(scrollBar);
            UpdateList();
        }
        private void SetRectangle(UIElement uiElement, float left, float top, float width, float height) {
			uiElement.Left.Set(left, 0f);
			uiElement.Top.Set(top, 0f);
			uiElement.Width.Set(width, 0f);
			uiElement.Height.Set(height, 0f);
		}
        private void UpdateList()
        {
            
        }
    }
}
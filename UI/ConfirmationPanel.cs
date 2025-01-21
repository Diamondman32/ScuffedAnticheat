using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ScuffedAnticheatMod.Network;
using ScuffedAnticheatMod.UI.UIHelpers;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace ScuffedAnticheatMod.UI
{
    public class ConfirmationPanel : UIPanel
    {
        private readonly int targetNum;
        private readonly List<Item> items;
        private readonly Action HideConfirmation;
        public ConfirmationPanel(int targetNum, List<Item> items, Action HideConfirmation)
        {
            this.targetNum = targetNum;
            this.items = items;
            this.HideConfirmation = HideConfirmation;
            Init();
        }

        private void Init()
        {
            BackgroundColor = new Color(255, 255, 255) * 0.04f;
            BorderColor = new Color(0, 0, 0) * 0.4f;
            Left = StyleDimension.FromPixelsAndPercent(0f, 0.025f);
            Top = StyleDimension.FromPixelsAndPercent(0f, 0.1f);
            Width = StyleDimension.FromPixelsAndPercent(0f, 0.95f);
            Height = StyleDimension.FromPixelsAndPercent(0f, 0.875f);
			SetPadding(0);

            // Text
            // UICenteredText text = new UICenteredText($"This will return a(n) {items.Name} to {Main.player[targetNum].name}", 1f, Color.DarkRed)
            // {
            //     Top = StyleDimension.FromPixelsAndPercent(0f, 0.1f)
            // };
            // text.SetPadding(0);
            // Append(text);

			// Yes Button
			UIPanel yesButton = new UIPanel()
			{
				BackgroundColor = new Color(0, 0, 0) * 0.5f,
				BorderColor = new Color(0, 0, 0) * 0.7f,
				Left = StyleDimension.FromPixelsAndPercent(0f, 0.7f),
                Top = StyleDimension.FromPixelsAndPercent(0f, 0.4f),
                Width = StyleDimension.FromPixelsAndPercent(0f, 0.2f),
                Height = StyleDimension.FromPixelsAndPercent(0f, 0.15f)
			};
			yesButton.OnClick += yesButton_onClick;
			yesButton.OnMouseOver += UIPanel_onHover;
			yesButton.OnMouseOut += UIPanel_onStopHover;
			Append(yesButton);

			// Text
            UICenteredText yesText = new UICenteredText("Yes", 1f);
            yesButton.Append(yesText);

			// No Button
			UIPanel noButton = new UIPanel()
			{
				BackgroundColor = new Color(0, 0, 0) * 0.5f,
				BorderColor = new Color(0, 0, 0) * 0.7f,
				Left = StyleDimension.FromPixelsAndPercent(0f, 0.1f),
                Top = StyleDimension.FromPixelsAndPercent(0f, 0.4f),
                Width = StyleDimension.FromPixelsAndPercent(0f, 0.2f),
                Height = StyleDimension.FromPixelsAndPercent(0f, 0.15f)
			};
			noButton.OnClick += noButton_onClick;
			noButton.OnMouseOver += UIPanel_onHover;
			noButton.OnMouseOut += UIPanel_onStopHover;
			Append(noButton);

			// Text
            UICenteredText noText = new UICenteredText("No", 1f);
            noButton.Append(noText);
        }

        private int FindRemainingInvSlots()
        {
            int openSlots = 0;
            foreach(Item item in Main.player[targetNum].inventory)
                if(item.type == ItemID.None)
                    openSlots++;
            return openSlots;
        }

        private void yesButton_onClick(UIMouseEvent evt, UIElement element)
        {
            int openSlots = FindRemainingInvSlots();
            int i = 0;
            foreach(Item item in items)
            {
                if(i < openSlots)
                {
                    UpdateDeletedItemSaveData.ReturnItemToPlayer(item, targetNum);
                    DeletedItemReponse.RemoveElement(item);
                    i++;
                }
                else
                {
                    Main.NewText($"{Main.player[targetNum].name}'s inventory is full!", Color.Red);
                    break;
                }
            }
        }

        private void noButton_onClick(UIMouseEvent evt, UIElement element)
        {
            HideConfirmation();
        }

        private void UIPanel_onHover(UIMouseEvent evt, UIElement element)
        {
            UIPanel itemBackground = (UIPanel)element;
            itemBackground.BorderColor = new Color(255, 255, 255) * 0.35f;
        }

        private void UIPanel_onStopHover(UIMouseEvent evt, UIElement element)
        {
            UIPanel itemBackground = (UIPanel)element;
            itemBackground.BorderColor = new Color(0, 0, 0) * 0.7f;
        }
    }
}
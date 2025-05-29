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
        private readonly bool returnItems;
        private readonly List<Item> items;
        private readonly Action<bool> HideConfirmation;
        private UIColorList itemList;
        private const int MAX_ITEMS_IN_ROW = PlayerItemWindow.MAX_ITEMS_IN_ROW;

        public ConfirmationPanel(int targetNum, bool returnItems, List<Item> items, Action<bool> HideConfirmation)
        {
            this.targetNum = targetNum;
            this.returnItems = returnItems;
            this.items = items;
            this.HideConfirmation = HideConfirmation;
            Init();
        }

        private void Init()
        {
            BackgroundColor = Color.Transparent;
            BorderColor = Color.Transparent;
            Width = StyleDimension.FromPixelsAndPercent(0f, 1f);
            Height = StyleDimension.FromPixelsAndPercent(0f, 1f);
            SetPadding(0);

            // Text
            UICenteredText text = new UICenteredText(
                $"This will {(returnItems ? "return" : "delete")} the following items {(returnItems ? $"to {Main.player[targetNum].name}!" : "!")}", 1f, Color.DarkRed)
            {
                Top = StyleDimension.FromPixelsAndPercent(0f, 0.1f)
            };
            text.SetPadding(0);
            Append(text);

            // Yes Button
            UIPanel yesButton = new UIPanel()
            {
                BackgroundColor = new Color(0, 0, 0) * 0.5f,
                BorderColor = new Color(0, 0, 0) * 0.7f,
                Left = StyleDimension.FromPixelsAndPercent(0f, 0.55f),
                Top = StyleDimension.FromPixelsAndPercent(0f, 0.2f),
                Width = StyleDimension.FromPixelsAndPercent(0f, 0.3f),
                Height = StyleDimension.FromPixelsAndPercent(0f, 0.075f)
            };
            yesButton.OnClick += yesButton_onClick;
            yesButton.OnMouseOver += UIPanel_onHover;
            yesButton.OnMouseOut += UIPanel_onStopHover;
            Append(yesButton);

            // Text
            UICenteredText yesText = new UICenteredText(returnItems ? "Return" : "Delete", 1f);
            yesButton.Append(yesText);

            // No Button
            UIPanel noButton = new UIPanel()
            {
                BackgroundColor = new Color(0, 0, 0) * 0.5f,
                BorderColor = new Color(0, 0, 0) * 0.7f,
                Left = StyleDimension.FromPixelsAndPercent(0f, 0.15f),
                Top = StyleDimension.FromPixelsAndPercent(0f, 0.2f),
                Width = StyleDimension.FromPixelsAndPercent(0f, 0.3f),
                Height = StyleDimension.FromPixelsAndPercent(0f, 0.075f)
            };
            noButton.OnClick += noButton_onClick;
            noButton.OnMouseOver += UIPanel_onHover;
            noButton.OnMouseOut += UIPanel_onStopHover;
            Append(noButton);

            // Text
            UICenteredText noText = new UICenteredText("Go Back", 1f);
            noButton.Append(noText);
            
            // Panel for List and Scrollbar shading
            UIPanel panel = new UIPanel()
            {
                BackgroundColor = new Color(255, 255, 255) * 0.04f,
                BorderColor = new Color(0, 0, 0) * 0.4f,
                Left = StyleDimension.FromPixelsAndPercent(0f, 0.025f),
                Top = StyleDimension.FromPixelsAndPercent(0f, 0.3f),
                Width = StyleDimension.FromPixelsAndPercent(0f, 0.95f),
                Height = StyleDimension.FromPixelsAndPercent(0f, 0.675f)
            };
            panel.SetPadding(0);
            Append(panel);

            // Scroll Bar
            UIColorScrollbar scrollBar = new UIColorScrollbar
            {
                borderAndBackgroundColor = new Color(0, 100, 0) * 0.3f,
                PaddingLeft = 0,
                PaddingRight = 0,
                Top = StyleDimension.FromPixelsAndPercent(0f, 0.05f),
                Left = StyleDimension.FromPixelsAndPercent(-25f, 1f),
                Height = StyleDimension.FromPixelsAndPercent(0f, 0.9f)
            };
            panel.Append(scrollBar);

            // Selected Items List
            itemList = new UIColorList()
			{
				Left = StyleDimension.FromPixelsAndPercent(0f, 0.015f),
                Top = StyleDimension.FromPixelsAndPercent(0f, 0.015f),
				Width = StyleDimension.FromPixelsAndPercent(-25f, 0.975f),
                Height = StyleDimension.FromPixelsAndPercent(0f, 0.985f)
			};
			itemList.SetPadding(0);
            itemList.SetScrollbar(scrollBar);
			AddSelectedItems();
			panel.Append(itemList);
        }
        
        // Async code that adds items when received
		private void AddSelectedItems()
		{
            List<UIItemSlot> uiItemSlots = new List<UIItemSlot>();
			List<UIPanel> itemSlotBackgrounds = new List<UIPanel>();
			List<UIPanel> itemSlotRows = new List<UIPanel>();
			itemList.Clear();
            int i = 0;

            foreach (Item item in items)
            {
                // Item Rows
                if (itemSlotBackgrounds.Count / MAX_ITEMS_IN_ROW >= itemSlotRows.Count)
                {
                    itemSlotRows.Add(new UIPanel()
                    {
                        BackgroundColor = Color.Transparent,
                        BorderColor = Color.Transparent,
                        Width = StyleDimension.FromPixelsAndPercent(0f, 1f),
                        Height = StyleDimension.FromPixelsAndPercent(40f, 0f)
                    });
                    itemSlotRows[^1].SetPadding(0);
                    itemList.Add(itemSlotRows[^1]);
                }

                // Item Backgrounds
                itemSlotBackgrounds.Add(new UIPanel()
                {
                    BackgroundColor = new Color(0, 0, 0) * 0.5f,
                    BorderColor = new Color(255, 255, 255) * 0.35f,
                    Left = StyleDimension.FromPixelsAndPercent(0f, 1f / MAX_ITEMS_IN_ROW * (i % MAX_ITEMS_IN_ROW)),
                    Width = StyleDimension.FromPixelsAndPercent(40f, 0f),
                    Height = StyleDimension.FromPixelsAndPercent(40f, 0f)
                });
                itemSlotBackgrounds[i].SetPadding(0);
                itemSlotRows[^1].Append(itemSlotBackgrounds[i]);

                // Items
                uiItemSlots.Add(new UIItemSlot(items.ToArray(), i, 14)
                {
                    Width = StyleDimension.FromPixelsAndPercent(0f, 1f),
                    Height = StyleDimension.FromPixelsAndPercent(0f, 1f)
                });
                uiItemSlots[i].SetPadding(0);
                itemSlotBackgrounds[i].Append(uiItemSlots[i]);

                i++;
            }
		}

        private int FindRemainingInvSlots()
        {
            int openSlots = 0;
            foreach (Item item in Main.player[targetNum].inventory)
                if (item.type == ItemID.None)
                    openSlots++;
            return openSlots;
        }

        private void yesButton_onClick(UIMouseEvent evt, UIElement element)
        {
            if (returnItems)
            {
                int openSlots = FindRemainingInvSlots();
                int i = 0;
                foreach (Item item in items)
                {
                    if (i <= openSlots)
                    {
                        UpdateDeletedItemSaveData.ReturnItemToPlayer(item, targetNum);
                        DeletedItemReponse.RemoveElement(item);
                        i++;
                    }
                    else
                    {
                        Main.NewText($"{Main.player[targetNum].name}'s does not have enough inventory space! Returned {i}/{items.Count} items", Color.Red);
                        break;
                    }
                }
            }
            else
            {
                foreach (Item item in items)
                {
                    DeletedItemReponse.RemoveElement(item);
                }
            }
            HideConfirmation(true);
        }

        private void noButton_onClick(UIMouseEvent evt, UIElement element)
        {
            HideConfirmation(false);
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
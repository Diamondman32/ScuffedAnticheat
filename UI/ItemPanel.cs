using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.UI.Elements;
using ScuffedAnticheatMod.Network;
using Terraria.UI;
using ScuffedAnticheatMod.UI.UIHelpers;

namespace ScuffedAnticheatMod.UI
{
    public class ItemPanel : UIPanelNoClickthrough
    {
		private UIElement parent;
		private readonly Player player;
		private static UIColorList itemList;
		private static List<UIPanel> itemSlotBackgrounds;
		private static UIPanel confirmationPanel;
		private static List<bool> selectedItemMap;
		private const int MAX_ITEMS_IN_ROW = PlayerItemWindow.MAX_ITEMS_IN_ROW;

		public ItemPanel(Player player)
		{
			this.player = player;
			Init();
		}

		// Is called on panel creation
		private void Init()
		{
			parent = null;
			BackgroundColor = Color.Transparent;
            BorderColor = Color.Transparent;
            Width = StyleDimension.FromPixelsAndPercent(0f, 1f);
            Height = StyleDimension.FromPixelsAndPercent(0f, 1f);
			SetPadding(0);

			// Text
			UICenteredText text1 = new UICenteredText("Confiscated Items", 1.25f)
            {
                Top = StyleDimension.FromPixelsAndPercent(0f, 0.025f)
            };
            text1.SetPadding(0);
            Append(text1);

			// Underline
            UIUnderline underline = new UIUnderline()
            {
                Color = new Color(255, 255, 255) * 0.7f,
                Left = StyleDimension.FromPixelsAndPercent(0f, 0.05f),
                Top = StyleDimension.FromPixelsAndPercent(0f, 0.085f),
                Width = StyleDimension.FromPixelsAndPercent(0f, 0.9f),
                Height = StyleDimension.FromPixelsAndPercent(5f, 0f)
            };
            underline.SetPadding(0);
            Append(underline);
			
			// Button for selected all items
			UIPanel selectAllButton = new UIPanel()
			{
				BackgroundColor = new Color(0, 0, 0) * 0.5f,
				BorderColor = new Color(0, 0, 0) * 0.7f,
				Left = StyleDimension.FromPixelsAndPercent(0f, 0.075f),
                Top = StyleDimension.FromPixelsAndPercent(0f, 0.11f),
				Width = StyleDimension.FromPixelsAndPercent(0f, 0.275f),
                Height = StyleDimension.FromPixelsAndPercent(30f, 0f)
			};
			selectAllButton.SetPadding(0);
			selectAllButton.OnClick += selectAll_onClick;
			selectAllButton.OnMouseOver += UIPanel_onHover;
			selectAllButton.OnMouseOut += UIPanel_onStopHover;
			Append(selectAllButton);

			// ^ Button Text
			UICenteredText text2 = new UICenteredText("Select All", 0.75f)
			{
				Top = StyleDimension.FromPixelsAndPercent(10f, 0f),
			};
            text2.SetPadding(0);
            selectAllButton.Append(text2);

			// Button for returning selected items
			UIPanel returnItemsButton = new UIPanel()
			{
				BackgroundColor = new Color(0, 0, 0) * 0.5f,
				BorderColor = new Color(0, 0, 0) * 0.7f,
				Left = StyleDimension.FromPixelsAndPercent(0f, 0.3625f),
                Top = StyleDimension.FromPixelsAndPercent(0f, 0.11f),
				Width = StyleDimension.FromPixelsAndPercent(0f, 0.275f),
                Height = StyleDimension.FromPixelsAndPercent(30f, 0f)
			};
			returnItemsButton.SetPadding(0);
			returnItemsButton.OnClick += returnItems_onClick;
			returnItemsButton.OnMouseOver += UIPanel_onHover;
			returnItemsButton.OnMouseOut += UIPanel_onStopHover;
			Append(returnItemsButton);

			// ^ Button Text
			UICenteredText text3 = new UICenteredText("Return Selected", 0.75f)
			{
				Top = StyleDimension.FromPixelsAndPercent(10f, 0f),
			};
            text3.SetPadding(0);
            returnItemsButton.Append(text3);

			// Button for deleting selected items
			UIPanel removeItemsButton = new UIPanel()
			{
				BackgroundColor = new Color(0, 0, 0) * 0.5f,
				BorderColor = new Color(0, 0, 0) * 0.7f,
				Left = StyleDimension.FromPixelsAndPercent(0f, 0.65f),
                Top = StyleDimension.FromPixelsAndPercent(0f, 0.11f),
				Width = StyleDimension.FromPixelsAndPercent(0f, 0.275f),
                Height = StyleDimension.FromPixelsAndPercent(30f, 0f)
			};
			removeItemsButton.SetPadding(0);
			removeItemsButton.OnClick += removeItems_onClick;
			removeItemsButton.OnMouseOver += UIPanel_onHover;
			removeItemsButton.OnMouseOut += UIPanel_onStopHover;
			Append(removeItemsButton);

			// ^ Button Text
			UICenteredText text4 = new UICenteredText("Remove Selected", 0.75f)
			{
				Top = StyleDimension.FromPixelsAndPercent(10f, 0f),
			};
            text4.SetPadding(0);
            removeItemsButton.Append(text4);

			// Panel for List and Scrollbar shading
			UIPanel panel = new UIPanel()
            {
                BackgroundColor = new Color(255, 255, 255) * 0.04f,
                BorderColor = new Color(0, 0, 0) * 0.4f,
                Left = StyleDimension.FromPixelsAndPercent(0f, 0.025f),
                Top = StyleDimension.FromPixelsAndPercent(40f, 0.1f),
                Width = StyleDimension.FromPixelsAndPercent(0f, 0.95f),
                Height = StyleDimension.FromPixelsAndPercent(-40f, 0.875f)
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

            // Item List
			selectedItemMap = new List<bool>();
            itemList = new UIColorList()
			{
				Left = StyleDimension.FromPixelsAndPercent(0f, 0.015f),
                Top = StyleDimension.FromPixelsAndPercent(0f, 0.015f),
				Width = StyleDimension.FromPixelsAndPercent(-25f, 0.975f),
                Height = StyleDimension.FromPixelsAndPercent(0f, 0.985f)
			};
			itemList.SetPadding(0);
            itemList.SetScrollbar(scrollBar);
			itemSlotBackgrounds = new List<UIPanel>();
			AddItems();
			panel.Append(itemList);
		}

		// Async code that adds items when received
		private async void AddItems()
		{
			RequestDeletedItems.AskNicelyForPlayersDeletedItems(player.whoAmI);

			List<UIItemDisplay> uiItemSlots = new List<UIItemDisplay>();
			List<UIPanel> itemSlotRows = new List<UIPanel>();
			itemSlotBackgrounds.Clear();
			itemList.Clear();

			await Task.Run(() =>
			{
				bool timeHasRunOut = false;
				var EndTime = (bool b) => timeHasRunOut = true;
				Timer timer = new((Object stateInfo) => { }, EndTime, 10000, Timeout.Infinite);

				int i = 0;
				List<Item> items = new List<Item>();

				// check for items received status and iteration progress. Gives up after 10 secs
				while ((!ReceiveDeletedItems.itemsReceived || ReceiveDeletedItems.playerDeletedItems.Count != i) && !timeHasRunOut)
				{
					if (i < ReceiveDeletedItems.playerDeletedItems.Count)
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

						// Set up selected map thingy
						if (i >= selectedItemMap.Count)
							selectedItemMap.Add(false);

						// Item Backgrounds
						itemSlotBackgrounds.Add(new UIPanel()
						{
							BackgroundColor = new Color(0, 0, 0) * 0.5f,
							BorderColor = selectedItemMap[i] ? new Color(255, 255, 255) * 0.7f : new Color(0, 0, 0) * 0.7f,
							Left = StyleDimension.FromPixelsAndPercent(0f, 1f / MAX_ITEMS_IN_ROW * (i % MAX_ITEMS_IN_ROW)),
							Width = StyleDimension.FromPixelsAndPercent(40f, 0f),
							Height = StyleDimension.FromPixelsAndPercent(40f, 0f)
						});
						itemSlotBackgrounds[i].SetPadding(0);
						itemSlotBackgrounds[^1].OnClick += itemSlotBackgrounds_onClick;
						itemSlotBackgrounds[i].OnMouseOver += itemSlotBackgrounds_onHover;
						itemSlotBackgrounds[i].OnMouseOut += itemSlotBackgrounds_onStopHover;
						itemSlotRows[^1].Append(itemSlotBackgrounds[i]);

						// Items
						items.Add(ReceiveDeletedItems.playerDeletedItems[i]);
						uiItemSlots.Add(new UIItemDisplay(items[i], 14)
						{
							Width = StyleDimension.FromPixelsAndPercent(0f, 1f),
							Height = StyleDimension.FromPixelsAndPercent(0f, 1f)
						});
						uiItemSlots[i].SetPadding(0);
						itemSlotBackgrounds[i].Append(uiItemSlots[i]);

						i++;
					}
				}

				timer.Dispose();
				if (timeHasRunOut)
					Main.NewText("Item searching timed out", Color.Red);
			});
		}

		public void RefreshItemList()
		{
			AddItems();
		}

		private void UIPanel_onHover(UIMouseEvent evt, UIElement element)
		{
			UIPanel panel = (UIPanel)element;
			panel.BorderColor = new Color(255, 255, 255) * 0.35f;
		}

		private void UIPanel_onStopHover(UIMouseEvent evt, UIElement element)
		{
			UIPanel panel = (UIPanel)element;
			panel.BorderColor = new Color(0, 0, 0) * 0.7f;
		}

		private void selectAll_onClick(UIMouseEvent evt, UIElement element)
		{
			UIPanel panel = (UIPanel)element;
			if (ReceiveDeletedItems.itemsReceived)
			{
				for (int i = 0; i < selectedItemMap.Count; i++)
					selectedItemMap[i] = true;
				itemSlotBackgrounds.ForEach(x => x.BorderColor = new Color(255, 255, 255) * 0.7f);
			}
		}

		private void returnItems_onClick(UIMouseEvent evt, UIElement element)
		{
			UIPanel panel = (UIPanel)element;
			panel.BorderColor = new Color(0, 0, 0) * 0.7f;
			ShowConfirmation(true);
		}
		
		private void removeItems_onClick(UIMouseEvent evt, UIElement element)
		{
			UIPanel panel = (UIPanel)element;
			panel.BorderColor = new Color(0, 0, 0) * 0.7f;
			ShowConfirmation(false);
		}

		private void itemSlotBackgrounds_onClick(UIMouseEvent evt, UIElement element)
		{
			UIPanel panel = (UIPanel)element;
			int itemIndex = itemSlotBackgrounds.FindIndex(x => x == panel);
			if (itemIndex == -1) return;

			if (!selectedItemMap[itemIndex])
			{
				panel.BorderColor = new Color(255, 255, 255) * 0.7f;
				selectedItemMap[itemIndex] = true;
			}
			else
			{
				panel.BorderColor = new Color(0, 0, 0) * 0.7f;
				selectedItemMap[itemIndex] = false;
			}
		}

		private void itemSlotBackgrounds_onHover(UIMouseEvent evt, UIElement element)
        {
            UIPanel panel = (UIPanel)element;
			int itemIndex = itemSlotBackgrounds.FindIndex(x => x == panel);
			if (itemIndex == -1) return;

			if(!selectedItemMap[itemIndex])
				panel.BorderColor = new Color(255, 255, 255) * 0.35f;
        }

        private void itemSlotBackgrounds_onStopHover(UIMouseEvent evt, UIElement element)
        {
			UIPanel panel = (UIPanel)element;
			int itemIndex = itemSlotBackgrounds.FindIndex(x => x == panel);
			if (itemIndex == -1) return;

			if(!selectedItemMap[itemIndex])
				panel.BorderColor = new Color(0, 0, 0) * 0.7f;
        }

		private void ShowConfirmation(bool returnItems)
		{
			parent ??= Parent;

			List<Item> selectedItems = new();
			for (int i = 0; i < selectedItemMap.Count; i++)
			{
				if (selectedItemMap[i])
					selectedItems.Add(ReceiveDeletedItems.playerDeletedItems[i]);
			}
			confirmationPanel = new ConfirmationPanel(player.whoAmI, returnItems, selectedItems, HideConfirmation);
			parent?.RemoveChild(this);
			parent?.Append(confirmationPanel);
		}

		private void HideConfirmation(bool status)
		{
			if (status)
				selectedItemMap.Clear();

			parent?.RemoveChild(confirmationPanel);
			AddItems();
			parent?.Append(this);
		}
    }
}
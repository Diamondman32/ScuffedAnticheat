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
    public class DeletedItemWindow : UIPanelNoClickthrough
    {
		public Player player { get; private set; }
		private UIColorList itemList;
		private List<UIItemSlot> uiItemSlots;
		private List<UIPanel> itemSlotBackgrounds;
		private List<UIPanel> itemSlotRows;
		private UIPanel itemListPanel;
		private UIPanel confirmationPanel;
		private Item selectedItem;
		private const int maxItemsInRow = 8;


		public DeletedItemWindow(Player player)
		{
			this.player = player;
			Init();
		}

		// Is called on panel creation
		private void Init()
		{
			// Panel positioning
			BackgroundColor = new Color(0, 100, 0) * 0.5f;
            Left.Set(0f, 0.50f);
            Top.Set(0f, 0.30f);
            Width.Set(0f, 0.20f);
            Height.Set(0f, 0.40f);
			SetPadding(0);

			// Item List Panel
			itemListPanel = new UIPanel()
			{
				BackgroundColor = Color.Transparent,
                BorderColor = Color.Transparent,
                Width = StyleDimension.FromPixelsAndPercent(0f, 1f),
                Height = StyleDimension.FromPixelsAndPercent(0f, 1f)
			};
			itemListPanel.SetPadding(0);
			Append(itemListPanel);

			// Text
			UICenteredText text = new UICenteredText("Stolen Items (Click to Return)", 1.25f)
            {
                Top = StyleDimension.FromPixelsAndPercent(0f, 0.025f)
            };
            text.SetPadding(0);
            itemListPanel.Append(text);

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
            itemListPanel.Append(underline);

			// Panel for List and Scrollbar shading
            UIPanel panel = new UIPanel()
            {
                BackgroundColor = new Color(255, 255, 255) * 0.04f,
                BorderColor = new Color(0, 0, 0) * 0.4f,
                Left = StyleDimension.FromPixelsAndPercent(0f, 0.025f),
                Top = StyleDimension.FromPixelsAndPercent(0f, 0.1f),
                Width = StyleDimension.FromPixelsAndPercent(0f, 0.95f),
                Height = StyleDimension.FromPixelsAndPercent(0f, 0.875f)
            };
            panel.SetPadding(0);
            itemListPanel.Append(panel);

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
            uiItemSlots = new List<UIItemSlot>();
			itemSlotBackgrounds = new List<UIPanel>();
			itemSlotRows = new List<UIPanel>();
            itemList = new UIColorList()
			{
				Left = StyleDimension.FromPixelsAndPercent(0f, 0.015f),
                Top = StyleDimension.FromPixelsAndPercent(0f, 0.015f),
				Width = StyleDimension.FromPixelsAndPercent(-25f, 0.975f),
                Height = StyleDimension.FromPixelsAndPercent(0f, 0.985f)
			};
			itemList.SetPadding(0);
            itemList.SetScrollbar(scrollBar);
			AddItems();
			panel.Append(itemList);
		}

		// Async code that adds items when received
		private async void AddItems()
		{
			itemList.Clear();
			uiItemSlots.Clear();
			itemSlotBackgrounds.Clear();
			itemSlotRows.Clear();
			RequestDeletedItems.AskNicelyForPlayersDeletedItems(player.whoAmI);

			await Task.Run(() =>
			{
				bool timeHasRunOut = false;
				var EndTime = (bool b) => timeHasRunOut = true;
				Timer timer = new((Object stateInfo) => {}, EndTime, 10000, Timeout.Infinite);

				int i = 0;
				List<Item> items = new List<Item>();

				// check for items received status and iteration progress. Gives up after 10 secs
				while((!DeletedItemReponse.itemsReceived || DeletedItemReponse.targetDeletedItems.Count != i) && !timeHasRunOut)
				{
					if(i < DeletedItemReponse.targetDeletedItems.Count)
					{
						// Item Rows
						if(itemSlotBackgrounds.Count / maxItemsInRow >= itemSlotRows.Count)
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
							BorderColor = new Color(0, 0, 0) * 0.7f,
							Left = StyleDimension.FromPixelsAndPercent(0f, 1f/maxItemsInRow*(i%maxItemsInRow)),
							Width = StyleDimension.FromPixelsAndPercent(40f, 0f),
							Height = StyleDimension.FromPixelsAndPercent(40f, 0f)
						});
						itemSlotBackgrounds[i].SetPadding(0);
						itemSlotBackgrounds[^1].OnClick += itemSlotBackgrounds_onClick;
						itemSlotBackgrounds[i].OnMouseOver += itemSlotBackgrounds_onHover;
                		itemSlotBackgrounds[i].OnMouseOut += itemSlotBackgrounds_onStopHover;
						itemSlotRows[^1].Append(itemSlotBackgrounds[i]);

						// Items
						items.Add(DeletedItemReponse.targetDeletedItems[i]);
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

				timer.Dispose();
				if(timeHasRunOut)
					Main.NewText("Item searching timed out", Color.Red);
			});
		}

		public void RefreshItemList()
		{
			AddItems();
		}

		private void itemSlotBackgrounds_onClick(UIMouseEvent evt, UIElement element)
		{
			UIPanel panel = (UIPanel)element;
			int itemIndex = itemSlotBackgrounds.FindIndex(x => x == panel);
			selectedItem = DeletedItemReponse.targetDeletedItems[itemIndex];
			ShowConfirmation();
		}

		private void itemSlotBackgrounds_onHover(UIMouseEvent evt, UIElement element)
        {
            UIPanel itemBackground = (UIPanel)element;
            itemBackground.BorderColor = new Color(255, 255, 255) * 0.35f;
        }

        private void itemSlotBackgrounds_onStopHover(UIMouseEvent evt, UIElement element)
        {
            UIPanel itemBackground = (UIPanel)element;
            itemBackground.BorderColor = new Color(0, 0, 0) * 0.7f;
        }

		private void ShowConfirmation()
		{
			confirmationPanel = new ConfirmationPanel(player.whoAmI, selectedItem, HideConfirmation);
			RemoveChild(itemListPanel);
			Append(confirmationPanel);
		}

		private void HideConfirmation()
		{
			RemoveChild(confirmationPanel);
			AddItems();
			Append(itemListPanel);
		}
    }
}
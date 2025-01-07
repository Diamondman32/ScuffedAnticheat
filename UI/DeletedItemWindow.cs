using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
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
    public class DeletedItemWindow : UIPanelNoClickthrough
    {
		private Player player;
		private float spacing = 80f;
		public List<UIItemSlot> uiItemSlots;

		public DeletedItemWindow(Player player)
		{
			this.player = player;
		}

        public override void OnInitialize()
        {
			// Item Slots
			uiItemSlots = new();
			Item[] items = Network.DeletedItemReponse.targetDeletedItems.ToArray();

			for(int i = 0; i < items.Length; i++)
			{
				uiItemSlots.Add(new UIItemSlot(items, i, 0));
				uiItemSlots[i].Left.Set(spacing*(i%10), 0.01f);
				uiItemSlots[i].Top.Set(spacing*(i/10), 0.01f);
				Append(uiItemSlots[i]);
			}
			if(items.Length == 0)
				Main.NewText("no items bozo");
        }
    }
}
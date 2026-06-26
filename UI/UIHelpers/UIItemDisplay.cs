using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;

namespace ScuffedAnticheatMod.UI.UIHelpers;

public class UIItemDisplay : UIElement
{
    private Item _item;

    private int _itemSlotContext;

    public UIItemDisplay(Item item, int itemSlotContext)
    {
        _item = item;
        _itemSlotContext = itemSlotContext;
        Width = new StyleDimension(48f, 0f);
        Height = new StyleDimension(48f, 0f);
    }

    private void HandleItemSlotLogic()
    {
        if (IsMouseHovering)
        {
            Main.LocalPlayer.mouseInterface = true;
            ItemSlot.OverrideHover(ref _item, _itemSlotContext);
            ItemSlot.MouseHover(ref _item, _itemSlotContext);
        }
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        HandleItemSlotLogic();
        Vector2 position = GetDimensions().Center() + new Vector2(52f, 52f) * -0.5f * Main.inventoryScale;
        ItemSlot.Draw(spriteBatch, ref _item, _itemSlotContext, position);
    }
}
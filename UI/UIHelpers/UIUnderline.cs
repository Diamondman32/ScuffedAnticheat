using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace ScuffedAnticheatMod.UI.UIHelpers;

public class UIUnderline : UIElement
{
    public static Asset<Texture2D> _texture;

    public Color Color;

    public int EdgeWidth;

    public UIUnderline(int EdgeWidth = 1)
    {
        this.EdgeWidth = EdgeWidth;
        Color = Color.White;
        _texture = ModContent.Request<Texture2D>("ScuffedAnticheatMod/Images/Separator3", AssetRequestMode.ImmediateLoad);

        Width.Set(_texture.Width(), 0f);
        Height.Set(_texture.Height(), 0f);
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        CalculatedStyle dimensions = GetDimensions();
        Utils.DrawPanel(_texture.Value, EdgeWidth, 0, spriteBatch, dimensions.Position(), dimensions.Width, Color);
    }

    public override bool ContainsPoint(Vector2 point)
    {
        return false;
    }
}
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.UI.Chat;

namespace ScuffedAnticheatMod.UI.UIHelpers
{
    public class UICenteredText : UIText
    {
        public UICenteredText(string text, float scale, bool largeText=false) : base(text, scale, largeText)
        {
            Vector2 textSize = ChatManager.GetStringSize(FontAssets.MouseText.Value, text, new Vector2(scale));
            Left = StyleDimension.FromPixelsAndPercent(-textSize.X/2, 0.5f);
            Top = StyleDimension.FromPixelsAndPercent(-textSize.Y/3, 0.5f);
        }
        public UICenteredText(string text, float scale, Color textColor, bool largeText=false) : this(text, scale, largeText)
        {
            TextColor = textColor;
        }
    }
}
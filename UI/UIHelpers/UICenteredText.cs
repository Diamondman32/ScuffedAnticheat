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
            float textWidth = ChatManager.GetStringSize(FontAssets.MouseText.Value, text, new Vector2(scale)).X;
            Left = StyleDimension.FromPixelsAndPercent(-textWidth/2, 0.5f);
            Top = StyleDimension.FromPixelsAndPercent(0f, 0.025f);
        }
        public UICenteredText(string text, float scale, Color textColor, bool largeText=false) : base(text, scale, largeText)
        {
            float textWidth = ChatManager.GetStringSize(FontAssets.MouseText.Value, text, new Vector2(scale)).X;
            Left = StyleDimension.FromPixelsAndPercent(-textWidth/2, 0.5f);
            Top = StyleDimension.FromPixelsAndPercent(0f, 0.025f);
            TextColor = textColor;
        }
    }
}
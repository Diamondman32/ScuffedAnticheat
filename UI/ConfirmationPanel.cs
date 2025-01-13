using Microsoft.Xna.Framework;
using ScuffedAnticheatMod.UI.UIHelpers;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace ScuffedAnticheatMod.UI
{
    public class ConfirmationPanel : UIPanel
    {
        public void Init()
        {
            BackgroundColor = new Color(255, 255, 255) * 0.04f;
            BorderColor = new Color(0, 0, 0) * 0.4f;
            Left = StyleDimension.FromPixelsAndPercent(0f, 0.025f);
            Top = StyleDimension.FromPixelsAndPercent(0f, 0.1f);
            Width = StyleDimension.FromPixelsAndPercent(0f, 0.95f);
            Height = StyleDimension.FromPixelsAndPercent(0f, 0.875f);
			SetPadding(0);
			
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
			// yesButton.OnClick += itemSlotBackgrounds_onClick;
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
			// noButton.OnClick += itemSlotBackgrounds_onClick;
			noButton.OnMouseOver += UIPanel_onHover;
			noButton.OnMouseOut += UIPanel_onStopHover;
			Append(noButton);

			// Text
            UICenteredText noText = new UICenteredText("No", 1f);
            noButton.Append(noText);
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
using Microsoft.Xna.Framework;
using Terraria.UI;

namespace ScuffedAnticheatMod.UI
{
    class ItemRecoveryUI : UIState
    {
        private PlayerWindow playerWindow;
        private static int lastTime = 5;

        public override void OnInitialize()
        {
            playerWindow = new PlayerWindow();
            playerWindow.Init();
            Append(playerWindow);
        }
        public override void OnActivate()
        {
            playerWindow.UpdatePlayerList();
        }
        public override void OnDeactivate()
        {
            playerWindow.Reset();
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Updates twice a second
            int curTime = gameTime.TotalGameTime.Milliseconds;
            if((curTime >= 500 && lastTime < 500) || (curTime < 500 && lastTime >= 500))
                playerWindow.UpdatePlayerList();
            lastTime = gameTime.TotalGameTime.Milliseconds;
        }
    }
}
// TODO:
// Add Hover border color on items
// Push underline down
// Impl for item returning
// add color from items to player entries
// Block scrollbar from scrolling hotbar when hovering over list
using Microsoft.Xna.Framework;
using Terraria.UI;

namespace ScuffedAnticheatMod.UI
{
    class ItemRecoveryUI : UIState
    {
        public PlayerWindow playerWindow;
        private int lastTime = 5;

        public override void OnInitialize()
        {
            playerWindow = new PlayerWindow();
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
using Microsoft.Xna.Framework;
using Terraria.UI;

namespace ScuffedAnticheatMod.UI
{
    class ItemRecoveryUI : UIState
    {
        public PlayerListWindow playerListWindow;
        private int lastTime = 5;

        public override void OnInitialize()
        {
            playerListWindow = new PlayerListWindow();
            Append(playerListWindow);
        }
        public override void OnActivate()
        {
            playerListWindow.UpdatePlayerList();
        }
        public override void OnDeactivate()
        {
            playerListWindow.Reset();
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Updates twice a second
            int curTime = gameTime.TotalGameTime.Milliseconds;
            if((curTime >= 500 && lastTime < 500) || (curTime < 500 && lastTime >= 500))
                playerListWindow.UpdatePlayerList();
            lastTime = gameTime.TotalGameTime.Milliseconds;
        }
    }
}
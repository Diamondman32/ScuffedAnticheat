using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace VIRUS.UI
{
	[Autoload(Side = ModSide.Client)]
	public class MenuBarSystem : ModSystem
	{
		internal PlayerWindow playerWindow;
		private UserInterface _playerWindow;

		public void TogglePlayerList()
		{
			if(_playerWindow.CurrentState == null)
				_playerWindow.SetState(playerWindow);
			else
				_playerWindow.SetState(null);
		}

        public override void Load()
        {
            playerWindow = new PlayerWindow();
			playerWindow.Activate();
			_playerWindow = new UserInterface();
			_playerWindow.SetState(null);
        }
        public override void UpdateUI(GameTime gameTime)
        {
            _playerWindow?.Update(gameTime);
        }
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (mouseTextIndex != -1)
			{
				layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
					"ScuffedAnticheat: UI",
					delegate
					{
						_playerWindow.Draw(Main.spriteBatch, new GameTime());
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}
    }
}
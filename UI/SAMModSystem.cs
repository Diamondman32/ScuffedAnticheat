using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace ScuffedAnticheatMod.UI
{
	[Autoload(Side = ModSide.Client)]
	public class SAMModSystem : ModSystem
	{
		internal ItemRecoveryUI playerListWindow;
		private UserInterface _playerListWindow;

		public void TogglePlayerList()
		{
			if(_playerListWindow.CurrentState == null)
			{
				_playerListWindow.SetState(playerListWindow);
			}
			else
				_playerListWindow.SetState(null);
		}

        public override void Load()
        {
            playerListWindow = new ItemRecoveryUI();
			playerListWindow.Activate();
			_playerListWindow = new UserInterface();
			_playerListWindow.SetState(null);
        }

        public override void UpdateUI(GameTime gameTime)
        {
            _playerListWindow?.Update(gameTime);
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
						_playerListWindow.Draw(Main.spriteBatch, new GameTime());
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}
    }
}
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace VIRUS.UI
{
	[Autoload(Side = ModSide.Client)]
	public class VIRUSModSystem : ModSystem
	{
		internal PlayerList PlayerList;
		private UserInterface _menuBar;

        public override void Load()
        {
            PlayerList = new PlayerList();
			PlayerList.Activate();
			_menuBar = new UserInterface();
			_menuBar.SetState(null);
        }
        public override void UpdateUI(GameTime gameTime)
        {
            _menuBar?.Update(gameTime);
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
						_menuBar.Draw(Main.spriteBatch, new GameTime());
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}
    }
}
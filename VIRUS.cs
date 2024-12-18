using Terraria.ModLoader;
using Terraria;
using System.IO;

namespace VIRUS
{
    public struct PlayerInventory(int id, Item[] inventory, Item[] armor, Item[] accessories)
    {
		int id = id;
		Item[] inventory = inventory;
		Item[] armor = armor;
		Item[] accessories = accessories;
    }

    // Main
    public class VIRUS : Mod
	{
		public static VIRUS instance;

        public override void Load()
        {
			instance = this;
            base.Load();
        }
        public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			Network.HEROsModMessaged(reader, whoAmI);
		}
    }
}
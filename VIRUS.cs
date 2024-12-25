using Terraria.ModLoader;
using System.IO;
using Terraria;
using Terraria.ID;

namespace VIRUS
{
    public class VIRUS : Mod
	{
		public static VIRUS instance;

        public override void Load()
        {
			instance = this;
            if(Main.netMode == NetmodeID.Server)
                Directory.CreateDirectory(Main.SavePath);
        }
        public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
            Network.VIRUSMessaged(reader, whoAmI);
		}
    }
}

// TODO:
//  Create json file and check serialization/deserialization                                  -- NEEDS CHECKING
//  Create UpdateInv method                                                                   -- NEEDS CHECKING
//  Find a way to check Calamity's extra accessory slot
//  Add a place where "deleted" items are recorded

//  Side Projects:
//  Consider tracking and updating player position
//  Add an integrated in-game menu that can access "deleted" items
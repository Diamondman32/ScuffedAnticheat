using Terraria.ModLoader;
using Terraria;

namespace ScuffedAnticheatMod
{
	public class TogglePlayerList : ModCommand
	{
		// CommandType.Chat means that command can be used in Chat in SP and MP
		public override CommandType Type
			=> CommandType.Chat;

		// The desired text to trigger this command
		public override string Command
			=> "toggle";

		// A short description of this command
		public override string Description
			=> "Toggles Player List";

		public override void Action(CommandCaller caller, string input, string[] args) {
			// ModContent.GetInstance<UI.SAMModSystem>().TogglePlayerList();
			Main.LocalPlayer.ToggleInv();
		}
	}
}
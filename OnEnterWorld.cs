using Terraria;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ScuffedAnticheatMod;

public class OnEnterWorld
{
    public enum ActionTypes { SendMessage, InsertMouseItem, UpdateLocation }
    private static readonly List<(ActionTypes, object)> onEnterWorldActions = new();

    public static void AddEnterWorldAction(ActionTypes type, object data)
    {
        onEnterWorldActions.Add((type, data));
    }

    public static void ChangeSpawnLocation(On.Terraria.Player.orig_Spawn orig, Player self, PlayerSpawnContext context)
    {
        orig(self, context);

        Main.NewText(context);
        if (context == PlayerSpawnContext.SpawningIntoWorld)
        {
            foreach ((ActionTypes type, object data) in onEnterWorldActions)
            {
                switch (type)
                {
                    case ActionTypes.SendMessage:
                        break;
                    case ActionTypes.InsertMouseItem:
                        break;
                    case ActionTypes.UpdateLocation:
                        Vector2 position = (Vector2) data;
                        Main.LocalPlayer.Teleport(position, -1);
                        Main.NewText("fart");
                        Main.screenPosition = Main.LocalPlayer.position - new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
                        break;
                }
            }
        }
        onEnterWorldActions.Clear();
    }
}
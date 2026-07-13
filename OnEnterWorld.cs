using Terraria;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.Chat;
using Terraria.Localization;

namespace ScuffedAnticheatMod;

public class OnEnterWorld
{
    public enum ActionTypes { SendMessage, InsertMouseItem, UpdateLocation }
    private static readonly List<(ActionTypes, object)> onEnterWorldActions = new();
    private static Item mouseItem;

    public static void AddEnterWorldAction(ActionTypes type, object data)
    {
        onEnterWorldActions.Add((type, data));
    }

    // TODO: If join is unsuccessful, some notifications may not go out that maybe should
    public static void ChangeSpawnLocation(On.Terraria.Player.orig_Spawn orig, Player self, PlayerSpawnContext context)
    {
        orig(self, context);

        if (context == PlayerSpawnContext.SpawningIntoWorld)
        {
            foreach ((ActionTypes type, object data) in onEnterWorldActions)
            {
                switch (type)
                {
                    case ActionTypes.SendMessage:
                        string message = (string) data;
                        ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(message), Color.Purple);
                        break;
                    case ActionTypes.InsertMouseItem:
                        mouseItem = (Item) data;
                        On.Terraria.Player.Update -= OnFirstUpdate;
                        On.Terraria.Player.Update += OnFirstUpdate;
                        break;
                    case ActionTypes.UpdateLocation:
                        Vector2 position = (Vector2) data;
                        Main.LocalPlayer.Teleport(position, -1);
                        Main.screenPosition = Main.LocalPlayer.position - new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
                        break;
                }
            }
        }
        onEnterWorldActions.Clear();
    }

    private static void OnFirstUpdate(On.Terraria.Player.orig_Update orig, Player self, int i)
    {
        orig(self, i);

        if (mouseItem != null)
        {
            Main.playerInventory = true;
            Main.mouseItem = mouseItem;
            mouseItem = null;
        }
        On.Terraria.Player.Update -= OnFirstUpdate;
    }
}
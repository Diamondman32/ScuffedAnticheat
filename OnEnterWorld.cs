using Terraria;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ScuffedAnticheatMod;

public class OnEnterWorld
{
    private static readonly List<(string, Color)> playerNotifications = new();
    private static Vector2? playerLocation = null;
    private static Item mouseItem = null;

    // Add message that is displayed in chat on player join
    public static void AddChatNotification(string message, Color color)
    {
        playerNotifications.Add((message, color));
    }

    // Set location that player is moved when they rejoin the world
    public static void SetPlayerStarterLocation(Vector2 location)
    {
        playerLocation = location;
    }

    // Add mouse item that is put into the mouse item slot when the player joins
    public static void SetMouseItem(Item item)
    {
        mouseItem = item;
    }

    // Detour of Player.Spawn that moves the player to their last known location and conditionally hooks PostUpdate if there are any messages or a mouseItem
    public static void ChangeSpawnLocation(On.Terraria.Player.orig_Spawn orig, Player self, PlayerSpawnContext context)
    {
        orig(self, context);

        if (context == PlayerSpawnContext.SpawningIntoWorld)
        {
            if (playerLocation != null)
            {
                Main.LocalPlayer.Teleport((Vector2) playerLocation, -1);
                Main.screenPosition = Main.LocalPlayer.position - new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
                playerLocation = null;
            }

            if (playerNotifications.Count > 0 || mouseItem != null)
            {
                On.Terraria.Player.Update -= OnFirstUpdate;
                On.Terraria.Player.Update += OnFirstUpdate;
            }
        }
    }
    // TODO: config: Concurrent save vs synced with world save

    // Detour of Player.Update that finishes mouseItem and message work from above
    private static void OnFirstUpdate(On.Terraria.Player.orig_Update orig, Player self, int i)
    {
        orig(self, i);

        if (mouseItem != null)
        {
            Main.playerInventory = true;
            Main.mouseItem = mouseItem;
            mouseItem = null;
        }

        if (playerNotifications.Count > 0)
        {
            foreach((string message, Color color) in playerNotifications)
                Main.NewText(message, color);
            playerNotifications.Clear();
        }

        On.Terraria.Player.Update -= OnFirstUpdate;
    }
}
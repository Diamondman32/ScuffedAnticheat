using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace ScuffedAnticheatMod
{
    class SAMModServerConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [Header("Mod Control")]

        [Label("Enforce Server Mods")]
        [Tooltip("If true, the server will reject players that have any additional client-side mods that the server instance does not have.")]
        [DefaultValue(true)]
        public bool ServerEnforcedMods { get; set; }

        [Header("Inventory Tracking")]

        [Label("Server-Controlled Inventory Data")]
        [Tooltip("If true, the server will forcefully modify player inventory data to what it was on the world previously.\nAll \"Deleted\" items can be returned by an admin.")]
        [DefaultValue(true)]
        public bool ServerControlledInvs { get; set; }

        [Label("Inventory Save Interval")]
        [Tooltip("Controls how often the server saves all player inventory changes.\n0 for save on each inventory change.")]
        [Range(0, 60)]
        [DefaultValue(0)]
        [Increment(1)]
        [DrawTicks]
        public int InventoryUpdateInterval { get; set; }

        [Header("Position Tracking")]

        [Label("Server Position Tracking")]
        [Tooltip("If true, the server will track player position and teleport joining players to where they they were in the world previously.")]
        [DefaultValue(true)]
        public bool ServerControlledPosition { get; set; }

        [Label("Player Position Save Interval")]
        [Tooltip("Controls how often the server saves position data.")]
        [Range(1, 60)]
        [DefaultValue(5)]
        [Increment(1)]
        [DrawTicks]
        public int LocationTrackingUpdateInterval { get; set; }
    }
}
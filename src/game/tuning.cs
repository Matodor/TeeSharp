using System.Collections.Generic;

namespace Teecsharp
{
    public partial class CTuningParams
    {
        private static readonly Dictionary<string, CTuneParam> default_Tuning = new Dictionary<string, CTuneParam>
        {
            // physics tuning
            { "GroundControlSpeed", new CTuneParam("GroundControlSpeed", "ground_control_speed", 10.0f) },
            { "GroundControlAccel", new CTuneParam("GroundControlAccel", "ground_control_accel", 100.0f / TicksPerSecond) },
            { "GroundFriction", new CTuneParam("GroundFriction", "ground_friction", 0.5f) },
            { "GroundJumpImpulse", new CTuneParam("GroundJumpImpulse", "ground_jump_impulse", 13.200f) },
            { "AirJumpImpulse", new CTuneParam("AirJumpImpulse", "air_jump_impulse", 12.0f) },
            { "AirControlSpeed", new CTuneParam("AirControlSpeed", "air_control_speed", 250.0f / TicksPerSecond) },
            { "AirControlAccel", new CTuneParam("AirControlAccel", "air_control_accel", 1.5f) },
            { "AirFriction", new CTuneParam("AirFriction", "air_friction", 0.95f) },
            { "HookLength", new CTuneParam("HookLength", "hook_length", 380.0f) },
            { "HookFireSpeed", new CTuneParam("HookFireSpeed", "hook_fire_speed", 80.0f) },
            { "HookDragAccel", new CTuneParam("HookDragAccel", "hook_drag_accel", 3.0f) },
            { "HookDragSpeed", new CTuneParam("HookDragSpeed", "hook_drag_speed", 15.0f) },
            { "Gravity", new CTuneParam("Gravity", "gravity", 0.5f) },

            { "VelrampStart", new CTuneParam("VelrampStart", "velramp_start", 550) },
            { "VelrampRange", new CTuneParam("VelrampRange", "velramp_range", 2000) },
            { "VelrampCurvature", new CTuneParam("VelrampCurvature", "velramp_curvature", 1.4f) },

            // weapon tuning
            { "GunCurvature", new CTuneParam("GunCurvature", "gun_curvature", 1.25f) },
            { "GunSpeed", new CTuneParam("GunSpeed", "gun_speed", 2200.0f) },
            { "GunLifetime", new CTuneParam("GunLifetime", "gun_lifetime", 2.0f) },

            { "ShotgunCurvature", new CTuneParam("ShotgunCurvature", "shotgun_curvature", 1.25f) },
            { "ShotgunSpeed", new CTuneParam("ShotgunSpeed", "shotgun_speed", 2750.0f) },
            { "ShotgunSpeeddiff", new CTuneParam("ShotgunSpeeddiff", "shotgun_speeddiff", 0.8f) },
            { "ShotgunLifetime", new CTuneParam("ShotgunLifetime", "shotgun_lifetime", 0.20f) },

            { "GrenadeCurvature", new CTuneParam("GrenadeCurvature", "grenade_curvature", 7.0f) },
            { "GrenadeSpeed", new CTuneParam("GrenadeSpeed", "grenade_speed", 1000.0f) },
            { "GrenadeLifetime", new CTuneParam("GrenadeLifetime", "grenade_lifetime", 2.0f) },

            { "LaserReach", new CTuneParam("LaserReach", "laser_reach", 800.0f) },
            { "LaserBounceDelay", new CTuneParam("LaserBounceDelay", "laser_bounce_delay", 150) },
            { "LaserBounceNum", new CTuneParam("LaserBounceNum", "laser_bounce_num", 1) },
            { "LaserBounceCost", new CTuneParam("LaserBounceCost", "laser_bounce_cost", 0) },
            { "LaserDamage", new CTuneParam("LaserDamage", "laser_damage", 5) },

            { "PlayerCollision", new CTuneParam("PlayerCollision", "player_collision", 1) },
            { "PlayerHooking", new CTuneParam("PlayerHooking", "player_hooking", 1) },
        };
    }
}

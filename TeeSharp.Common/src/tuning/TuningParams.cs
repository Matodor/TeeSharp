using System.Collections.Generic;

namespace TeeSharp.Common
{
    public class TuningParams : BaseTuningParams
    {
        public TuningParams()
        {
            AppendParameters(new Dictionary<string, TuningParameter>
            {
                // physics tuning
                { "GroundControlSpeed", new TuningParameter("GroundControlSpeed", "ground_control_speed", 10.0f) },
                { "GroundControlAccel", new TuningParameter("GroundControlAccel", "ground_control_accel", 100.0f / 50) },
                { "GroundFriction", new TuningParameter("GroundFriction", "ground_friction", 0.5f) },
                { "GroundJumpImpulse", new TuningParameter("GroundJumpImpulse", "ground_jump_impulse", 13.200f) },
                { "AirJumpImpulse", new TuningParameter("AirJumpImpulse", "air_jump_impulse", 12.0f) },
                { "AirControlSpeed", new TuningParameter("AirControlSpeed", "air_control_speed", 250.0f / 50) },
                { "AirControlAccel", new TuningParameter("AirControlAccel", "air_control_accel", 1.5f) },
                { "AirFriction", new TuningParameter("AirFriction", "air_friction", 0.95f) },
                { "HookLength", new TuningParameter("HookLength", "hook_length", 380.0f) },
                { "HookFireSpeed", new TuningParameter("HookFireSpeed", "hook_fire_speed", 80.0f) },
                { "HookDragAccel", new TuningParameter("HookDragAccel", "hook_drag_accel", 3.0f) },
                { "HookDragSpeed", new TuningParameter("HookDragSpeed", "hook_drag_speed", 15.0f) },
                { "Gravity", new TuningParameter("Gravity", "gravity", 0.5f) },

                { "VelrampStart", new TuningParameter("VelrampStart", "velramp_start", 550) },
                { "VelrampRange", new TuningParameter("VelrampRange", "velramp_range", 2000) },
                { "VelrampCurvature", new TuningParameter("VelrampCurvature", "velramp_curvature", 1.4f) },

                // weapon tuning
                { "GunCurvature", new TuningParameter("GunCurvature", "gun_curvature", 1.25f) },
                { "GunSpeed", new TuningParameter("GunSpeed", "gun_speed", 2200.0f) },
                { "GunLifetime", new TuningParameter("GunLifetime", "gun_lifetime", 2.0f) },

                { "ShotgunCurvature", new TuningParameter("ShotgunCurvature", "shotgun_curvature", 1.25f) },
                { "ShotgunSpeed", new TuningParameter("ShotgunSpeed", "shotgun_speed", 2750.0f) },
                { "ShotgunSpeeddiff", new TuningParameter("ShotgunSpeeddiff", "shotgun_speeddiff", 0.8f) },
                { "ShotgunLifetime", new TuningParameter("ShotgunLifetime", "shotgun_lifetime", 0.20f) },

                { "GrenadeCurvature", new TuningParameter("GrenadeCurvature", "grenade_curvature", 7.0f) },
                { "GrenadeSpeed", new TuningParameter("GrenadeSpeed", "grenade_speed", 1000.0f) },
                { "GrenadeLifetime", new TuningParameter("GrenadeLifetime", "grenade_lifetime", 2.0f) },

                { "LaserReach", new TuningParameter("LaserReach", "laser_reach", 800.0f) },
                { "LaserBounceDelay", new TuningParameter("LaserBounceDelay", "laser_bounce_delay", 150) },
                { "LaserBounceNum", new TuningParameter("LaserBounceNum", "laser_bounce_num", 1) },
                { "LaserBounceCost", new TuningParameter("LaserBounceCost", "laser_bounce_cost", 0) },
                { "LaserDamage", new TuningParameter("LaserDamage", "laser_damage", 5) },

                { "PlayerCollision", new TuningParameter("PlayerCollision", "player_collision", 1) },
                { "PlayerHooking", new TuningParameter("PlayerHooking", "player_hooking", 1) },
            });
        }
    }
}
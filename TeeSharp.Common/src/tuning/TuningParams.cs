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
                { "GroundControlSpeed", new TuningParameter("ground_control_speed", 10.0f) },
                { "GroundControlAccel", new TuningParameter("ground_control_accel", 100.0f / 50) },
                { "GroundFriction", new TuningParameter("ground_friction", 0.5f) },
                { "GroundJumpImpulse", new TuningParameter("ground_jump_impulse", 13.200f) },
                { "AirJumpImpulse", new TuningParameter("air_jump_impulse", 12.0f) },
                { "AirControlSpeed", new TuningParameter("air_control_speed", 250.0f / 50) },
                { "AirControlAccel", new TuningParameter("air_control_accel", 1.5f) },
                { "AirFriction", new TuningParameter("air_friction", 0.95f) },
                { "HookLength", new TuningParameter("hook_length", 380.0f) },
                { "HookFireSpeed", new TuningParameter("hook_fire_speed", 80.0f) },
                { "HookDragAccel", new TuningParameter("hook_drag_accel", 3.0f) },
                { "HookDragSpeed", new TuningParameter("hook_drag_speed", 15.0f) },
                { "Gravity", new TuningParameter("gravity", 0.5f) },

                { "VelrampStart", new TuningParameter("velramp_start", 550) },
                { "VelrampRange", new TuningParameter("velramp_range", 2000) },
                { "VelrampCurvature", new TuningParameter("velramp_curvature", 1.4f) },

                // weapon tuning
                { "GunCurvature", new TuningParameter("gun_curvature", 1.25f) },
                { "GunSpeed", new TuningParameter("gun_speed", 2200.0f) },
                { "GunLifetime", new TuningParameter("gun_lifetime", 2.0f) },

                { "ShotgunCurvature", new TuningParameter("shotgun_curvature", 1.25f) },
                { "ShotgunSpeed", new TuningParameter("shotgun_speed", 2750.0f) },
                { "ShotgunSpeeddiff", new TuningParameter("shotgun_speeddiff", 0.8f) },
                { "ShotgunLifetime", new TuningParameter("shotgun_lifetime", 0.20f) },

                { "GrenadeCurvature", new TuningParameter("grenade_curvature", 7.0f) },
                { "GrenadeSpeed", new TuningParameter("grenade_speed", 1000.0f) },
                { "GrenadeLifetime", new TuningParameter("grenade_lifetime", 2.0f) },

                { "LaserReach", new TuningParameter("laser_reach", 800.0f) },
                { "LaserBounceDelay", new TuningParameter("laser_bounce_delay", 150) },
                { "LaserBounceNum", new TuningParameter("laser_bounce_num", 1) },
                { "LaserBounceCost", new TuningParameter("laser_bounce_cost", 0) },

                { "PlayerCollision", new TuningParameter("player_collision", 1) },
                { "PlayerHooking", new TuningParameter("player_hooking", 1) },
            });
        }
    }
}
using System.Collections.Generic;
using TeeSharp.Core;

namespace TeeSharp.Common
{
    public class TuningParams : BaseTuningParams
    {
        public TuningParams()
        {
            Parameters = new Dictionary<string, TuningParameter>();

            AppendParameters(new Dictionary<string, TuningParameter>
            {
                // physics tuning
                {"ground_control_speed", new TuningParameter(10.0f)},
                {"ground_control_accel", new TuningParameter(100.0f / 50)},
                {"ground_friction",      new TuningParameter(0.5f)},
                {"ground_jump_impulse",  new TuningParameter(13.200f)},
                {"air_jump_impulse",     new TuningParameter(12.0f)},
                {"air_control_speed",    new TuningParameter(250.0f / 50)},
                {"air_control_accel",    new TuningParameter(1.5f)},
                {"air_friction",         new TuningParameter(0.95f)},
                {"hook_length",          new TuningParameter(380.0f)},
                {"hook_fire_speed",      new TuningParameter(80.0f)},
                {"hook_drag_accel",      new TuningParameter(3.0f)},
                {"hook_drag_speed",      new TuningParameter(15.0f)},
                {"gravity",              new TuningParameter(0.5f)},

                {"velramp_start",        new TuningParameter(550)},
                {"velramp_range",        new TuningParameter(2000)},
                {"velramp_curvature",    new TuningParameter(1.4f)},

                // weapon tuning
                {"gun_curvature",        new TuningParameter(1.25f)},
                {"gun_speed",            new TuningParameter(2200.0f)},
                {"gun_lifetime",         new TuningParameter(2.0f)},

                {"shotgun_curvature",    new TuningParameter(1.25f)},
                {"shotgun_speed",        new TuningParameter(2750.0f)},
                {"shotgun_speeddiff",    new TuningParameter(0.8f)},
                {"shotgun_lifetime",     new TuningParameter(0.20f)},

                {"grenade_curvature",    new TuningParameter(7.0f)},
                {"grenade_speed",        new TuningParameter(1000.0f)},
                {"grenade_lifetime",     new TuningParameter(2.0f)},

                {"laser_reach",          new TuningParameter(800.0f)},
                {"laser_bounce_delay",   new TuningParameter(150)},
                {"laser_bounce_num",     new TuningParameter(1)},
                {"laser_bounce_cost",    new TuningParameter(0)},

                {"player_collision",     new TuningParameter(1)},
                {"player_hooking",       new TuningParameter(1)},
            });
        }

        public override IEnumerator<KeyValuePair<string, TuningParameter>> GetEnumerator()
        {
            return Parameters.GetEnumerator();
        }

        protected override void AppendParameters(IDictionary<string, TuningParameter> parameters)
        {
            foreach (var pair in parameters)
            {
                if (!Parameters.TryAdd(pair.Key, pair.Value))
                    Debug.Log("tuning", $"Parameter '{pair.Key}' already added");
            }
        }

        public override void Reset()
        {
            foreach (var pair in Parameters)
            {
                pair.Value.Value = pair.Value.DefaultValue;
            }
        }
    }
}
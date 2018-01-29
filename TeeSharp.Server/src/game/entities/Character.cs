using TeeSharp.Common;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;

namespace TeeSharp.Server.Game.Entities
{
    public class Character : Entity<Character>
    {
        public override float ProximityRadius { get; protected set; } = 28f;

        public virtual bool IsAlive { get; protected set; }
        public virtual BasePlayer Player { get; protected set; }

        public Character(BasePlayer player, vec2 spawnPos) : base(1)
        {
            Player = player;
            IsAlive = true;

            GameContext.GameController.OnCharacterSpawn(this);
        }
        
        public override void OnSnapshot(int snappingClient)
        {
        }

        public virtual void Die(int clientId, Weapon weapon)
        {
        }

        public virtual  void OnPredictedInput(SnapObj_PlayerInput input)
        {
        }

        public virtual void OnDirectInput(SnapObj_PlayerInput input)
        {
        }

        public virtual void ResetInput()
        {
        }
    }
}
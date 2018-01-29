namespace TeeSharp.Server.Game.Entities
{
    public class Character : Entity<Character>
    {
        public virtual bool IsAlive { get; protected set; }

        public Character() : base(1)
        {
            IsAlive = true;
        }
        
        public override void OnSnapshot(int snappingClient)
        {
        }
    }
}
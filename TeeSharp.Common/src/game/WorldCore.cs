namespace TeeSharp.Common.Game
{
    public class WorldCore
    {
        public virtual CharacterCore[] CharacterCores { get; set; }
        public virtual BaseTuningParams Tuning { get; set; }

        public WorldCore(int characters, BaseTuningParams tuning)
        {
            CharacterCores = new CharacterCore[characters];
            Tuning = tuning;
        }
    }
}
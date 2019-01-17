namespace TeeSharp.Common.Game
{
    public class WorldCore
    {
        public virtual BaseCharacterCore[] CharacterCores { get; set; }
        public virtual BaseTuningParams Tuning { get; set; }

        public WorldCore(int characters, BaseTuningParams tuning)
        {
            CharacterCores = new BaseCharacterCore[characters];
            Tuning = tuning;
        }
    }
}
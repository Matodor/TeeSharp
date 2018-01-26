namespace TeeSharp.Map
{
    public abstract class Layer
    {
        public LayerType Type { get; set; }
        public string Name { get; set; }
        public int Flags { get; set; }

        protected Layer()
        {
            Type = LayerType.INVALID;
            Name = "(invalid)";
        }
    }
}
namespace TeeSharp.Map
{
    public abstract class Layer
    {
        public LayerType Type { get; set; }
        public string Name { get; set; }

        protected Layer()
        {
            Type = LayerType.INVALID;
            Name = "(invalid)";
        }
    }
}
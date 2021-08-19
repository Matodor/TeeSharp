namespace TeeSharp.Core.MinIoC
{
    public interface IContainerService
    {
        Container.IScope Container { get; set; }
    }
}
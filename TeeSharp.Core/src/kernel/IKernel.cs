namespace TeeSharp.Core
{
    public interface IKernel
    {
        T Get<T>();
        Binder<T> Bind<T>();
    }
}
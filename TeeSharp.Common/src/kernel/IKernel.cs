namespace TeeSharp.Common
{
    public interface IKernel
    {
        T Get<T>() where T : BaseInterface;
        Binder<T> Bind<T>() where T : BaseInterface;
    }
}
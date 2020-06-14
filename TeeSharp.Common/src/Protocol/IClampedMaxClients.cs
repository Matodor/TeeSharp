namespace TeeSharp.Common.Protocol
{
    public interface IClampedMaxClients
    {
        void Validate(int maxClients, ref string failedOn);
    }
}
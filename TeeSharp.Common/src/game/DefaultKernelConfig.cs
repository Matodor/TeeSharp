using TeeSharp.Common.Console;
using TeeSharp.Common.Protocol;
using TeeSharp.Common.Storage;
using TeeSharp.Core;
using TeeSharp.Network;

namespace TeeSharp.Common.Game
{
    public class DefaultKernelConfig : IKernelConfig
    {
        public virtual void Load(IKernel kernel)
        {
            kernel.Bind<BaseStorage>().To<Storage.Storage>().AsSingleton();
            kernel.Bind<BaseGameConsole>().To<GameConsole>().AsSingleton();
            kernel.Bind<BaseMapLayers>().To<MapLayers>().AsSingleton();
            kernel.Bind<BaseGameMsgUnpacker>().To<GameMsgUnpacker>().AsSingleton();

            kernel.Bind<BaseTokenManager>().To<TokenManager>();
            kernel.Bind<BaseTokenCache>().To<TokenCache>();

            kernel.Bind<BaseNetworkConnection>().To<NetworkConnection>();
            kernel.Bind<BaseChunkReceiver>().To<ChunkReceiver>();
        }
    }
}
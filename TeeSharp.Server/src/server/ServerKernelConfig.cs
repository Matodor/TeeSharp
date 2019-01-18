using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Common.Game;
using TeeSharp.Core;
using TeeSharp.Network;
using TeeSharp.Server.Game;
using TeeSharp.Server.Game.Entities;

namespace TeeSharp.Server
{
    public class ServerKernelConfig : DefaultKernelConfig
    {
        public override void Load(IKernel kernel)
        {
            base.Load(kernel);

            kernel.Bind<BaseServer>().To<Server>().AsSingleton();
            kernel.Bind<BaseConfig>().To<ServerConfig>().AsSingleton();
            kernel.Bind<BaseGameContext>().To<GameContext>().AsSingleton();
            kernel.Bind<BaseNetworkServer>().To<NetworkServer>().AsSingleton();
            kernel.Bind<BaseRegister>().To<Register>().AsSingleton();
            kernel.Bind<BaseNetworkBan>().To<NetworkBan>().AsSingleton();
            kernel.Bind<BaseVotes>().To<Votes>().AsSingleton();
            kernel.Bind<BaseEvents>().To<Events>().AsSingleton();
            kernel.Bind<BaseMapCollision>().To<MapCollision>().AsSingleton();
            kernel.Bind<BaseGameWorld>().To<GameWorld>().AsSingleton();

            kernel.Bind<BaseServerClient>().To<ServerClient>();
            kernel.Bind<BasePlayer>().To<Player>();
            kernel.Bind<BaseCharacter>().To<Character>();
        }
    }
}
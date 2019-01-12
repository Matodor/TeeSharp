using System.Collections.Generic;
using TeeSharp.Common.Console;
using TeeSharp.Common.Protocol;
using TeeSharp.Common.Snapshots;
using TeeSharp.Core;

namespace TeeSharp.Server
{
    public abstract class BaseServerClient : BaseInterface
    {
        public static readonly int MaxInputSize = SnapshotItemsInfo.GetSize<SnapshotPlayerInput>() / sizeof(int);
        public const int MaxNameLength = 16;
        public const int MaxClanLength = 12;
        public const int MaxInputs = 200;
        public const int AuthedAdmin = 2;
        public const int AuthedModerator = 1;

        public virtual IEnumerator<KeyValuePair<string, ConsoleCommand>> SendCommandsEnumerator { get; set; }

        public class Input
        {
            public int Tick { get; set; }
            public readonly int[] Data;

            public Input()
            {
                Data = new int[MaxInputSize];
            }
        }

        public virtual SnapshotRate SnapshotRate { get; set; }
        public virtual ServerClientState State { get; set; }
        public virtual int Latency { get; set; }
        public virtual int MapChunk { get; set; }
        public virtual int AuthTries { get; set; }

        /// <summary>
        /// 0 - non authed, 1 - moderator, 2 - admin
        /// </summary>
        public virtual int AuthLevel { get; set; }

        public virtual string Name { get; set; }
        public virtual string Clan { get; set; }
        public virtual int Country { get; set; }
        public virtual int Version { get; set; }

        public virtual int LastAckedSnapshot { get; set; }
        public virtual int LastInputTick { get; set; }
        
        public virtual bool Quitting { get; set; }
        public virtual SnapshotStorage SnapshotStorage { get; protected set; }
        public virtual Input[] Inputs { get; protected set; }
        public virtual Input LatestInput { get; protected set; }
        public virtual int CurrentInput { get; set; }

        public abstract void Reset();
    }
}
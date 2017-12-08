using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeeSharp.Server
{
    public enum SnapRate
    {
        INIT = 0,
        FULL,
        RECOVER
    }

    public enum ServerClientState
    {
        EMPTY = 0,
        AUTH,
        CONNECTING,
        READY,
        INGAME,
    }

    public class ServerClient
    {
        public class Input
        {
            public int[] Data;
            public long GameTick;
        }

        public ConsoleAccessLevel AccessLevel;
        public ServerClientState ClientState;
        public string Name;
        public string Clan;
        public int Country;
        public int AuthTries;
        public long Traffic;
        public long TrafficSince;
        public int NextMapChunk;

        public int CurrentInput;
        public Input LatestInput;
        public readonly Input[] Inputs;

        public int LastAckedSnapshot;
        public long LastInputTick;
        public SnapRate SnapRate;
        public int Latency;

        public SnapshotStorage SnapshotStorage { get; }

        public ServerClient()
        {
            Inputs = new Input[200];
            SnapshotStorage = new SnapshotStorage();
        }

        public virtual void Reset()
        {
            AccessLevel = 0;
            Name = "";
            Clan = "";
            Country = -1;
            AuthTries = 0;
            Traffic = 0;
            TrafficSince = 0;
            NextMapChunk = 0;

            SnapshotStorage.PurgeAll();
            LastAckedSnapshot = -1;
            SnapRate = SnapRate.INIT;
        }
    }
}

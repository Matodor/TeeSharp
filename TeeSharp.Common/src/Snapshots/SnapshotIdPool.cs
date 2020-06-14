using TeeSharp.Core;

namespace TeeSharp.Common.Snapshots
{
    public class SnapshotIdPool
    {
        private const int MaxIds = 32 * 1024;

        private enum IDState
        {
            Free = 0,
            Allocated,
            TimeOuted
        }

        private class ID
        {
            public int Next;
            public IDState State;
            public long Timeout;
        }

        private ID[] _ids;
        private int _firstFree;
        private int _firstTimed;
        private int _lastTimed;

        public SnapshotIdPool()
        {
            _ids = new ID[MaxIds];
            for (var i = 0; i < _ids.Length; i++)
                _ids[i] = new ID();

            Reset();
        }

        public void Reset()
        {
            for (var i = 0; i < _ids.Length; i++)
            {
                _ids[i].Next = i + 1;
                _ids[i].State = IDState.Free;
            }

            _ids[_ids.Length - 1].Next = -1;
            _firstFree = 0;
            _firstTimed = -1;
            _lastTimed = -1;
        }

        public void RemoveFirstTimeout()
        {
            var nextTimed = _ids[_firstTimed].Next;

            _ids[_firstTimed].Next = _firstFree;
            _ids[_firstTimed].State = IDState.Free;
            _firstFree = _firstTimed;   

            _firstTimed = nextTimed;
            if (_firstTimed == -1)
                _lastTimed = -1;
        }

        public int NewId()
        {
            var now = Time.Get();
            while (_firstTimed != -1 && _ids[_firstTimed].Timeout < now)
                RemoveFirstTimeout();

            var id = _firstFree;
            if (id == -1)
            {
                Debug.Error("server", "id error");
                return id;
            }

            _firstFree = _ids[_firstFree].Next;
            _ids[id].State = IDState.Allocated;

            return id;
        }

        public void TimeoutIds()
        {
            while (_firstTimed != -1)
                RemoveFirstTimeout();
        }

        public void FreeId(int id)
        {
            if (id < 0)
                return;

            _ids[id].State = IDState.TimeOuted;
            _ids[id].Timeout = Time.Get() + Time.Freq() * 5;
            _ids[id].Next = -1;

            if (_lastTimed != -1)
            {
                _ids[_lastTimed].Next = id;
                _lastTimed = id;
            }
            else
            {
                _firstTimed = id;
                _lastTimed = id;
            }
        }
    }
}
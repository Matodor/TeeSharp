using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;

namespace TeeSharp.Common.Snapshots
{
    public static class SnapObjectsInfo
    {
        private static readonly Dictionary<SnapshotObjects, int> _typesSizes;
        private static readonly Dictionary<SnapshotObjects, Func<BaseSnapObject>> _activators;

        static SnapObjectsInfo()
        {
            var types = new Dictionary<SnapshotObjects, Type>()
            {
                {SnapshotObjects.EVENT_DAMAGEIND, typeof(SnapEvent_DamageInd)},
                {SnapshotObjects.EVENT_DEATH, typeof(SnapEvent_Death)},
                {SnapshotObjects.EVENT_EXPLOSION, typeof(SnapEvent_Explosion)},
                {SnapshotObjects.EVENT_HAMMERHIT, typeof(SnapEvent_HammerHit)},
                {SnapshotObjects.EVENT_SOUNDGLOBAL, typeof(SnapEvent_SoundGlobal)},
                {SnapshotObjects.EVENT_SOUNDWORLD, typeof(SnapEvent_SoundWorld)},
                {SnapshotObjects.EVENT_SPAWN, typeof(SnapEvent_Spawn)},

                {SnapshotObjects.OBJ_CHARACTER, typeof(SnapObj_Character)},
                {SnapshotObjects.OBJ_CLIENTINFO, typeof(SnapObj_ClientInfo)},
                {SnapshotObjects.OBJ_GAMEINFO, typeof(SnapObj_GameInfo)},
                {SnapshotObjects.OBJ_LASER, typeof(SnapObj_Laser)},
                {SnapshotObjects.OBJ_PICKUP, typeof(SnapObj_Pickup)},
                {SnapshotObjects.OBJ_PLAYERINFO, typeof(SnapObj_PlayerInfo)},
                {SnapshotObjects.PlayerInput, typeof(SnapObj_PlayerInput)},
                {SnapshotObjects.OBJ_PROJECTILE, typeof(SnapObj_Projectile)},
                {SnapshotObjects.OBJ_SPECTATORINFO, typeof(SnapObj_SpectatorInfo)},
            };

            _activators = new Dictionary<SnapshotObjects, Func<BaseSnapObject>>(types.Count);
            _typesSizes = new Dictionary<SnapshotObjects, int>(types.Count);

            foreach (var pair in types)
            {
                var activator = CreateActivator(pair.Value);
                _activators.Add(pair.Key, activator);
                _typesSizes.Add(pair.Key, activator().SerializeLength * sizeof(int));
            }
        }

        public static int GetSizeByType(SnapshotObjects type)
        {
            return _typesSizes.ContainsKey(type)
                ? _typesSizes[type]
                : 0;
        }

        public static BaseSnapObject GetInstanceByType(SnapshotObjects type)
        {
            return _activators.ContainsKey(type) 
                ? _activators[type]() 
                : null;
        }

        private static Func<BaseSnapObject> CreateActivator(Type type)
        {
            var constructor = type.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, 
                null, new Type[0], null
            );
            var e = Expression.New(constructor);
            return Expression.Lambda<Func<BaseSnapObject>>(e).Compile();
        }
    }
}
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
        private static readonly Dictionary<SnapshotItems, int> _typesSizes;
        private static readonly Dictionary<SnapshotItems, Func<BaseSnapObject>> _activators;

        static SnapObjectsInfo()
        {
            var types = new Dictionary<SnapshotItems, Type>()
            {
                {SnapshotItems.EVENT_DAMAGEIND, typeof(SnapEvent_Damage)},
                {SnapshotItems.EVENT_DEATH, typeof(SnapEvent_Death)},
                {SnapshotItems.EVENT_EXPLOSION, typeof(SnapEvent_Explosion)},
                {SnapshotItems.EVENT_HAMMERHIT, typeof(SnapEvent_HammerHit)},
                {SnapshotItems.EVENT_SOUNDGLOBAL, typeof(SnapEvent_SoundGlobal)},
                {SnapshotItems.EVENT_SOUNDWORLD, typeof(SnapEvent_SoundWorld)},
                {SnapshotItems.EVENT_SPAWN, typeof(SnapEvent_Spawn)},

                {SnapshotItems.OBJ_CHARACTER, typeof(SnapObj_Character)},
                {SnapshotItems.OBJ_CLIENTINFO, typeof(SnapObj_ClientInfo)},
                {SnapshotItems.OBJ_GAMEINFO, typeof(SnapObj_GameInfo)},
                {SnapshotItems.OBJ_LASER, typeof(SnapObj_Laser)},
                {SnapshotItems.OBJ_PICKUP, typeof(SnapObj_Pickup)},
                {SnapshotItems.OBJ_PLAYERINFO, typeof(SnapObj_PlayerInfo)},
                {SnapshotItems.PlayerInput, typeof(SnapObj_PlayerInput)},
                {SnapshotItems.OBJ_PROJECTILE, typeof(SnapObj_Projectile)},
                {SnapshotItems.OBJ_SPECTATORINFO, typeof(SnapObj_SpectatorInfo)},
            };

            _activators = new Dictionary<SnapshotItems, Func<BaseSnapObject>>(types.Count);
            _typesSizes = new Dictionary<SnapshotItems, int>(types.Count);

            foreach (var pair in types)
            {
                var activator = CreateActivator(pair.Value);
                _activators.Add(pair.Key, activator);
                _typesSizes.Add(pair.Key, activator().SerializeLength * sizeof(int));
            }
        }

        public static int GetSizeByType(SnapshotItems type)
        {
            return _typesSizes.ContainsKey(type)
                ? _typesSizes[type]
                : 0;
        }

        public static BaseSnapObject GetInstanceByType(SnapshotItems type)
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
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
        private static readonly Dictionary<SnapObject, int> _typesSizes;
        private static readonly Dictionary<SnapObject, Func<BaseSnapObject>> _activators;

        static SnapObjectsInfo()
        {
            var types = new Dictionary<SnapObject, Type>()
            {
                {SnapObject.EVENT_COMMON, typeof(SnapEvent_DamageInd)},
                {SnapObject.EVENT_DEATH, typeof(SnapEvent_Death)},
                {SnapObject.EVENT_EXPLOSION, typeof(SnapEvent_Explosion)},
                {SnapObject.EVENT_HAMMERHIT, typeof(SnapEvent_HammerHit)},
                {SnapObject.EVENT_SOUNDGLOBAL, typeof(SnapEvent_SoundGlobal)},
                {SnapObject.EVENT_SOUNDWORLD, typeof(SnapEvent_SoundWorld)},
                {SnapObject.EVENT_SPAWN, typeof(SnapEvent_Spawn)},

                {SnapObject.OBJ_CHARACTER, typeof(SnapObj_Character)},
                {SnapObject.OBJ_CLIENTINFO, typeof(SnapObj_ClientInfo)},
                {SnapObject.OBJ_GAMEINFO, typeof(SnapObj_GameInfo)},
                {SnapObject.OBJ_LASER, typeof(SnapObj_Laser)},
                {SnapObject.OBJ_PICKUP, typeof(SnapObj_Pickup)},
                {SnapObject.OBJ_PLAYERINFO, typeof(SnapObj_PlayerInfo)},
                {SnapObject.OBJ_PLAYERINPUT, typeof(SnapObj_PlayerInput)},
                {SnapObject.OBJ_PROJECTILE, typeof(SnapObj_Projectile)},
                {SnapObject.OBJ_SPECTATORINFO, typeof(SnapObj_SpectatorInfo)},
            };

            _activators = new Dictionary<SnapObject, Func<BaseSnapObject>>(types.Count);
            _typesSizes = new Dictionary<SnapObject, int>(types.Count);

            foreach (var pair in types)
            {
                var activator = CreateActivator(pair.Value);
                _activators.Add(pair.Key, activator);
                _typesSizes.Add(pair.Key, activator().SerializeLength * sizeof(int));
            }
        }

        public static int GetSizeByType(SnapObject type)
        {
            return _typesSizes.ContainsKey(type)
                ? _typesSizes[type]
                : 0;
        }

        public static BaseSnapObject GetInstanceByType(SnapObject type)
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
using System;

namespace Teecsharp
{
    public abstract class IConfig : IInterface
    {
        public abstract void Init();
        public abstract void Reset();
        public abstract void RestoreStrings();
        public abstract void Save();

        public abstract void RegisterCallback(Action<IConfig, object> pfnFunc, object pUserData);

        public abstract void WriteLine(string pLine);
    }
}

using System;

namespace TeeSharp.Common.Config
{
    public class ConfigVariableInteger : ConfigVariable
    {
        public event Action<int> OnChange;

        public override object GetValue()
        {
            throw new System.NotImplementedException();
        }
    }
}
using System;

namespace TeeSharp.Common.Config
{
    public class ConfigVariableFloat : ConfigVariable
    {
        public event Action<float> OnChange;

        public override object GetValue()
        {
            throw new System.NotImplementedException();
        }
    }
}
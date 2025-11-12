using System;

namespace CrystalUnbolt
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HideAttribute : ConditionAttribute
    {
        public HideAttribute()
        {

        }
    }
}
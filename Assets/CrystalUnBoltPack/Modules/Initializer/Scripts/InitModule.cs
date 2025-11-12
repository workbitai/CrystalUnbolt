using UnityEngine;

namespace CrystalUnbolt
{
    public abstract class GameModule : ScriptableObject
    {
        public abstract string ModuleName { get; }

        public abstract void CreateComponent();
    }
}
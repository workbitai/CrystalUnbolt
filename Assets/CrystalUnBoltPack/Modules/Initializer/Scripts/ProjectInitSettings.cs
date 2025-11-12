#pragma warning disable 0649

using UnityEngine;

namespace CrystalUnbolt
{
    public class ProjectInitSettings : ScriptableObject
    {
        [SerializeField] GameModule[] modules;
        public GameModule[] Modules => modules;

        public void Init(CrystalInitializer initializer)
        {
            for (int i = 0; i < modules.Length; i++)
            {
                if(modules[i] != null)
                {
                    modules[i].CreateComponent();
                }
            }
        }

        public T GetModule<T>() where T : GameModule
        {
            foreach (var module in modules)
            {
                if (module != null && module is T)
                {
                    return (T)module;
                }
            }

            return null;
        }
    }
}
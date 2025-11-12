using UnityEngine;

namespace CrystalUnbolt
{
    [System.Serializable]
    public class FloatingTextCase
    {
        [SerializeField] string name;
        public string Name => name;

        [SerializeField] CrystalFloatingTextBaseBehavior floatingTextBehavior;
        public CrystalFloatingTextBaseBehavior FloatingTextBehavior => floatingTextBehavior;

        private Pool floatingTextPool;
        public Pool FloatingTextPool => floatingTextPool;

        public void Init()
        {
            floatingTextPool = new Pool(floatingTextBehavior.gameObject);
        }
    }
}
#pragma warning disable 0414

using UnityEngine;

namespace CrystalUnbolt
{
    [RegisterModule("Save Controller", core: true, order: 900)]
    public class SaveInitModule : GameModule
    {
        public override string ModuleName => "Save Controller";

        [SerializeField] float autoSaveDelay = 0;
        [SerializeField] bool cleanSaveStart = false;

        [Space]
        [SerializeField] string webGLPrefix = "gameName";

        public override void CreateComponent()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            BaseSaveWrapper wrapper = BaseSaveWrapper.ActiveWrapper;
            if(wrapper is WebGLSaveWrapper)
            {
                WebGLSaveWrapper webGLWrapper = (WebGLSaveWrapper)wrapper;
                webGLWrapper.Init(webGLPrefix);
            }
#endif

            DataManager.Init(autoSaveDelay, cleanSaveStart);
        }
    }
}

using UnityEngine;

namespace CrystalUnbolt
{
    [System.Serializable]
    public class CrystalPlankSkinData : AbstractSkinData
    {
        [SkinPreview]
        [SerializeField] Sprite previewSprite;
        public Sprite PreviewSprite => previewSprite;

        [SerializeField] CrystalPlanksSkinData planksSkinData;
        public CrystalPlanksSkinData PlanksSkinData => planksSkinData;
    }
}

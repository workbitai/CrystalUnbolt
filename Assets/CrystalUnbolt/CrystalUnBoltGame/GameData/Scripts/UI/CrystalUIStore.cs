using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalUnbolt.IAPStore
{
    public class CrystalUIStore : BaseScreen
    {
        private const float DEFAULT_STORE_HEIGHT_OFFSET = 300;

        [BoxGroup("References", "References")]
        [SerializeField] RectTransform safeAreaTransform;
        [BoxGroup("References")]
        [SerializeField] CrystalCurrencyUIPanelSimple coinsUI;

        [BoxGroup("Scroll View", "Scroll View")]
        [SerializeField] VerticalLayoutGroup layout;
        [BoxGroup("Scroll View")]
        [SerializeField] RectTransform content;

        [BoxGroup("Buttons", "Buttons")]
        [SerializeField] Button closeButton;
        
        private AnimCase[] appearTweenCases;
        private Transform[] offersTransforms;

        private void Awake()
        {
            offersTransforms = new Transform[content.childCount];
            for (int i = 0; i < offersTransforms.Length; i++)
            {
                offersTransforms[i] = content.GetChild(i);
            }

            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        public override void Init()
        {
            Debug.Log("IN Store");
            SafeAreaHandler.RegisterRectTransform(safeAreaTransform);

            coinsUI.Init();
        }

        public override void PlayHideAnimation()
        {
            ScreenManager.OnPageClosed(this);
        }
        public override void PlayShowAnimationMainReturn()
        {

        }
        public override void PlayShowAnimation()
        {
            appearTweenCases.KillActive();

            float height = layout.padding.top + layout.padding.bottom + DEFAULT_STORE_HEIGHT_OFFSET;

            Transform[] activeOffers = offersTransforms.Where(x => x.gameObject.activeSelf).ToArray();
            appearTweenCases = new AnimCase[activeOffers.Length];
            for (int i = 0; i < activeOffers.Length; i++)
            {
                RectTransform offerRectTransform = (RectTransform)activeOffers[i].transform;
                offerRectTransform.localScale = Vector3.zero;

                appearTweenCases[i] = offerRectTransform.DOScale(1.0f, 0.3f, i * 0.05f).SetEasing(Ease.Type.CircOut);

                height += offerRectTransform.sizeDelta.y;
            }

            height += activeOffers.Length * layout.spacing;

            closeButton.transform.localScale = Vector3.zero;
            closeButton.transform.DOScale(1.0f, 0.3f, 0.2f).SetEasing(Ease.Type.BackOut);

            content.sizeDelta = new Vector2(0, height);
            content.anchoredPosition = Vector2.zero;

            appearTweenCases[^1].OnComplete(() =>
            {
                ScreenManager.OnPageOpened(this);
            });
        }

        public void Hide()
        {
            appearTweenCases.KillActive();

            ScreenManager.CloseScreen<CrystalUIStore>();
        }

        private void OnCloseButtonClicked()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif

            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);

            ScreenManager.CloseScreen<CrystalUIStore>();
            MyAdsAdapter.ShowBanner();
        }

        public void SpawnCurrencyCloud(RectTransform spawnRectTransform, CurrencyType currencyType, int amount, GameCallback completeCallback = null)
        {
            FloatingCloud.SpawnCurrency(currencyType.ToString(), spawnRectTransform, coinsUI.RectTransform, amount, null, completeCallback);
        }
    }
}
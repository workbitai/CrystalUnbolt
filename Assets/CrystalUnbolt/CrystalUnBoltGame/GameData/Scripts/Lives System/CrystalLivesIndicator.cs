using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalUnbolt
{
    public class CrystalLivesIndicator : MonoBehaviour
    {
        [Space]
        [SerializeField] TextMeshProUGUI livesCountText;
        [SerializeField] Image infinityImage;
        [SerializeField] TextMeshProUGUI durationText;

        [Space]
        [SerializeField] Button addButton;

        [Space]
        [CachedTarget("durationText")]
       // [SerializeField] CachedRectTransform defaultTransformData;
      //  [CachedTarget("durationText")]
       // [SerializeField] CachedRectTransform fullTransformData;
//
      //  private CanvasGroup canvasGroup;

        private AnimCase canvasGroupTweenCase;

        private bool addButonEnabled;

       // private RectTransform durationRectTransform;

        private void Awake()
        {
          //  canvasGroup = gameObject.GetOrSetComponent<CanvasGroup>();
          //  durationRectTransform = durationText.rectTransform;

            addButonEnabled = false;
        }

        private void Start()
        {
            if (CrystalUIAddLivesPanel.Exists())
            {
                if(addButton != null)
                {
                    addButton.gameObject.SetActive(true);
                    addButton.onClick.AddListener(OnAddButtonClicked);

                    addButonEnabled = true;
                }
            }
            else
            {
                if(addButton != null)
                {
                    addButton.gameObject.SetActive(false);
                }
            }

            OnStatusChanged(CrystalLivesSystem.Status);
        }

        private void OnEnable()
        {
            CrystalLivesSystem.StatusChanged += OnStatusChanged;
        }

        private void OnDisable()
        {
            CrystalLivesSystem.StatusChanged -= OnStatusChanged;
        }

        public void Show()
        {
          //  if (canvasGroup != null)
            {
                canvasGroupTweenCase.KillActive();
//
            //    canvasGroup.alpha = 0;

               // canvasGroupTweenCase = canvasGroup.DOFade(1, 0.3f);
            }
        }

        public void Hide()
        {
           // if (canvasGroup != null)
            {
                canvasGroupTweenCase.KillActive();
             //   canvasGroupTweenCase = canvasGroup.DOFade(0, 0.3f);
            }
        }

        private void OnStatusChanged(CrystalLivesStatus status)
        {
            if(status.InfiniteMode)
            {
                infinityImage.gameObject.SetActive(true);
                livesCountText.gameObject.SetActive(false);

                durationText.text = CrystalLivesSystem.GetFormatedTime(status.InfiniteModeTime);

                if(addButonEnabled)
                    addButton.gameObject.SetActive(false);
            }
            else
            {
                infinityImage.gameObject.SetActive(false);
                durationText.gameObject.SetActive(true);

                livesCountText.gameObject.SetActive(true);
                livesCountText.text = status.LivesCount.ToString();

                if (status.NewLifeTimerEnabled)
                {
                    durationText.text = CrystalLivesSystem.GetFormatedTime(status.NewLifeTime);

                    if (addButonEnabled)
                        addButton.gameObject.SetActive(true);
                }
                else
                {
                    durationText.text = CrystalLivesSystem.TEXT_FULL;

                    if (addButonEnabled)
                        addButton.gameObject.SetActive(false);
                }
            }

            if(addButonEnabled && addButton.gameObject.activeSelf)
            {
              ////  defaultTransformData.Apply(durationRectTransform);
            }
            else
            {
              //  fullTransformData.Apply(durationRectTransform);
            }
        }

        private void OnAddButtonClicked()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            CrystalUIAddLivesPanel.Show();
        }
    }
}
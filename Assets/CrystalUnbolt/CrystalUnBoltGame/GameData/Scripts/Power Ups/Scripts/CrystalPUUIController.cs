using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalUnbolt
{
    public class CrystalPUUIController : MonoBehaviour
    {
        [SerializeField] Transform containerTransform;
        [SerializeField] GameObject itemPrefab;

        [Space]
        [SerializeField] RectTransform floatingTextRectTransform;
        [SerializeField] TextMeshProUGUI floatingText;
        [SerializeField] float floatingTextDelay = 1.0f;
        [SerializeField] Ease.Type floatingTextEasing = Ease.Type.QuartOut;

        [Space]
        [SerializeField] CrystalPUUIPurchasePanel powerUpPurchasePanel;
        public CrystalPUUIPurchasePanel PowerUpPurchasePanel => powerUpPurchasePanel;

        [SerializeField] CrystalPUUIUnlockPanel powerUpUnlockPanel;
        public CrystalPUUIUnlockPanel PowerUpUnlockPanel => powerUpUnlockPanel;

        private CrystalPUController powerUpController;

        private CrystalPUUIBehavior[] uiBehaviors;
        public CrystalPUUIBehavior[] UIBehaviors => uiBehaviors;

        private float defaultFloatingTextWidth;
        private Vector2 defaultFloatingTextPosition;
        private AnimCase floatingTextTweenCase;
        private RectTransform rectTransform;

        public void Initialise(CrystalPUController powerUpController)
        {
            this.powerUpController = powerUpController;

            rectTransform = (RectTransform)transform;

            defaultFloatingTextPosition = floatingTextRectTransform.anchoredPosition;
            defaultFloatingTextWidth = floatingTextRectTransform.sizeDelta.x;

            CrystalPUBehavior[] activePowerUps = CrystalPUController.ActivePowerUps;
            uiBehaviors = new CrystalPUUIBehavior[activePowerUps.Length];

            for (int i = 0; i < activePowerUps.Length; i++)
            {
                GameObject itemObject = Instantiate(itemPrefab, containerTransform);

                uiBehaviors[i] = itemObject.GetComponent<CrystalPUUIBehavior>();
                uiBehaviors[i].Initialise(activePowerUps[i]);
            }

            powerUpPurchasePanel.Init();
        }

        private void Update()
        {
            if (uiBehaviors == null)
                return;

            foreach(CrystalPUUIBehavior uiBehavior in uiBehaviors)
            {
                if (uiBehavior.Behavior.IsDirty)
                {
                    Debug.Log("Redraw");

                    uiBehavior.Redraw();
                }
            }
        }

        public void OnLevelStarted(int levelNumber)
        {
            List<CrystalPUSettings> unlockedPowerUps = new List<CrystalPUSettings>();

            for (int i = 0; i < uiBehaviors.Length; i++)
            {
                if(uiBehaviors[i].Behavior.IsActive())
                {
                    uiBehaviors[i].Activate();
                    uiBehaviors[i].SetBlockState(levelNumber);

                    CrystalPUSettings settings = uiBehaviors[i].Settings;
                    if (settings.RequiredLevel != 0 && !settings.IsUnlocked && settings.RequiredLevel == levelNumber)
                    {
                        unlockedPowerUps.Add(settings);
                    }
                }
                else
                {
                    uiBehaviors[i].Disable();
                }

                uiBehaviors[i].OnLevelStarted(levelNumber);
            }

            if(!unlockedPowerUps.IsNullOrEmpty())
            {
                CrystalPUController.PowerUpsUIController.PowerUpUnlockPanel.Show(unlockedPowerUps);
            }
        }

        public void OnStageStarted()
        {
            for (int i = 0; i < uiBehaviors.Length; i++)
            {
                uiBehaviors[i].OnStageStarted();
            }
        }

        public void OnLevelFinished()
        {
            for (int i = 0; i < uiBehaviors.Length; i++)
            {
                if (uiBehaviors[i].Behavior.IsActive())
                {
                    uiBehaviors[i].Behavior.ResetBehavior();
                    uiBehaviors[i].OnLevelFinished();
                    uiBehaviors[i].Redraw();
                    Debug.Log("Redraw");

                }
            }
        }

        public void OnPowerUpUsed(CrystalPUBehavior powerUpBehavior)
        {
            RedrawPanels();

            string floatingMessageText = powerUpBehavior.GetFloatingMessage();
            if (!string.IsNullOrEmpty(floatingMessageText))
                SpawnFloatingText(floatingMessageText);
        }

        public void SpawnFloatingText(string text)
        {
            floatingTextTweenCase.KillActive();

            floatingTextRectTransform.gameObject.SetActive(true);

            floatingText.text = text;
            floatingText.ForceMeshUpdate();

            float prefferedHeight = LayoutUtility.GetPreferredHeight(floatingText.rectTransform);

            floatingTextRectTransform.sizeDelta = new Vector2(defaultFloatingTextWidth, prefferedHeight);
            floatingTextRectTransform.anchoredPosition = defaultFloatingTextPosition;

            floatingTextTweenCase = floatingTextRectTransform.DOAnchoredPosition(new Vector2(defaultFloatingTextPosition.x, defaultFloatingTextPosition.y + 50), floatingTextDelay).SetEasing(floatingTextEasing).OnComplete(() =>
            {
                floatingTextRectTransform.gameObject.SetActive(false);
            });
        }

        public void RedrawPanels()
        {
            for (int i = 0; i < uiBehaviors.Length; i++)
            {
                Debug.Log("Redraw");

                uiBehaviors[i].Redraw();
            }
        }

        public void HidePanels()
        {
            foreach (var uiBehavior in uiBehaviors)
            {
                uiBehavior.gameObject.SetActive(false);
            }
        }

        public void HidePanel(CrystalPUType CrystalPUType)
        {
            foreach (var uiBehavior in uiBehaviors)
            {
                if (uiBehavior.Settings.Type == CrystalPUType)
                {
                    uiBehavior.gameObject.SetActive(false);

                    break;
                }
            }
        }

        public void ShowPanels()
        {
            foreach (var uiBehavior in uiBehaviors)
            {
                if (uiBehavior.IsActive)
                {
                    uiBehavior.gameObject.SetActive(true);
                }
            }
        }

        public void ShowPanel(CrystalPUType CrystalPUType)
        {
            foreach (var uiBehavior in uiBehaviors)
            {
                if (uiBehavior.IsActive && uiBehavior.Settings.Type == CrystalPUType)
                {
                    uiBehavior.gameObject.SetActive(true);

                    break;
                }
            }
        }

        public CrystalPUUIBehavior GetPanel(CrystalPUType CrystalPUType)
        {
            foreach (var uiBehavior in uiBehaviors)
            {
                if (uiBehavior.Settings.Type == CrystalPUType)
                {
                    return uiBehavior;
                }
            }

            return null;
        }
    }
}

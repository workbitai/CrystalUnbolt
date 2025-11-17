using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalUnbolt
{
    public class CrystalPUUIUnlockPanel : MonoBehaviour, IPopupWindow
    {
        [SerializeField] GameObject powerUpPanel;

        [Space(5)]
        [SerializeField] Image powerUpPurchasePreview;
        [SerializeField] TMP_Text powerUpPurchaseDescriptionText;

        [Space(5)]
        [SerializeField] Button closeButton;

        public bool IsOpened => powerUpPanel.activeSelf;

        private List<CrystalPUSettings> unlockedPowerUps;
        private int pageIndex = 0;

        [SerializeField] RectTransform panel;

        private void Awake()
        {
            closeButton.onClick.AddListener(ClosePanel);
        }

        public void Show(List<CrystalPUSettings> unlockedPowerUps)
        {
            this.unlockedPowerUps = unlockedPowerUps;

            powerUpPanel.SetActive(true);

            pageIndex = 0;

            PreparePage(pageIndex);

            ScreenManager.OnPopupWindowOpened(this);

            PopupHelper.ShowPopup(panel);
            PopupHelper.ShowPopup(powerUpPurchasePreview.transform);
            PopupHelper.ShowPopup(powerUpPurchaseDescriptionText.transform);
        }

        private void PreparePage(int index)
        {
            if (!unlockedPowerUps.IsInRange(index)) return;

            CrystalPUSettings settings = unlockedPowerUps[index];

            powerUpPurchasePreview.sprite = settings.Icon;
           
            powerUpPurchaseDescriptionText.text = settings.Description;

            powerUpPurchasePreview.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            powerUpPurchasePreview.transform.DOScale(1.0f, 0.25f).SetEasing(Ease.Type.BackOut);
        }

        public void ClosePanel()
        {
            pageIndex++;
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            if(pageIndex >= unlockedPowerUps.Count)
            {
                powerUpPanel.SetActive(false);

                ScreenManager.OnPopupWindowClosed(this);

                foreach(CrystalPUSettings unlockerPowerUp in unlockedPowerUps)
                {
                    CrystalPUController.UnlockPowerUp(unlockerPowerUp.Type);
                }
            }
            else
            {
                PreparePage(pageIndex);
            }
        }
    }
}
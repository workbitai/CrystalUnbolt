using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Coffee.UIExtensions;

namespace CrystalUnbolt
{
    public class CrystalPUUIBehavior : MonoBehaviour
    {
        [Group("Refs")]
        [SerializeField] Image backgroundImage;

        [Group("Refs")]
        [SerializeField] Image iconImage;

        [Group("Refs")]
        [SerializeField] Image lockIcon;
        [Group("Refs")]
        [SerializeField] Image lockStateIconImage;

        [Group("Refs")]
        [SerializeField] GameObject defaultElementsObjects;

        [Group("Refs")]
        [SerializeField] GameObject amountContainerObject;

        [Group("Refs")]
        [SerializeField] TextMeshProUGUI amountText;

        [Group("Refs")]
        [SerializeField] GameObject amountPurchaseObject;

        [Group("Refs")]
        [SerializeField] GameObject busyStateVisualsObject;

        [Group("Refs")]
        [SerializeField] GameObject selectedOutlineObject;

        [Space]
        [SerializeField] GameObject timerObject;
        [SerializeField] TextMeshProUGUI timerText;
        [SerializeField] Image timerBackground;

        [Space]
        [SerializeField] GameObject lockStateObject;
        [SerializeField] TextMeshProUGUI lockText;

        [Space]
        [SerializeField] SimpleBounce bounce;

        protected CrystalPUBehavior behavior;
        public CrystalPUBehavior Behavior => behavior;

        protected CrystalPUSettings settings;
        public CrystalPUSettings Settings => settings;

        private Button button;

        private bool isTimerActive;
        private Coroutine timerCoroutine;

        private bool isActive = false;
        public bool IsActive => isActive;

        private bool isLocked = false;
        public bool IsLocked => isLocked;


        private Sequence iconSeq;
        [SerializeField] private ShinyEffectForUGUI shiny;

        private void OnEnable()
        {
            StartCoroutine(ShineLoop());
            iconSeq = IconAnimationHelper.PlayLockIconPremium(
               lockIcon.transform,
               duration: 2.2f,
               scaleUp: 1.08f,
               rotation: 4f,
               glowMin: 0.7f,
               glowMax: 1f,
               startDelay: 0.4f
           );
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            if (lockIcon != null) iconSeq.Kill();
            if (lockIcon != null) lockIcon.transform.localScale = Vector3.one;
        }
        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(() => OnButtonClicked());
        }
        private System.Collections.IEnumerator ShineLoop()
        {
            while (true)
            {
                if (shiny != null && shiny.gameObject.activeInHierarchy)
                {
                    shiny.Play(1.5f);
                }

                yield return new WaitForSeconds(5f);
            }
        }
        public void Initialise(CrystalPUBehavior powerUpBehavior)
        {
            behavior = powerUpBehavior;
            settings = powerUpBehavior.Settings;

            ApplyVisuals();
            Debug.Log("Redraw");
            Redraw();

            bounce.Init(transform);

            gameObject.SetActive(false);

            isActive = false;
        }

        protected virtual void ApplyVisuals()
        {
            iconImage.sprite = settings.Icon;
            iconImage.color = Color.white;

            if (lockStateIconImage != null)
            {
                lockStateIconImage.sprite = settings.Icon;
                lockStateIconImage.color = Color.white;
                lockStateIconImage.enabled = settings.Icon != null;
            }

            backgroundImage.color = settings.BackgroundColor;
        }

        public void SetBlockState(int levelNumber)
        {
            if (levelNumber >= settings.RequiredLevel)
            {
                lockStateObject.SetActive(false);

                defaultElementsObjects.SetActive(true);

                isLocked = false;
            }
            else
            {
                lockStateObject.SetActive(true);
                lockText.text = string.Format("LEVEL {0}", settings.RequiredLevel);

                defaultElementsObjects.SetActive(false);
                if (lockStateIconImage != null && settings.Icon != null)
                {
                    lockStateIconImage.sprite = settings.Icon;
                    lockStateIconImage.enabled = true;
                }

                isLocked = true;
            }
        }

        public void Activate()
        {
            isActive = true;

            gameObject.SetActive(true);

            transform.localScale = Vector3.zero;
            transform.DOScale(1.0f, 0.3f).SetEasing(Ease.Type.BackOut);
            Debug.Log("Redraw");

            Redraw();
        }

        public void Disable()
        {
            isActive = false;

            gameObject.SetActive(false);
        }

        public void OnLevelStarted(int levelNumber)
        {

        }

        public void OnStageStarted()
        {
            if (timerCoroutine != null)
                StopCoroutine(timerCoroutine);

            CrystalUIGame uiGame = ScreenManager.GetPage<CrystalUIGame>();
            CrystalTimerVisualiser timerVisualiser = uiGame.CrystalGameTimer;
            timerVisualiser.SetFreezeFillAmount(0);
        }

        public void OnLevelFinished()
        {
            if (isTimerActive)
            {
                if (timerCoroutine != null)
                {
                    StopCoroutine(timerCoroutine);
                }

                CrystalUIGame uiGame = ScreenManager.GetPage<CrystalUIGame>();
                CrystalTimerVisualiser timerVisualiser = uiGame.CrystalGameTimer;
                timerVisualiser.SetFreezeFillAmount(0);

                timerObject.SetActive(false);
                iconImage.color = Color.white;

                isTimerActive = false;
            }
        }

        private IEnumerator TimerCoroutine(CrystalPUTimer timer)
        {
            isTimerActive = true;

            CrystalUIGame uiGame = ScreenManager.GetPage<CrystalUIGame>();
            CrystalTimerVisualiser timerVisualiser = uiGame.CrystalGameTimer;

            timerObject.SetActive(true);
            timerBackground.fillAmount = 1.0f;
            timerText.text = timer.Seconds;

            iconImage.color = new Color(1, 1, 1, 0.3f);

            while (timer.IsActive)
            {
                yield return null;
                yield return null;

                timerBackground.fillAmount = 1.0f - timer.State;
                timerVisualiser.SetFreezeFillAmount(1 - timer.State);
                timerText.text = timer.Seconds;

                if (timerBackground.fillAmount <= 0.0f)
                    break;
            }

            timerObject.SetActive(false);
            iconImage.color = Color.white;

            isTimerActive = false;
        }

        public void OnButtonClicked()
        {
            if (CrystalUIGame.QueText.gameObject.activeInHierarchy || CrystalLevelController.isRealGameFinish) return;
            if (isLocked) return;

            if (settings.Save.Amount > 0)
            {
                if (!behavior.IsBusy)
                {
                    if (behavior.IsSelectable())
                    {
                        if (CrystalPUController.SelectPowerUp(settings.Type))
                        {
                            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
                            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
                            bounce.Bounce();
                        }
                    }
                    else
                    {
                        if (CrystalPUController.UsePowerUp(settings.Type))
                        {
                            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
                            bounce.Bounce();
                        }
                    }
                }
            }
            else
            {
                CrystalPUController.UnselectPowerUp();

                SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);

                CrystalPUController.PowerUpsUIController.PowerUpPurchasePanel.Show(settings);
            }
        }

        public void Redraw()
        {
            int amount = settings.Save.Amount;
            if (amount > 0)
            {
                amountContainerObject.SetActive(true);
                amountPurchaseObject.SetActive(false);

                amountText.text = amount.ToString();

            }
            else
            {
                amountContainerObject.SetActive(false);
                amountPurchaseObject.SetActive(true);
            }

            CrystalPUTimer timer = behavior.GetTimer();
            if (!isTimerActive)
            {
                if (timer != null)
                {
                    timerCoroutine = StartCoroutine(TimerCoroutine(timer));
                }
            }

            if (settings.VisualiseActiveState)
                RedrawBusyVisuals(behavior.IsBusy);

            if (behavior.IsSelectable())
                selectedOutlineObject.SetActive(behavior.IsSelected);

            behavior.OnRedrawn();
        }

        protected virtual void RedrawBusyVisuals(bool state)
        {
            busyStateVisualsObject.SetActive(behavior.IsBusy);
        }

    }
}
// This is the modified IconAnimationHelper class in CrystalPUUIBehavior.cs

// This is the modified IconAnimationHelper class in CrystalPUUIBehavior.cs

public static class IconAnimationHelper
{
    public static Sequence PlayLockIconPremium(Transform target,
                                           float duration = 2.2f,
                                           float scaleUp = 0.95f,
                                           float rotation = 4f,
                                           float glowMin = 0.7f,
                                           float glowMax = 1f,
                                           float startDelay = 0f)
    {
        if (target == null) return null;

        // Reset
        target.localScale = Vector3.one * 0.7f;
        target.localRotation = Quaternion.identity;

        // Glow via CanvasGroup
        CanvasGroup glow = target.GetComponent<CanvasGroup>();
        if (glow == null)
            glow = target.gameObject.AddComponent<CanvasGroup>();

        glow.alpha = glowMax;

        Sequence seq = DOTween.Sequence();

        if (startDelay > 0)
            seq.AppendInterval(startDelay);

        // Step 1 � soft upscale + tilt
        seq.Append(target.DOScale(Vector3.one * 0.9f * scaleUp, duration * 0.25f).SetEase(Ease.OutQuad));
        seq.Join(target.DOLocalRotate(new Vector3(0, 0, rotation), duration * 0.25f).SetEase(Ease.OutQuad));
        //  seq.Join(glow.DOFade(glowMax, duration * 0.25f));

        // Step 2 � settle back to normal
        seq.Append(target.DOScale(Vector3.one * 0.7f, duration * 0.25f).SetEase(Ease.OutBack));
        seq.Join(target.DOLocalRotate(Vector3.zero, duration * 0.25f).SetEase(Ease.OutQuad));

        // Step 3 � subtle breathing dip
        seq.Append(target.DOScale(Vector3.one * 0.8f * 0.97f, duration * 0.20f).SetEase(Ease.InOutSine));
        // seq.Join(glow.DOFade(glowMin, duration * 0.20f));

        // Step 4 � smooth restore
        seq.Append(target.DOScale(Vector3.one * 0.7f, duration * 0.15f).SetEase(Ease.OutSine));
        //  seq.Join(glow.DOFade(glowMax, duration * 0.15f));

        // Step 5 � pause
        seq.AppendInterval(duration * 0.15f);

        seq.SetLoops(-1, LoopType.Restart);

        return seq;
    }

}
public static class PowerUpLock
{
    public static bool IsLocked = false;
}

using System;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace CrystalUnbolt
{
    public class CrystalUIDailySpin : BaseScreen
    {
        [SerializeField] RectTransform safeAreaTransform;

        [Header("Wheel & UI")]
        public RectTransform Wheel;
        public RectTransform WheelMain;
        public Button SpinButton;
        public GameObject PaidCostBadgeRoot;
        public Text PaidCostText;
        public TextMeshProUGUI NextFreeLabel;
        public Text RewardMessageText;
        public Button CloseButton;
        public ParticleSystem rewardParticleSystem;
        
        [Header("Audio")]
        public AudioClip wheelSpinningSound; 
        
        [Header("New Button Flow")]
        public Button FreeSpinButton;
        public Button PaidSpeedButton;
        public Button AdsButton;
        public TextMeshProUGUI PaidSpeedCostText;
        public TextMeshProUGUI AdsButtonText;

        [Header("CrystalReward Popup (new)")]
        public GameObject RewardPopupRoot;           
        public RectTransform RewardPopupRect;        
        public Image RewardIconImage;                
        public TextMeshProUGUI RewardPopupText;      
        public TextMeshProUGUI RewardPopupTextShadow; 
        public Button RewardPopupCloseButton;

        [Header("Popup Sprites (map to RewardType)")]
        public Sprite CoinSprite;
        public Sprite HolderSprite;
        public Sprite FrozenSprite;
        public Sprite CleanerSprite;
        public Sprite HammerSprite;

        [Header("Popup Animation (Rect-only)")]
        public float PopupOpenDuration = 0.35f;
        public float PopupCloseDuration = 0.22f;
        public float PopupScaleFrom = 0.6f;
        public float PopupScaleOvershoot = 1.05f;
        public float PopupIconPunch = 0.12f;
        public float IconPulseMin = 0.8f;        
        public float IconPulseMax = 1.5f;
        [Header("Icon Idle Animation (while popup open)")]
        public bool IconIdleEnabled = true;
        public float IconPulseMagnitude = 0.06f; 
        public float IconPulseSpeed = 1.0f;      

        [Header("Popup Text Styling (double-color)")]
        public Color TmpGradientTop = Color.white;
        public Color TmpGradientBottom = new Color(1f, 0.92f, 0.0f);
        public Color PopupTextOutlineColor = new Color(0f, 0f, 0f, 0.7f);
        [Range(0f, 0.4f)] public float PopupTextOutlineWidth = 0.12f;
        public Color PopupShadowColor = new Color(1f, 0.85f, 0.2f);

        private static float startedStorePanelRectPositionY;
        private readonly float PANEL_BOTTOM_OFFSET_Y = -2000f;

        [Header("Spin Settings")]
        public int PaidSpinCost = 500;
        [Range(2f, 8f)] public float SpinDuration = 4f;
        public int FullTurns = 5;

        [Header("Angle / Pointer")]
        public float pointerAngle = 90f;
        public bool SpinClockwise = false;

        [Header("Free Spin Cooldown")]
        [Range(0, 48)] public int CooldownHours = 24;
        [Range(0, 59)] public int CooldownMinutes = 0;
        [Range(0, 59)] public int CooldownSeconds = 0;

        [Header("Sectors (any order)")]
        public SpinSector[] Sectors;

        [Header("Options")]
        public bool FreeSpinEnabled = true;
        public bool AllowPaidWhenNoFree = true;

        [Serializable] public class RewardEvent : UnityEvent<string, int> { }
        public RewardEvent OnRewardGranted;
        public event Action<RewardType, int> RewardGranted;
        public UnityEvent<int> OnCoinsGranted;
        public UnityEvent<int> OnHolderGranted;
        public UnityEvent<int> OnFrozenGranted;
        public UnityEvent<int> OnCleanerGranted;
        public UnityEvent<int> OnHammerGranted;

        const string PREF_LAST_FREE_TICKS = "DS_LASTFREE_TICKS";

        bool _isSpinning;
        bool _freeAvailable;
        DateTime _nextFree;

        float _startAngle;
        float _targetAngle;
        float _t;

        SpinSector _picked;
        
        private AudioSource _spinningAudioSource;
        private Coroutine _audioStopCoroutine;

        Coroutine _popupCoroutine;
        Coroutine _panelShowCoroutine;
        Coroutine _iconIdleCoroutine;
        bool _createdShadowAtRuntime = false;
        TextMeshProUGUI _runtimeShadowRef = null;

        Vector3 _iconStartLocalPos;
        Vector3 _iconStartLocalScale;
        Quaternion _iconStartLocalRot;

        void Awake()
        {
            WireUI(true);
            SafeAreaHandler.RegisterRectTransform(safeAreaTransform);

            if (Sectors != null)
            {
                foreach (var s in Sectors)
                    if (s != null && s.ValueText) s.ValueText.text = s.Amount.ToString();
            }

            BootstrapFreeTimer();
            RefreshPaidBadge();

            if (Wheel != null) _startAngle = Wheel.localEulerAngles.z;

            if (RewardPopupRoot) RewardPopupRoot.SetActive(false);
            if (RewardPopupRect) RewardPopupRect.localScale = Vector3.one * PopupScaleFrom;
            if (RewardPopupCloseButton != null)
            {
                RewardPopupCloseButton.onClick.RemoveAllListeners();
                RewardPopupCloseButton.onClick.AddListener(HideRewardPopup);
            }
            rewardParticleSystem.gameObject.SetActive(false);

            if (RewardIconImage != null)
            {
                _iconStartLocalPos = RewardIconImage.rectTransform.localPosition;
                _iconStartLocalScale = RewardIconImage.rectTransform.localScale;
                _iconStartLocalRot = RewardIconImage.rectTransform.localRotation;
            }
            
            SetupSpinningAudioSource();
        }
        
        void SetupSpinningAudioSource()
        {
            _spinningAudioSource = gameObject.AddComponent<AudioSource>();
            _spinningAudioSource.clip = wheelSpinningSound;
            _spinningAudioSource.loop = true;
            _spinningAudioSource.volume = 0.7f; 
            _spinningAudioSource.playOnAwake = false;
            _spinningAudioSource.priority = 128; 
        }

        public override void PlayShowAnimation()
        {
            if (WheelMain == null) return;
            Vector2 startPos = WheelMain.anchoredPosition;
            Vector2 offscreen = new Vector2(startPos.x, PANEL_BOTTOM_OFFSET_Y);
            WheelMain.anchoredPosition = offscreen;

            if (_panelShowCoroutine != null) StopCoroutine(_panelShowCoroutine);
            _panelShowCoroutine = StartCoroutine(PanelSlideSequence(WheelMain, new Vector2(startPos.x, startedStorePanelRectPositionY + 100f), new Vector2(startPos.x, startedStorePanelRectPositionY), 0.4f, 0.2f));
        }

        public override void PlayHideAnimation()
        {
            if (WheelMain == null) return;
            Vector2 startPos = WheelMain.anchoredPosition;
            Vector2 upPos = new Vector2(startPos.x, startedStorePanelRectPositionY + 100f);
            Vector2 bottom = new Vector2(startPos.x, PANEL_BOTTOM_OFFSET_Y);

            if (_panelShowCoroutine != null) StopCoroutine(_panelShowCoroutine);
            _panelShowCoroutine = StartCoroutine(PanelSlideSequence(WheelMain, upPos, bottom, 0.2f, 0.4f, onComplete: () => ScreenManager.OnPageClosed(this)));
        }

        IEnumerator PanelSlideSequence(RectTransform rt, Vector2 firstTarget, Vector2 settleTarget, float firstDur, float settleDur, Action onComplete = null)
        {
            yield return StartCoroutine(AnimateAnchoredPosition(rt, firstTarget, firstDur, EaseType.OutSine));
            yield return StartCoroutine(AnimateAnchoredPosition(rt, settleTarget, settleDur, EaseType.InOutSine));
            onComplete?.Invoke();
            _panelShowCoroutine = null;
        }

        enum EaseType { Linear, OutBack, InBack, OutSine, InOutSine, OutBounce, OutCubic }
        float EaseFunc(EaseType e, float t)
        {
            switch (e)
            {
                case EaseType.OutBack:
                    {
                        float s = 1.70158f;
                        t = t - 1f;
                        return (t * t * ((s + 1f) * t + s) + 1f);
                    }
                case EaseType.InBack:
                    {
                        float s = 1.70158f;
                        return t * t * ((s + 1f) * t - s);
                    }
                case EaseType.OutSine:
                    return Mathf.Sin(t * Mathf.PI * 0.5f);
                case EaseType.InOutSine:
                    return -0.5f * (Mathf.Cos(Mathf.PI * t) - 1f);
                case EaseType.OutBounce:
                    {
                        if (t < (1f / 2.75f)) return 7.5625f * t * t;
                        else if (t < (2f / 2.75f)) { t -= (1.5f / 2.75f); return 7.5625f * t * t + 0.75f; }
                        else if (t < (2.5f / 2.75f)) { t -= (2.25f / 2.75f); return 7.5625f * t * t + 0.9375f; }
                        else { t -= (2.625f / 2.75f); return 7.5625f * t * t + 0.984375f; }
                    }
                case EaseType.OutCubic:
                    t -= 1f; return t * t * t + 1f;
                default:
                    return t;
            }
        }

        IEnumerator AnimateAnchoredPosition(RectTransform rt, Vector2 target, float duration, EaseType ease)
        {
            if (rt == null) yield break;
            Vector2 from = rt.anchoredPosition;
            if (Mathf.Approximately(duration, 0f)) { rt.anchoredPosition = target; yield break; }
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float u = Mathf.Clamp01(elapsed / duration);
                float eu = EaseFunc(ease, u);
                rt.anchoredPosition = Vector2.LerpUnclamped(from, target, eu);
                yield return null;
            }
            rt.anchoredPosition = target;
        }

        public override void Init() { }
        public override void PlayShowAnimationMainReturn() { }

        void OnEnable()
        {
            SetInteractable(true);
            if (Wheel != null) _startAngle = Wheel.localEulerAngles.z;
            UpdateButtonVisibility();
        }

        void OnDisable()
        {
            WireUI(false);
            StopSpinningAudio(); 
        }
        
        void OnDestroy()
        {
            StopSpinningAudio();
        }

        void WireUI(bool on)
        {
            if (SpinButton != null)
            {
                SpinButton.onClick.RemoveAllListeners();
                if (on) SpinButton.onClick.AddListener(OnSpinClicked);
            }
            if (CloseButton != null)
            {
                CloseButton.onClick.RemoveAllListeners();
                if (on) CloseButton.onClick.AddListener(CloseOnce);
            }
            
            if (FreeSpinButton != null)
            {
                FreeSpinButton.onClick.RemoveAllListeners();
                if (on) FreeSpinButton.onClick.AddListener(OnFreeSpinClicked);
            }
            if (PaidSpeedButton != null)
            {
                PaidSpeedButton.onClick.RemoveAllListeners();
                if (on) PaidSpeedButton.onClick.AddListener(OnPaidSpeedClicked);
            }
            if (AdsButton != null)
            {
                AdsButton.onClick.RemoveAllListeners();
                if (on) AdsButton.onClick.AddListener(OnAdsClicked);
            }
        }

        void Update()
        {
            if (FreeSpinEnabled) 
            {
                UpdateFreeLabel();
                UpdateButtonVisibility(); 
            }

            if (_isSpinning && Wheel)
            {
                _t += Time.deltaTime;
                float u = Mathf.Clamp01(_t / SpinDuration);
                u = u * u * u * (u * (6f * u - 15f) + 10f);
                float ang = Mathf.Lerp(_startAngle, _targetAngle, u);
                Wheel.localEulerAngles = new Vector3(0, 0, ang);

                if (_t >= SpinDuration)
                {
                    _isSpinning = false;
                    _startAngle = _targetAngle % 360f;
                    StopSpinningAudio(); 
                    SetInteractable(true);
                    UpdateButtonVisibility(); 
                    FinishReward(); 
                }
            }
        }

        void CloseOnce()
        {
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            ScreenManager.CloseScreen<CrystalUIDailySpin>();
        }

        void OnSpinClicked()
        {
            if (_isSpinning) return;
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_MEDIUM);
#endif
            if (_freeAvailable) StartSpin(true);
            else
            {
                if (!AllowPaidWhenNoFree) return;
                if (PaidSpin(PaidSpinCost)) StartSpin(false);
            }
        }
        
        void OnFreeSpinClicked()
        {
            if (_isSpinning) return;
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_MEDIUM);
#endif
            if (_freeAvailable) 
            {
                StartSpin(true);
                UpdateButtonVisibility();
            }
        }
        
        void OnPaidSpeedClicked()
        {
            if (_isSpinning) return;
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_MEDIUM);
#endif
            if (PaidSpin(PaidSpinCost)) 
            {
                StartSpin(false);
            }
        }
        
        void OnAdsClicked()
        {
            if (_isSpinning) return;
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_MEDIUM);
#endif
            
            ShowAdAndSpin();
        }
        
        void UpdateButtonVisibility()
        {
            bool freeAvailable = _freeAvailable;
            
            if (FreeSpinButton != null)
                FreeSpinButton.gameObject.SetActive(freeAvailable);
                
            if (PaidSpeedButton != null)
                PaidSpeedButton.gameObject.SetActive(!freeAvailable);
                
            if (AdsButton != null)
                AdsButton.gameObject.SetActive(!freeAvailable);
                
            if (PaidSpeedCostText != null)
                PaidSpeedCostText.text = PaidSpinCost.ToString();
                
            if (AdsButtonText != null)
                AdsButtonText.text = "Watch Ad";
        }
        
        void ShowAdAndSpin()
        {
            MyAdsAdapter.ShowRewardBasedVideo((success) =>
            {
                if (success)
                {
                    StartSpin(false); 
                }
                else
                {
                    if (RewardMessageText) 
                        RewardMessageText.text = "<color=red>Ad not available!</color>";
                }
            });
        }

        private bool PaidSpin(int amount)
        {
            int currentCoins = EconomyManager.Get(CurrencyType.Coins);
            if (currentCoins < amount)
            {
                if (RewardMessageText) RewardMessageText.text = "<color=red>Balance Low!</color>";
                return false;
            }

            EconomyManager.Substract(CurrencyType.Coins, amount);
            return true;
        }

        void StartSpin(bool isFree)
        {
            if (!Wheel || Sectors == null || Sectors.Length == 0) return;
            
            UpdateButtonVisibility();

            int total = Sectors.Sum(s => Mathf.Max(0, s.Probability));
            if (total <= 0) total = 1;
            int roll = UnityEngine.Random.Range(1, total + 1);

            int acc = 0, index = 0;
            for (int i = 0; i < Sectors.Length; i++)
            {
                acc += Mathf.Max(0, Sectors[i].Probability);
                if (roll <= acc) { index = i; break; }
            }
            _picked = Sectors[index];

            if (_picked.ValueText == null)
            {
                Debug.LogWarning("ValueText missing on sector " + index);
                return;
            }

            RectTransform txtRt = _picked.ValueText.rectTransform;
            Vector3 worldCenter = txtRt.TransformPoint(txtRt.rect.center);
            Vector2 local = (Vector2)Wheel.InverseTransformPoint(worldCenter);
            float centerAngle = Mathf.Atan2(local.y, local.x) * Mathf.Rad2Deg;
            float desiredAbs = pointerAngle - centerAngle;
            float sign = SpinClockwise ? -1f : 1f;
            _startAngle = Wheel.localEulerAngles.z;
            float delta = Mathf.DeltaAngle(_startAngle, desiredAbs) * sign;
            _targetAngle = _startAngle + FullTurns * 360f * sign + delta;

            _t = 0f; _isSpinning = true;
            if (RewardMessageText) RewardMessageText.text = "";
            SetInteractable(false);
            
            StartSpinningAudio();

            if (isFree)
            {
                PlayerPrefs.SetString(PREF_LAST_FREE_TICKS, DateTime.Now.Ticks.ToString());
                ScheduleNextFree();
                
                // Schedule notification for when next free spin is available
                CrystalLocalNotificationManager notificationManager = FindObjectOfType<CrystalLocalNotificationManager>();
                if (notificationManager != null)
                {
                    notificationManager.ScheduleFreeSpinNotificationAfterCooldown(CooldownHours, CooldownMinutes, CooldownSeconds);
                    Debug.Log($"[CrystalUIDailySpin] Scheduled free spin notification for {CooldownHours}h {CooldownMinutes}m {CooldownSeconds}s from now");
                }
            }
        }

        void FinishReward()
        {
            if (_picked == null) return;
            ShowRewardPopup(_picked.Type, _picked.Amount);
            rewardParticleSystem.gameObject.SetActive(true);

            ApplyRewardSwitch(_picked.Type, _picked.Amount);
            OnRewardGranted?.Invoke(_picked.Type.ToString(), _picked.Amount);
            RewardGranted?.Invoke(_picked.Type, _picked.Amount);
            InvokePerTypeEvents(_picked.Type, _picked.Amount);
            RefreshPaidBadge();
        }

        void ShowRewardPopup(RewardType type, int amount)
        {
            if (RewardPopupRoot == null || RewardPopupRect == null || RewardPopupText == null || RewardIconImage == null)
            {
                if (RewardMessageText)
                    RewardMessageText.text = $"You Got <color=yellow>{amount}</color> {Pretty(type)}";
                return;
            }

            RewardPopupRoot.SetActive(true);

            RewardPopupText.text = $"Congratulation You Got {amount} {Pretty(type)}";
            var vg = new VertexGradient(TmpGradientTop, TmpGradientTop, TmpGradientBottom, TmpGradientBottom);
            RewardPopupText.colorGradient = vg;
            RewardPopupText.ForceMeshUpdate();

            try
            {
                var mat = RewardPopupText.fontMaterial;
                if (mat.HasProperty("_OutlineColor")) mat.SetColor("_OutlineColor", PopupTextOutlineColor);
                if (mat.HasProperty("_OutlineWidth")) mat.SetFloat("_OutlineWidth", PopupTextOutlineWidth);
            }
            catch { }

            if (RewardPopupTextShadow != null)
            {
                RewardPopupTextShadow.text = RewardPopupText.text;
                RewardPopupTextShadow.color = PopupShadowColor;
                RewardPopupTextShadow.ForceMeshUpdate();
                _createdShadowAtRuntime = false;
                _runtimeShadowRef = null;
            }
            else
            {
                if (!_createdShadowAtRuntime) CreateRuntimeShadowText();
                if (_runtimeShadowRef != null)
                {
                    _runtimeShadowRef.text = RewardPopupText.text;
                    _runtimeShadowRef.color = PopupShadowColor;
                    _runtimeShadowRef.ForceMeshUpdate();
                }
            }

            Sprite sp = GetSpriteForType(type);
            if (sp != null)
            {
                RewardIconImage.sprite = sp;
                RewardIconImage.SetNativeSize();
            }

            RewardPopupRect.localScale = Vector3.one * PopupScaleFrom;
            RewardIconImage.rectTransform.localPosition = _iconStartLocalPos;
            RewardIconImage.rectTransform.localScale = _iconStartLocalScale;
            RewardIconImage.rectTransform.localRotation = _iconStartLocalRot;

            if (_popupCoroutine != null) StopCoroutine(_popupCoroutine);
            _popupCoroutine = StartCoroutine(PopupOpenCoroutine());
        }

        IEnumerator PopupOpenCoroutine()
        {
            float dur = Mathf.Max(0.0001f, PopupOpenDuration);
            float elapsed = 0f;

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float u = Mathf.Clamp01(elapsed / dur);

                float scale;
                if (u < 0.7f)
                {
                    float uu = u / 0.7f;
                    scale = Mathf.Lerp(PopupScaleFrom, PopupScaleOvershoot, EaseFunc(EaseType.OutBack, uu));
                }
                else
                {
                    float uu = (u - 0.7f) / 0.3f;
                    scale = Mathf.Lerp(PopupScaleOvershoot, 1f, EaseFunc(EaseType.InBack, uu));
                }

                RewardPopupRect.localScale = Vector3.one * scale;

                float iconScale = 1f + Mathf.Sin(u * Mathf.PI) * 0.12f;
                RewardIconImage.rectTransform.localScale = _iconStartLocalScale * iconScale;

                yield return null;
            }

            RewardPopupRect.localScale = Vector3.one;
            RewardIconImage.rectTransform.localScale = _iconStartLocalScale;

            yield return StartCoroutine(IconPunch(RewardIconImage.rectTransform, PopupIconPunch, 0.12f));

            if (IconIdleEnabled)
            {
                if (_iconIdleCoroutine != null) StopCoroutine(_iconIdleCoroutine);
                _iconIdleCoroutine = StartCoroutine(IconPulseRoutine());
            }

            _popupCoroutine = null;
        }

        IEnumerator IconPunch(RectTransform t, float magnitude, float time)
        {
            float elapsed = 0f;
            Vector3 orig = t.localScale;
            while (elapsed < time)
            {
                elapsed += Time.deltaTime;
                float u = elapsed / time;
                float s = 1f + Mathf.Sin(u * Mathf.PI) * (magnitude * 3f);
                t.localScale = orig * s;
                yield return null;
            }
            t.localScale = orig;
        }

        IEnumerator IconPulseRoutine()
        {
            if (RewardIconImage == null) yield break;
            RectTransform rt = RewardIconImage.rectTransform;
            Vector3 baseScale = _iconStartLocalScale; 
            float t = 0f;
            while (true)
            {
                t += Time.deltaTime * IconPulseSpeed;
                float s = (Mathf.Sin(t * Mathf.PI * 2f) * 0.5f) + 0.5f;
                float cur = Mathf.Lerp(IconPulseMin, IconPulseMax, s);
                rt.localScale = baseScale * cur;
                yield return null;
            }
        }


        void HideRewardPopup()
        {
            if (RewardPopupRoot == null) return;

            if (_popupCoroutine != null) StopCoroutine(_popupCoroutine);
            _popupCoroutine = StartCoroutine(PopupCloseCoroutine());

            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
        }

        IEnumerator PopupCloseCoroutine()
        {
            if (_iconIdleCoroutine != null) { StopCoroutine(_iconIdleCoroutine); _iconIdleCoroutine = null; }

            float dur = Mathf.Max(0.0001f, PopupCloseDuration);
            float elapsed = 0f;
            Vector3 startScale = RewardPopupRect.localScale;
            Vector3 startIconScale = RewardIconImage.rectTransform.localScale;
            Vector3 startIconPos = RewardIconImage.rectTransform.localPosition;
            Quaternion startIconRot = RewardIconImage.rectTransform.localRotation;

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float u = Mathf.Clamp01(elapsed / dur);
                float eu = EaseFunc(EaseType.InBack, u);

                RewardPopupRect.localScale = Vector3.Lerp(startScale, Vector3.one * (PopupScaleFrom * 0.9f), eu);
                RewardIconImage.rectTransform.localScale = Vector3.Lerp(startIconScale, _iconStartLocalScale, eu);
                RewardIconImage.rectTransform.localPosition = Vector3.Lerp(startIconPos, _iconStartLocalPos, eu);
                RewardIconImage.rectTransform.localRotation = Quaternion.Slerp(startIconRot, _iconStartLocalRot, eu);

                yield return null;
            }

            RewardPopupRect.localScale = Vector3.one * (PopupScaleFrom * 0.9f);
            try { RewardPopupRoot.SetActive(false); } catch { }

            RewardIconImage.rectTransform.localScale = _iconStartLocalScale;
            RewardIconImage.rectTransform.localPosition = _iconStartLocalPos;
            RewardIconImage.rectTransform.localRotation = _iconStartLocalRot;

            _popupCoroutine = null;
        }

        void CreateRuntimeShadowText()
        {
            if (RewardPopupText == null) return;
            var parent = RewardPopupText.transform.parent;
            if (parent == null) return;

            GameObject go = new GameObject("PopupTextShadow");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            var mainRt = RewardPopupText.rectTransform;
            rt.anchorMin = mainRt.anchorMin; rt.anchorMax = mainRt.anchorMax;
            rt.pivot = mainRt.pivot;
            rt.anchoredPosition = mainRt.anchoredPosition;
            rt.sizeDelta = mainRt.sizeDelta;
            rt.localScale = Vector3.one;

            var shadow = go.AddComponent<TextMeshProUGUI>();
            shadow.font = RewardPopupText.font;
            shadow.fontSize = RewardPopupText.fontSize;
            shadow.alignment = RewardPopupText.alignment;
            shadow.enableWordWrapping = RewardPopupText.enableWordWrapping;
            shadow.raycastTarget = false;
            shadow.text = RewardPopupText.text;
            shadow.color = PopupShadowColor;
            shadow.ForceMeshUpdate();

            int idx = RewardPopupText.transform.GetSiblingIndex();
            go.transform.SetSiblingIndex(Math.Max(0, idx));

            _createdShadowAtRuntime = true;
            _runtimeShadowRef = shadow;
        }

        Sprite GetSpriteForType(RewardType type)
        {
            switch (type)
            {
                case RewardType.Coin: return CoinSprite;
                case RewardType.Holder: return HolderSprite;
                case RewardType.Frozen: return FrozenSprite;
                case RewardType.Cleaner: return CleanerSprite;
                case RewardType.Hammer: return HammerSprite;
                default: return null;
            }
        }

        void ApplyRewardSwitch(RewardType type, int amount)
        {
            switch (type)
            {
                case RewardType.Coin: EconomyManager.Add(CurrencyType.Coins, amount); break;
                case RewardType.Holder: CrystalPUController.AddPowerUp(CrystalPUType.AddExtraHole, amount); break;
                case RewardType.Frozen: CrystalPUController.AddPowerUp(CrystalPUType.Timer, amount); break;
                case RewardType.Cleaner: CrystalPUController.AddPowerUp(CrystalPUType.RemoveScrew, amount); break;
                case RewardType.Hammer: CrystalPUController.AddPowerUp(CrystalPUType.DestroyPlank, amount); break;
                default: Debug.LogWarning($"Unhandled reward type: {type} x{amount}"); break;
            }
        }

        void InvokePerTypeEvents(RewardType type, int amount)
        {
            switch (type)
            {
                case RewardType.Coin: OnCoinsGranted?.Invoke(amount); break;
                case RewardType.Holder: OnHolderGranted?.Invoke(amount); break;
                case RewardType.Frozen: OnFrozenGranted?.Invoke(amount); break;
                case RewardType.Cleaner: OnCleanerGranted?.Invoke(amount); break;
                case RewardType.Hammer: OnHammerGranted?.Invoke(amount); break;
            }
        }

        string Pretty(RewardType t)
        {
            switch (t)
            {
                case RewardType.Coin: return "Coins";
                case RewardType.Holder: return "Holder";
                case RewardType.Frozen: return "Frozen";
                case RewardType.Cleaner: return "Cleaner";
                case RewardType.Hammer: return "Hammer";
                default: return t.ToString();
            }
        }

        void SetInteractable(bool v)
        {
            if (SpinButton) SpinButton.interactable = v;
            if (CloseButton) CloseButton.interactable = v;
            
            if (FreeSpinButton) FreeSpinButton.interactable = v;
            if (PaidSpeedButton) PaidSpeedButton.interactable = v;
            if (AdsButton) AdsButton.interactable = v;
        }

        void BootstrapFreeTimer()
        {
            if (!FreeSpinEnabled) { _freeAvailable = false; if (NextFreeLabel) NextFreeLabel.text = ""; return; }

            if (!PlayerPrefs.HasKey(PREF_LAST_FREE_TICKS))
            {
                _freeAvailable = true;
                if (NextFreeLabel) NextFreeLabel.text = "YOU CAN SPIN NOW - TRY YOUR LUCK !";
            }
            else ScheduleNextFree();
            
            UpdateButtonVisibility();
        }

        void ScheduleNextFree()
        {
            long ticks = Convert.ToInt64(PlayerPrefs.GetString(PREF_LAST_FREE_TICKS, DateTime.Now.Ticks.ToString()));
            _nextFree = new DateTime(ticks).AddHours(CooldownHours).AddMinutes(CooldownMinutes).AddSeconds(CooldownSeconds);
            _freeAvailable = false;
            RefreshPaidBadge();
            
            UpdateButtonVisibility();
        }

        void UpdateFreeLabel()
        {
            if (_freeAvailable) { if (NextFreeLabel) NextFreeLabel.text = "YOU CAN SPIN NOW - TRY YOUR LUCK !"; return; }

            var d = _nextFree - DateTime.Now;
            if (d.TotalSeconds <= 0) 
            { 
                _freeAvailable = true; 
                if (NextFreeLabel) NextFreeLabel.text = "YOU CAN SPIN NOW - TRY YOUR LUCK !"; 
                RefreshPaidBadge();
            }
            else if (NextFreeLabel) { int h = Mathf.FloorToInt((float)d.TotalHours); NextFreeLabel.text = $"REST THE WHEEL ! NEXT LUCKY SPIN IN : {h:00}:{d.Minutes:00}:{d.Seconds:00}"; }
        }


        void RefreshPaidBadge()
        {
            if (PaidCostBadgeRoot) PaidCostBadgeRoot.SetActive(!_freeAvailable);
            if (PaidCostText) PaidCostText.text = PaidSpinCost.ToString();
        }
        
        void StartSpinningAudio()
        {
            if (_spinningAudioSource != null && wheelSpinningSound != null && SoundManager.IsAudioTypeActive(AudioType.Sound))
            {
                _spinningAudioSource.Play();
                
                if (_audioStopCoroutine != null)
                    StopCoroutine(_audioStopCoroutine);
                _audioStopCoroutine = StartCoroutine(StopAudioAfterDelay(4f));
            }
        }
        
        void StopSpinningAudio()
        {
            if (_audioStopCoroutine != null)
            {
                StopCoroutine(_audioStopCoroutine);
                _audioStopCoroutine = null;
            }
            
            if (_spinningAudioSource != null && _spinningAudioSource.isPlaying)
            {
                _spinningAudioSource.Stop();
                _spinningAudioSource.time = 0f;
            }
        }
        
        IEnumerator StopAudioAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (_spinningAudioSource != null && _spinningAudioSource.isPlaying)
            {
                _spinningAudioSource.Stop();
                _spinningAudioSource.time = 0f;
            }
            
            _audioStopCoroutine = null;
        }

        [Serializable]
        public class SpinSector
        {
            public Text ValueText;
            public int Amount = 100;
            [Range(0, 100)] public int Probability = 10;
            public RewardType Type = RewardType.Coin;
        }

        public enum RewardType { Coin, Holder, Frozen, Cleaner, Hammer }
    }
}

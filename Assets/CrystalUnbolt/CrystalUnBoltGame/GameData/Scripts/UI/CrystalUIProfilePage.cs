using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace CrystalUnbolt
{
    public class CrystalUIProfilePage : BaseScreen
    {
        [Header("Services")]
        [SerializeField] private CrystalLoginAuthManager auth;
        [SerializeField] private CrystalPlayerDataService dataService;
        [SerializeField] private CrystalUIMainMenu header;   

        [Header("Main Panel")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Button closeButton;

        [Header("Display (view mode)")]
        [SerializeField] private Image avatarImage;         
        [SerializeField] private TextMeshProUGUI nameText;  

        [Header("Edit (edit mode)")]
        [SerializeField] private TMP_InputField nameInput;  
        [SerializeField] private GameObject inputContainer; 
        [SerializeField] private Button editButton;
        [SerializeField] private Button saveButton;

        [Header("Avatar Grid")]
        [SerializeField] private Sprite[] avatarSprites;    
        [SerializeField] private Button[] avatarButtons;    
        [SerializeField] private Image[] selectionFrames;   
        [SerializeField] private Image[] profileIcon;       

        [Header("Animation")]
        [SerializeField] private Transform[] popTargets;
        [SerializeField] private float showDuration = 0.25f;

        private AnimCase[] appearTweenCases;
        private int selectedAvatarId = 0;
        private PlayerData _profile;
        private Coroutine _photoLoadRoutine;
        private string _currentPhotoUrl;
        private bool _isGuest = false;
        private const string GUEST_NAME_KEY = "profile_guest_name";
        private const string GUEST_AVATAR_KEY = "profile_guest_avatar";

        private bool _wired = false;
        private bool _isRefreshing = false;

        [BoxGroup("Scroll View", "Scroll View")]
        [SerializeField] RectTransform profileAnimatedPanelRect;
        private static float startedStorePanelRectPositionY;
        private readonly float PANEL_BOTTOM_OFFSET_Y = -2000f;

        public override void Init()
        {
            Debug.Log("Init");

            WireOnce();                 
            StartCoroutine(RefreshAndApply()); 
        }

        private int _refreshToken = 0;


        private void Start()
        {
            Debug.Log("ONENBLE");
        }

        private void WireOnce()
        {
            if (_wired) return;
            _wired = true;

            if (closeButton) closeButton.onClick.AddListener(OnClose);
            if (editButton) editButton.onClick.AddListener(EnterEditMode);
            if (saveButton) saveButton.onClick.AddListener(OnSave);

            if (avatarButtons != null)
            {
                for (int i = 0; i < avatarButtons.Length; i++)
                {
                    int id = i;
                    if (avatarButtons[i] != null)
                        avatarButtons[i].onClick.AddListener(() => SelectAvatar(id));
                }
            }

            SetupGridSprites();
            ConfigureRaycasts();
        }

        private void SetupGridSprites()
        {
            if (profileIcon == null || avatarSprites == null) return;
            int n = Mathf.Min(profileIcon.Length, avatarSprites.Length);
            for (int i = 0; i < n; i++)
            {
                if (profileIcon[i]) profileIcon[i].sprite = avatarSprites[i];
                profileIcon[i].SetNativeSize();

            }
        }

        private void ConfigureRaycasts()
        {
            if (avatarImage) avatarImage.raycastTarget = false;
            if (nameText) nameText.raycastTarget = false;

            if (selectionFrames != null)
                foreach (var r in selectionFrames) if (r) r.raycastTarget = false;

            if (profileIcon != null)
                foreach (var ic in profileIcon) if (ic) ic.raycastTarget = false;

            if (avatarButtons != null)
                foreach (var b in avatarButtons)
                    if (b)
                    {
                        b.interactable = true;
                        var img = b.GetComponent<Image>();
                        if (img) img.raycastTarget = true; 
                    }
        }

        public override void PlayShowAnimation()
        {
            if (panel) panel.transform.localScale = Vector3.one;

            if (appearTweenCases != null)
                foreach (var tc in appearTweenCases) tc?.Kill();

            if (popTargets != null && popTargets.Length > 0)
            {
                appearTweenCases = new AnimCase[popTargets.Length];
                for (int i = 0; i < popTargets.Length; i++)
                {
                    var t = popTargets[i];
                    t.localScale = Vector3.zero;
                    appearTweenCases[i] = t
                        .DOScale(1f, showDuration, i * 0.03f)
                        .SetEasing(Ease.Type.CircOut);
                }
                appearTweenCases[^1].OnComplete(() => ScreenManager.OnPageOpened(this));
            }
            else
            {
                profileAnimatedPanelRect.anchoredPosition = profileAnimatedPanelRect.anchoredPosition.SetY(PANEL_BOTTOM_OFFSET_Y);

                profileAnimatedPanelRect.DOAnchoredPosition(new Vector3(profileAnimatedPanelRect.anchoredPosition.x, startedStorePanelRectPositionY + 100f, 0f), 0.4f).SetEasing(Ease.Type.SineInOut).OnComplete(delegate
                {
                    profileAnimatedPanelRect.DOAnchoredPosition(new Vector3(profileAnimatedPanelRect.anchoredPosition.x, startedStorePanelRectPositionY, 0f), 0.2f).SetEasing(Ease.Type.SineInOut).OnComplete(() =>
                    {
                        ScreenManager.OnPageOpened(this);
                    });
                });
            }


        }

        public override void PlayHideAnimation() => ScreenManager.OnPageClosed(this);
        public override void PlayShowAnimationMainReturn()
        {
            if (panel) panel.transform.localScale = Vector3.one;
        }
        public void RefreshNow()
        {
            saveButton.gameObject.SetActive(true);
            if (!_wired) WireOnce();

            _refreshToken++;

            StartCoroutine(RefreshAndApply());
        }

        public IEnumerator RefreshAndApply()
        {
            if (_isRefreshing) yield break;
            _isRefreshing = true;
            int myToken = ++_refreshToken;

            var u = auth?.CurrentUser;
            if (u == null)
            {
                _isGuest = true;

                var p = new PlayerData
                {
                    uid = "guest",
                    name = PlayerPrefs.GetString(GUEST_NAME_KEY, "Guest"),
                    email = "",
                    photoUrl = "",
                    avatarId = Mathf.Clamp(
                        PlayerPrefs.GetInt(GUEST_AVATAR_KEY, 0),
                        0,
                        (avatarSprites != null && avatarSprites.Length > 0 ? avatarSprites.Length - 1 : 0)
                    ),
                    level = 1,
                    coins = 0
                };

                if (myToken != _refreshToken) { _isRefreshing = false; yield break; } 
                _profile = p;
                ApplyProfileToUI(_profile);   
                EnterViewMode();              
                _isRefreshing = false;
                yield break;
            }

            _isGuest = false;

            var loadTask = dataService.LoadByUid(u.UserId);
            while (!loadTask.IsCompleted) yield return null;

            var loaded = loadTask.Result;
            if (loaded == null)
            {
                var ensureTask = dataService.EnsureProfile(
                    u,
                    u.DisplayName ?? "Player",
                    u.Email ?? "",
                    (u.PhotoUrl != null ? u.PhotoUrl.ToString() : "")
                );
                while (!ensureTask.IsCompleted) yield return null;
                loaded = ensureTask.Result;

                if (!string.IsNullOrWhiteSpace(loaded.photoUrl) && loaded.avatarId >= 0)
                {
                    loaded.avatarId = -1;
                    var partial0 = new System.Collections.Generic.Dictionary<string, object>
                    {
                        ["avatarId"] = -1,
                        ["updatedAt"] = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    };
                    var upd0 = dataService.UpdateFields(u.UserId, partial0);
                    while (!upd0.IsCompleted) yield return null;
                }
            }

            if (loaded != null && loaded.avatarId >= 0 && !string.IsNullOrWhiteSpace(loaded.photoUrl))
            {
                loaded.photoUrl = "";
                var partial1 = new System.Collections.Generic.Dictionary<string, object>
                {
                    ["photoUrl"] = "",
                    ["updatedAt"] = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };
                var upd1 = dataService.UpdateFields(u.UserId, partial1);
                while (!upd1.IsCompleted) yield return null;
            }

            if (myToken != _refreshToken) { _isRefreshing = false; yield break; } 
            _profile = loaded;
            ApplyProfileToUI(_profile);
            EnterViewMode();
            _isRefreshing = false;
        }



        private void ApplyProfileToUI(PlayerData p)
        {
            if (p == null) return;

            _profile = p; 

            string uiName = string.IsNullOrEmpty(p.name) ? "Guest" : p.name;
            if (nameText) nameText.text = uiName;
            if (nameInput) nameInput.text = uiName;

            if (p.avatarId >= 0) p.photoUrl = "";

            if (!string.IsNullOrWhiteSpace(p.photoUrl))
            {
                selectedAvatarId = -1;

                if (_photoLoadRoutine != null && _currentPhotoUrl != p.photoUrl)
                {
                    StopCoroutine(_photoLoadRoutine);
                    _photoLoadRoutine = null;
                }
                _currentPhotoUrl = p.photoUrl;
                if (_photoLoadRoutine == null)
                    _photoLoadRoutine = StartCoroutine(LoadPhotoIntoAvatarImage(_currentPhotoUrl));

                RefreshSelectionFrames();

                if (header != null) header.Apply(p);
                if (auth != null) auth.SetProfile(p);
                return;
            }

            if (avatarSprites != null && avatarSprites.Length > 0)
            {
                int max = avatarSprites.Length - 1;
                selectedAvatarId = Mathf.Clamp(p.avatarId, 0, max);
                if (avatarImage) avatarImage.sprite = avatarSprites[selectedAvatarId];
            }

            RefreshSelectionFrames();

            if (header != null) header.Apply(p);
            if (auth != null) auth.SetProfile(p);
        }


        private IEnumerator LoadPhotoIntoAvatarImage(string url)
        {
            if (!(url.StartsWith("http://") || url.StartsWith("https://")))
            {
                _photoLoadRoutine = null;
                yield break;
            }

            using (var req = UnityWebRequestTexture.GetTexture(url))
            {
                yield return req.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
                bool ok = req.result == UnityWebRequest.Result.Success;
#else
        bool ok = !(req.isNetworkError || req.isHttpError);
#endif
                if (!ok)
                {
                    Debug.LogWarning("[ProfilePage] Photo load failed: " + req.error + " url=" + url);
                }
                else
                {
                    var tex = DownloadHandlerTexture.GetContent(req);
                    if (tex != null && avatarImage)
                    {
                        var rect = new Rect(0, 0, tex.width, tex.height);
                        var sprite = Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), 100f);
                        avatarImage.sprite = sprite;
                    }
                }
            }

            _photoLoadRoutine = null;
        }

        private void RefreshSelectionFrames()
        {
            if (selectionFrames == null) return;

            bool photoMode = selectedAvatarId < 0;

            for (int i = 0; i < selectionFrames.Length; i++)
            {
                var ring = selectionFrames[i];
                if (!ring) continue;

                ring.gameObject.SetActive(!photoMode && i == selectedAvatarId);
                ring.raycastTarget = false;
            }
        }


        private void SelectAvatar(int id)
        {
            if (avatarSprites == null || avatarSprites.Length == 0) return;

            if (_profile != null) _profile.photoUrl = "";

            selectedAvatarId = Mathf.Clamp(id, 0, avatarSprites.Length - 1);

            if (avatarImage)
                avatarImage.sprite = avatarSprites[selectedAvatarId];

            RefreshSelectionFrames();
#if MODULE_HAPTIC
           Haptic.Play(Haptic.HAPTIC_HARD);
#endif
        }

        private void EnterEditMode()
        {
            if (nameInput)
            {
                nameInput.interactable = true;
                SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
                nameInput.Select();
                nameInput.ActivateInputField();
                nameInput.MoveTextEnd(false);
            }


            RefreshSelectionFrames();
#if MODULE_HAPTIC
           Haptic.Play(Haptic.HAPTIC_HARD);
#endif
        }

        private void EnterViewMode()
        {
            if (nameInput) nameInput.interactable = false;
            if (inputContainer) inputContainer.SetActive(true);


            RefreshSelectionFrames();
        }

        private async void OnSave()
        {
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
            if (_profile == null) return;
#if MODULE_HAPTIC
           Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            string newName = nameInput ? nameInput.text.Trim() : _profile.name;
            if (string.IsNullOrEmpty(newName)) newName = "Player";

            _profile.name = newName;
            _profile.photoUrl = "";                         
            _profile.avatarId = selectedAvatarId;

            if (_isGuest || auth?.CurrentUser == null)
            {
                PlayerPrefs.SetString(GUEST_NAME_KEY, _profile.name);
                PlayerPrefs.SetInt(GUEST_AVATAR_KEY, _profile.avatarId);
                PlayerPrefs.Save();
                Debug.Log("[Profile] Guest profile saved locally.");

                ApplyProfileToUI(_profile);
                RefreshSelectionFrames();
                EnterViewMode();
                StartCoroutine(WaitWhileSave());
                return;
            }

            var uid = auth.CurrentUser.UserId;
            var partial = new Dictionary<string, object>
            {
                ["name"] = _profile.name,
                ["avatarId"] = _profile.avatarId,
                ["updatedAt"] = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            try
            {
                partial["photoUrl"] = "";                      
                await dataService.UpdateFields(uid, partial);
                Debug.Log("[Profile] Cloud save OK.");
                StartCoroutine(WaitWhileSave());
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[Profile] Cloud save failed: " + ex);
            }

            ApplyProfileToUI(_profile);
            EnterViewMode();
        }

        IEnumerator WaitWhileSave()
        {
            yield return new WaitForSeconds(1f);
            OnClose();
        }
        private void OnClose()
        {
#if MODULE_HAPTIC
           Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
            ScreenManager.CloseScreen<CrystalUIProfilePage>();
        }
    }
}

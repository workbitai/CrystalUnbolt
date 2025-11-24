using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace CrystalUnbolt
{
    [System.Serializable]
    public class FloatingCloud
    {
        [SerializeField] Data[] floatingCloudCases;

        private static Dictionary<int, Data> floatingCloudLink = new Dictionary<int, Data>();
        private static List<Animation> activeClouds = new List<Animation>();

        public void Init()
        {
            for (int i = 0; i < floatingCloudCases.Length; i++)
            {
                RegisterCase(floatingCloudCases[i]);
            }

            Currency[] currencies = EconomyManager.Currencies;
            if(!currencies.IsNullOrEmpty())
            {
                foreach (var currency in currencies)
                {
                    Currency.FloatingCloudCase floatingCloudCase = currency.FloatingCloud;
                    if (floatingCloudCase.AddToCloud)
                    {
                        FloatingCloudSettings floatingCloudSettings;

                        if (floatingCloudCase.SpecialPrefab != null)
                        {
                            floatingCloudSettings = new FloatingCloudSettings(currency.CurrencyType.ToString(), floatingCloudCase.SpecialPrefab);
                        }
                        else
                        {
                            floatingCloudSettings = new FloatingCloudSettings(currency.CurrencyType.ToString(), currency.Icon, new Vector2(100, 100));
                        }

                        floatingCloudSettings.SetAudio(floatingCloudCase.AppearAudioClip, floatingCloudCase.CollectAudioClip);

                        RegisterCase(floatingCloudSettings);
                    }
                }
            }
        }

        public static void Clear()
        {
            Unload();

            foreach (Data floatingCloudData in floatingCloudLink.Values)
            {
                floatingCloudData.Destroy();
            }

            floatingCloudLink.Clear();
        }

        public static void Unload()
        {
            for (int i = 0; i < activeClouds.Count; i++)
            {
                activeClouds[i].Clear();
            }

            activeClouds.Clear();
        }

        public static void RegisterCase(FloatingCloudSettings floatingCloudSettings)
        {
            int cloudHash = floatingCloudSettings.Name.GetHashCode();

            if (floatingCloudLink.ContainsKey(cloudHash))
            {
                Debug.LogError($"Cloud {floatingCloudSettings.Name} already registered!");

                return;
            }

            Data floatingCloudCase = new Data(floatingCloudSettings);
            floatingCloudCase.Init();

            floatingCloudLink.Add(cloudHash, floatingCloudCase);
        }

        public static void RegisterCase(Data floatingCloudCase)
        {
            int cloudHash = floatingCloudCase.Name.GetHashCode();

            if (floatingCloudLink.ContainsKey(cloudHash))
            {
                Debug.LogError($"Cloud {floatingCloudCase.Name} already registered!");

                return;
            }

            floatingCloudCase.Init();

            floatingCloudLink.Add(cloudHash, floatingCloudCase);
        }

        public static void SpawnCurrency(string key, RectTransform rectTransform, RectTransform targetTransform, int elementsAmount, string text, GameCallback onCurrencyHittedTarget = null)
        {
            SpawnCurrency(key.GetHashCode(), rectTransform, targetTransform, elementsAmount, text, onCurrencyHittedTarget);
        }

        public static void SpawnCurrency(int hash, RectTransform rectTransform, RectTransform targetTransform, int elementsAmount, string text, GameCallback onCurrencyHittedTarget = null)
        {
            if (!floatingCloudLink.ContainsKey(hash))
            {
                Debug.LogError($"Cloud with hash {hash} isn't registered!");

                return;
            }

            Animation animation = new Animation(floatingCloudLink[hash], rectTransform, targetTransform, elementsAmount, onCurrencyHittedTarget);
            animation.PlayAnimation();

            activeClouds.Add(animation);
        }

        public static void OnAnimationFinished(Animation animation)
        {
            activeClouds.Remove(animation);
        }

        [System.Serializable]
        public class Data
        {
            [SerializeField] string name;
            public string Name => name;

            [SerializeField] GameObject prefab;
            public GameObject Prefab => prefab;

            [SerializeField] AudioClip appearAudioClip;
            public AudioClip AppearAudioClip => appearAudioClip;

            [SerializeField] AudioClip collectAudioClip;
            public AudioClip CollectAudioClip => collectAudioClip;

            [Space]
            [SerializeField] float cloudRadius;
            public float CloudRadius => cloudRadius;

            private Pool pool;
            public Pool Pool => pool;

            public Data(FloatingCloudSettings settings)
            {
                name = settings.Name;
                prefab = settings.Prefab;

                cloudRadius = settings.CloudRadius;

                appearAudioClip = settings.AppearAudioClip;
                collectAudioClip = settings.CollectAudioClip;
            }

            public void Init()
            {
                pool = new Pool(prefab, "FloatingCloud_" + name);
            }

            public void Destroy()
            {
                ObjectPoolManager.DestroyPool(pool);

                pool = null;
            }
        }

        public class Animation
        {
            private Data floatingCloudData;
            private RectTransform rectTransform;
            private RectTransform targetTransform;
            private int elementsAmount;
            private List<RectTransform> elementsList = new List<RectTransform>();
            private GameCallback onCurrencyHittedTarget;

            private Transform fakeTargetTransform;

            private TweenCaseCollection tweenCaseCollection;

            public Animation(Data floatingCloudData, RectTransform rectTransform, RectTransform targetTransform, int elementsAmount, GameCallback onCurrencyHittedTarget)
            {
                this.floatingCloudData = floatingCloudData;
                this.rectTransform = rectTransform;
                this.targetTransform = targetTransform;
                this.elementsAmount = elementsAmount;
                this.elementsList = new List<RectTransform>();
                this.onCurrencyHittedTarget = onCurrencyHittedTarget;

                GameObject fakeTargetObject = new GameObject("Fake Target");
                fakeTargetTransform = fakeTargetObject.transform;
                fakeTargetTransform.SetParent(targetTransform.parent);
                fakeTargetTransform.position = targetTransform.position;
                fakeTargetTransform.localScale = targetTransform.localScale;
                fakeTargetTransform.localRotation = targetTransform.localRotation;
            }

            public void PlayAnimation()
            {
                RectTransform targetRect = targetTransform;

                tweenCaseCollection = Tween.BeginTweenCaseCollection();

                if (floatingCloudData.AppearAudioClip != null)
                    SoundManager.PlaySound(floatingCloudData.AppearAudioClip);

                float cloudRadius = floatingCloudData.CloudRadius;
                Vector3 centerPoint = rectTransform.position;

                int completed = 0;
                float pitch = 0.9f;
                bool rewardApplied = false;

                float perCoinDelay = 0.12f; // DELAY between coin spawns

                for (int i = 0; i < elementsAmount; i++)
                {
                    int index = i;

                    // Delay BEFORE spawning the coin
                    tweenCaseCollection.AddTween(
                        Tween.DelayedCall(index * perCoinDelay, () =>
                        {
                            AnimCase curveTween = null;

                            // === Spawn Coin ===
                            GameObject obj = floatingCloudData.Pool.GetPooledObject();
                            RectTransform coin = (RectTransform)obj.transform;

                            coin.SetParent(fakeTargetTransform);
                            coin.position = centerPoint;
                            coin.localScale = Vector3.one;
                            coin.localRotation = Quaternion.identity;

                            elementsList.Add(coin);

                            Image img = obj.GetComponent<Image>();
                            img.color = new Color(1, 1, 1, 0);

                            // Fade in
                            tweenCaseCollection.AddTween(
                                img.DOFade(1f, 0.18f, unscaledTime: true)
                            );

                            // Small random outward pop
                            Vector2 burstOffset = UnityEngine.Random.insideUnitCircle * cloudRadius;
                            tweenCaseCollection.AddTween(
                                coin.DOAnchoredPosition(coin.anchoredPosition + burstOffset, 0.35f, unscaledTime: true)
                                .SetEasing(Ease.Type.CubicOut)
                                .OnComplete(() =>
                                {
                                    // === CURVED ARC ANIMATION ===

                                    Vector3 start = coin.localPosition;
                                    Vector3 end = Vector3.zero;

                                    // midpoint for ARC
                                    Vector3 midpoint = (start + end) * 0.5f;
                                    midpoint.y += 120f; // height of curve

                                    float travelTime = 0.55f;
                                    float t = 0f;

                                    curveTween = Tween.DoFloat(0f, 1f, travelTime, (value) =>
                                    {
                                        t = value;

                                        // Quadratic Bezier curve
                                        Vector3 p0 = start;
                                        Vector3 p1 = midpoint;
                                        Vector3 p2 = end;

                                        Vector3 pos =
                                            (1 - t) * (1 - t) * p0 +
                                            2 * (1 - t) * t * p1 +
                                            t * t * p2;

                                        coin.localPosition = pos;

                                    }, unscaledTime: true)
                                    .SetEasing(Ease.Type.SineIn)
                                    .OnComplete(() =>
                                    {
                                        // Apply reward ONCE
                                        if (!rewardApplied)
                                        {
                                            rewardApplied = true;
                                            onCurrencyHittedTarget?.Invoke();
                                        }

                                        // landing sound
                                        if (floatingCloudData.CollectAudioClip != null)
                                            SoundManager.PlaySound(floatingCloudData.CollectAudioClip, pitch);

                                        pitch += 0.015f;

                                        // UI punch
                                        tweenCaseCollection.AddTween(
                                            targetRect.DOScale(1.2f, 0.15f, unscaledTime: true)
                                            .OnComplete(() =>
                                            {
                                                tweenCaseCollection.AddTween(
                                                    targetRect.DOScale(1f, 0.1f, unscaledTime: true)
                                                );
                                            })
                                        );

                                        // return coin to pool
                                        coin.SetParent(floatingCloudData.Pool.ObjectsContainer);
                                        obj.SetActive(false);

                                        completed++;
                                        if (completed >= elementsAmount)
                                        {
                                            FloatingCloud.OnAnimationFinished(this);
                                            GameObject.Destroy(fakeTargetTransform.gameObject);
                                        }
                                    });

                                    tweenCaseCollection.AddTween(curveTween);
                                })
                            );
                        }, unscaledTime: true)
                    );
                }

                Tween.EndTweenCaseCollection();
            }



            public void Clear()
            {
                tweenCaseCollection.Kill();

                if(fakeTargetTransform != null)
                    GameObject.Destroy(fakeTargetTransform.gameObject);
            }
        }
    }
}

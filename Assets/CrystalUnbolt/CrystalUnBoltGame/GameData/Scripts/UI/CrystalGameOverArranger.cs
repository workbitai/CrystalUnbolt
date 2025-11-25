using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace CrystalUnbolt
{
    public class CrystalGameOverArranger : MonoBehaviour
    {
        public static bool IsPuzzleModeActive = false;
        public static int CurrentPuzzleAnswer = -1;
        private static Dictionary<CrystalBaseHole, int> holeDigitIndexMap = new Dictionary<CrystalBaseHole, int>();
        private static int activeBasehole;
        private static int screwIndex = 0;

        [Header("Number Screw Settings")]
        [SerializeField] private float numberScrewScale = 1.2f;   // <- you control this in Inspector

        public static CrystalGameOverArranger instance;

        private Coroutine _periodicShakeRoutine;
        public static float PeriodicShakeInterval = 2f;

        // Controls overall screw/holes arrangement animation speed
        [Header("Animation")]
        [SerializeField] private float arrangementSpeed = 1f;

        public GameObject _wrongAnswerMessageObj;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            ResetState();
        }

        public void ResetState()
        {
            IsPuzzleModeActive = false;
            CurrentPuzzleAnswer = -1;
            holeDigitIndexMap.Clear();
            activeBasehole = 0;

            if (CrystalUIGame.QueText != null && CrystalUIGame.QueText.gameObject != null)
            {
                CrystalUIGame.QueText.gameObject.SetActive(false);
                CrystalUIGame.QueText.text = "";
            }

            StopPeriodicShakeLoop();
            HideWrongAnswerMessage();

            Debug.Log("[Puzzle] CrystalGameOverArranger state RESET");
        }

        static bool AreAnyPuzzleHolesEmpty()
        {
            if (holeDigitIndexMap == null || holeDigitIndexMap.Count == 0) return true;

            foreach (var kv in holeDigitIndexMap)
            {
                if (kv.Key is CrystalHoleController h &&
                    h.gameObject.activeInHierarchy &&
                    h.PlacedNumber < 0)
                    return true;
            }
            return false;
        }

        public static void StartPeriodicShakeLoop()
        {
            if (instance == null) return;
            instance.InternalStartPeriodicShakeLoop();
        }

        public static void StopPeriodicShakeLoop()
        {
            if (instance == null) return;
            instance.InternalStopPeriodicShakeLoop();
        }

        void InternalStartPeriodicShakeLoop()
        {
            InternalStopPeriodicShakeLoop();
            _periodicShakeRoutine = StartCoroutine(PeriodicShakeLoop());
        }

        void InternalStopPeriodicShakeLoop()
        {
            if (_periodicShakeRoutine != null)
            {
                StopCoroutine(_periodicShakeRoutine);
                _periodicShakeRoutine = null;
            }
        }

        IEnumerator PeriodicShakeLoop()
        {
            while (IsPuzzleModeActive && AreAnyPuzzleHolesEmpty())
            {
                var screws = CrystalLevelController.StageLoader != null ? CrystalLevelController.StageLoader.Screws : null;
                if (screws != null)
                {
                    DepthWeightedShake(screws, onlyUnplaced: true, duration: 0.25f, basePos: 0.008f, baseRot: 8f);
                }
                yield return new WaitForSeconds(PeriodicShakeInterval);
            }
            _periodicShakeRoutine = null;
        }

        static void DepthWeightedShake(List<CrystalScrewController> screws, bool onlyUnplaced, float duration, float basePos, float baseRot)
        {
            if (screws == null || screws.Count == 0) return;

            List<CrystalScrewController> list = new List<CrystalScrewController>();
            foreach (var s in screws)
            {
                if (s == null || !s.gameObject.activeInHierarchy) continue;
                if (onlyUnplaced && s.IsPlaced) continue;
                list.Add(s);
            }
            if (list.Count == 0) return;

            float minY = float.MaxValue, maxY = float.MinValue;
            foreach (var s in list)
            {
                float y = s.transform.position.y;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }
            float span = Mathf.Max(0.0001f, maxY - minY);

            foreach (var s in list)
            {
                float y = s.transform.position.y;
                float factor = Mathf.InverseLerp(maxY, minY, y);

                float pos = Mathf.Lerp(basePos * 0.5f, basePos * 1.8f, factor);
                float rot = Mathf.Lerp(baseRot * 0.5f, baseRot * 1.8f, factor);

                var shaker = s.GetComponent<SimpleShaker>();
                if (shaker == null) shaker = s.gameObject.AddComponent<SimpleShaker>();
                shaker.Shake(duration, pos, rot);
            }
        }

        static void VibrateLight()
        {
#if UNITY_IOS && !UNITY_EDITOR
            try
            {
                if (Haptic.IsInitialized && Haptic.IsActive)
                {
                    Haptic.Play(Haptic.HAPTIC_HARD);
                }
                else
                {
                    Handheld.Vibrate();
                }
            }
            catch { }
#elif UNITY_ANDROID && !UNITY_EDITOR
            try { Handheld.Vibrate(); } catch { }
#else
            try { Handheld.Vibrate(); } catch { }
#endif
        }

        public static void ShowWrongAnswerMessage()
        {
            if (instance == null) return;
            instance.InternalShowWrongAnswerMessage();
        }

        public static void HideWrongAnswerMessage()
        {
            if (instance == null) return;
            instance.InternalHideWrongAnswerMessage();
        }

        void InternalShowWrongAnswerMessage()
        {
            if (_wrongAnswerMessageObj == null)
            {
                _wrongAnswerMessageObj = new GameObject("WrongAnswerMessage");
                _wrongAnswerMessageObj.transform.SetParent(transform);

                Canvas canvas = _wrongAnswerMessageObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 1000;

                GameObject textObj = new GameObject("MessageText");
                textObj.transform.SetParent(_wrongAnswerMessageObj.transform);

                var text = textObj.AddComponent<TextMeshProUGUI>();
                text.text = "Fill Right Answer";
                text.fontSize = 48;
                text.color = Color.red;
                text.alignment = TextAlignmentOptions.Center;

                RectTransform rect = textObj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.1f);
                rect.anchorMax = new Vector2(0.5f, 0.1f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(400, 100);
            }

            _wrongAnswerMessageObj.SetActive(true);
            Debug.Log("[Message] Showing 'Fill Right Answer' message");
        }

        void InternalHideWrongAnswerMessage()
        {
            if (_wrongAnswerMessageObj != null)
            {
                _wrongAnswerMessageObj.SetActive(false);
                Debug.Log("[Message] Hiding 'Fill Right Answer' message");
            }
        }

        public static void CheckGameOver()
        {
            Debug.Log("IsPuzzleModeActive  =>  " + IsPuzzleModeActive);

            if (!IsPuzzleModeActive) return;

            bool anyEmpty = false;
            foreach (var pair in holeDigitIndexMap)
            {
                if (pair.Key is CrystalHoleController h && h.IsPuzzleHole && h.PlacedNumber < 0)
                {
                    anyEmpty = true;
                    break;
                }
            }

            if (anyEmpty)
            {
                HideWrongAnswerMessage();
                return;
            }

            List<CrystalBaseHole> baseHoles = CrystalLevelController.StageLoader.BaseHoles;
            List<CrystalScrewController> screws = CrystalLevelController.StageLoader.Screws;

            bool allPlaced = true;
            foreach (var screw in screws)
            {
                if (screw == null) continue;
                if (!screw.gameObject.activeInHierarchy) continue;

                if (!screw.IsPlaced)
                {
                    allPlaced = false;
                    break;
                }
            }

            Debug.Log("allPlaced  =>  " + allPlaced);
            if (!allPlaced) return;

            Debug.Log("[Puzzle] All screws placed, now checking sequence...");

            Debug.Log("[Puzzle] ==== Current Hole / Index Mapping ====");
            foreach (var pair in holeDigitIndexMap.OrderBy(p => p.Value))
            {
                Debug.Log($"[Puzzle] Hole: {pair.Key.name} | Index: {pair.Value} | PlacedNumber: {(pair.Key is CrystalHoleController hh ? hh.PlacedNumber : -99)}");
            }
            Debug.Log("[Puzzle] ======================================");

            string builtAnswer = "";
            foreach (var pair in holeDigitIndexMap.OrderBy(p => p.Value))
            {
                if (pair.Key is CrystalHoleController h)
                {
                    if (h.PlacedNumber < 0)
                    {
                        Debug.Log("[Puzzle] Some hole is empty, cannot validate yet!");
                        return;
                    }

                    builtAnswer += h.PlacedNumber.ToString();
                    Debug.Log($"[Puzzle] Hole {h.name} Index {pair.Value} -> Digit {h.PlacedNumber}");
                }
            }

            Debug.Log($"[Puzzle] Final builtAnswer string = '{builtAnswer}'");

            if (!int.TryParse(builtAnswer, out int userAnswer))
            {
                Debug.LogError($"[Puzzle] BuiltAnswer invalid: {builtAnswer}");
                return;
            }

            int correctAnswer = CurrentPuzzleAnswer;

            Debug.Log($"[Puzzle] User Answer = {userAnswer} | Correct = {correctAnswer}");

            if (userAnswer == correctAnswer)
            {
                Debug.Log("[Puzzle] Correct Answer!");
                StopPeriodicShakeLoop();
                HideWrongAnswerMessage();
                CrystalLevelController.OnPuzzleCompleted();
            }
            else
            {
                Debug.Log("[Puzzle] Wrong Answer! Try Again...");

                ShakePlacedScrews(screws, duration: 0.35f, posStrength: 0.015f, rotStrength: 10f);
                VibrateLight();

                ShowWrongAnswerMessage();
            }
        }

        static void ShakePlacedScrews(List<CrystalScrewController> screws, float duration, float posStrength, float rotStrength)
        {
            if (screws == null) return;

            if (instance != null)
            {
                instance.StartCoroutine(instance.ShakeWithDelay(screws, duration, posStrength, rotStrength));
            }
        }

        IEnumerator ShakeWithDelay(List<CrystalScrewController> screws, float duration, float posStrength, float rotStrength)
        {
            yield return new WaitForSeconds(0.3f);

#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_MEDIUM);
#endif

            foreach (var screw in screws)
            {
                if (screw == null) continue;
                if (!screw.gameObject.activeInHierarchy) continue;
                if (!screw.IsPlaced) continue;

                var shaker = screw.GetComponent<SimpleShaker>();
                if (shaker == null) shaker = screw.gameObject.AddComponent<SimpleShaker>();

                shaker.Shake(duration, posStrength, rotStrength);
            }
        }

        public static void SetQuestions(string que)
        {
            if (CrystalUIGame.QueText == null)
            {
                Debug.LogError("[Puzzle] queText reference missing in UIManager inspector!");
                return;
            }

            CrystalUIGame.QueText.gameObject.SetActive(true);
            CrystalUIGame.QueText.text = que;

            Debug.Log("[Puzzle] Question set: " + que);
        }

        public static void GameOverSequence()
        {
            if (CrystalLevelController.StageLoader == null || !CrystalLevelController.StageLoader.StageLoaded) return;

            // read desired screw scale from instance
            float numberScale = 1.2f;
            if (instance != null && instance.numberScrewScale > 0f)
                numberScale = instance.numberScrewScale;

            // animation speed (higher = faster)
            float speed = (instance != null && instance.arrangementSpeed > 0f)
                          ? instance.arrangementSpeed
                          : 1f;
            float inv = 1f / speed;

            float startDelay = 2f * inv;

            DOVirtual.DelayedCall(startDelay, () =>
            {
                var baseHoles = CrystalLevelController.StageLoader.BaseHoles
                    .Where(h => h != null && h.gameObject.activeInHierarchy).ToList();
                var screws = CrystalLevelController.StageLoader.Screws
                    .Where(s => s != null && s.gameObject.activeInHierarchy).ToList();

                IsPuzzleModeActive = false;
                CurrentPuzzleAnswer = -1;
                activeBasehole = 0;

                var screwHoleMap = new Dictionary<CrystalScrewController, CrystalBaseHole>();
                foreach (var screw in screws)
                {
                    CrystalBaseHole nearestHole = null;
                    float minDist = float.MaxValue;

                    foreach (var hole in baseHoles)
                    {
                        float dist = Vector3.Distance(screw.transform.position, hole.transform.position);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            nearestHole = hole;
                        }
                    }

                    if (nearestHole != null)
                        screwHoleMap[screw] = nearestHole;
                }

                var orderedPairs = screwHoleMap
                    .Where(p => p.Key != null && p.Value != null
                             && p.Key.gameObject.activeInHierarchy
                             && p.Value.gameObject.activeInHierarchy)
                    .OrderBy(p => p.Value.transform.position.y)
                    .ThenBy(p => p.Value.transform.position.x)
                    .ToList();

                const int maxVisibleScrews = 10;
                var visiblePairs = orderedPairs.Take(maxVisibleScrews).ToList();

                foreach (var extra in orderedPairs.Skip(maxVisibleScrews))
                {
                    try { extra.Key.ResetNewData(); } catch { }
                    if (extra.Key != null) extra.Key.gameObject.SetActive(false);
                    try { extra.Value.ResetDataHole(); } catch { }
                    if (extra.Value != null) extra.Value.gameObject.SetActive(false);
                }

                baseHoles = baseHoles.Where(h => h != null && h.gameObject.activeInHierarchy).ToList();

                var holesWithScrew = visiblePairs.Select(p => p.Value).Distinct().ToList();
                var holesEmpty = baseHoles
                    .Where(h => h != null && h.gameObject.activeInHierarchy && !holesWithScrew.Contains(h))
                    .ToList();

                float topRowY = 5f;
                float bottomRowY = -5f;
                float gap = 1.8f;
                float worldHalfWidth = Camera.main.orthographicSize * Camera.main.aspect;
                float worldWidth = worldHalfWidth * 2f;

                int screwCount = visiblePairs.Count;
                int topRowCount = screwCount / 2;
                int bottomRowCount = screwCount - topRowCount;
                float gapX = 1.8f;

                float topStartX = -((topRowCount - 1) * gapX / 2f);
                float bottomStartX = -((bottomRowCount - 1) * gapX / 2f);

                var holeTargetPositions = new Dictionary<CrystalBaseHole, Vector3>();

                for (int index = 0; index < visiblePairs.Count; index++)
                {
                    var hole = visiblePairs[index].Value;

                    float xPos, yPos;
                    if (index < topRowCount)
                    {
                        xPos = topStartX + index * gapX;
                        yPos = topRowY;
                    }
                    else
                    {
                        int bottomIndex = index - topRowCount;
                        xPos = bottomStartX + bottomIndex * gapX;
                        yPos = topRowY - 2f;
                    }

                    holeTargetPositions[hole] = new Vector3(xPos, yPos, 0f);
                }

                holeDigitIndexMap.Clear();
                int maxBottom = 2;
                int activeCount = Mathf.Min(holesEmpty.Count, maxBottom);

                if (activeCount > 1)
                {
                    float totalWidth = (activeCount - 1) * gap;
                    if (totalWidth > worldWidth) gap = worldWidth / (activeCount - 1);
                }

                float startX = -(activeCount - 1) * gap / 2f;
                int activeIndex = 0;

                Debug.Log("EMPTY BASE HOLE (after hide extras) == > " + holesEmpty.Count);

                for (int i = 0; i < holesEmpty.Count; i++)
                {
                    CrystalBaseHole hole = holesEmpty[i];

                    if (activeIndex >= maxBottom)
                    {
                        try { hole.ResetDataHole(); } catch { }
                        hole.gameObject.SetActive(false);
                        continue;
                    }

                    activeBasehole++;
                    Vector3 targetPos = new Vector3(startX + activeIndex * gap, bottomRowY, 0f);
                    holeTargetPositions[hole] = targetPos;
                    holeDigitIndexMap[hole] = activeIndex;
                    if (hole is CrystalHoleController h) h.IsPuzzleHole = true;
                    activeIndex++;
                }

                screwIndex = 0;
                int screwCounter = 0;

                // FIRST MOVE: screws fly into their top/bottom row positions
                for (int index = 0; index < visiblePairs.Count; index++)
                {
                    var screw = visiblePairs[index].Key;
                    var hole = visiblePairs[index].Value;

                    float xPos, yPos;
                    if (index < topRowCount)
                    {
                        xPos = topStartX + index * gapX;
                        yPos = topRowY;
                    }
                    else
                    {
                        int b = index - topRowCount;
                        xPos = bottomStartX + b * gapX;
                        yPos = topRowY - 2f;
                    }

                    Vector3 targetPos = new Vector3(xPos, yPos, 0f);
                    float delay = screwCounter * 0.3f * inv;

                    var screwLocal = screw;
                    var targetPosLocal = targetPos;
                    float delayLocal = delay;

                    DOVirtual.DelayedCall(delayLocal, () =>
                    {
                        screwLocal.ForceSelect();
                        Sequence seq = DOTween.Sequence();
                        screwLocal.transform.DOMove(targetPosLocal, 0.5f * inv);

                        // force screw (and its number) to the configured scale
                        var t = screwLocal.transform;
                        t.localScale = new Vector3(numberScale, numberScale, t.localScale.z);
                    });

                    screwIndex++;
                    screwCounter++;
                }

                // Move holes into their desired world positions
                float holesStartDelay = (visiblePairs.Count * 0.3f + 0.1f) * inv;
                foreach (var kv in holeTargetPositions)
                {
                    var hole = kv.Key;
                    var targetPos = kv.Value;

                    var t = hole.transform;
                    var targetPosLocal = targetPos;

                    DOVirtual.DelayedCall(holesStartDelay, () =>
                    {
                        t.DOMove(targetPosLocal, 0.3f * inv);
                        t.DOScale(t.localScale * 1.4f, 0.3f * inv);
                    });
                }

                // SECOND MOVE: numbered screws drop exactly to hole positions
                int screwMoveIndex = 0;
                float holeToScrewDelay = 0.5f * inv;

                for (int i = 0; i < visiblePairs.Count; i++)
                {
                    var screw = visiblePairs[i].Key;
                    var hole = visiblePairs[i].Value;

                    if (holeTargetPositions.TryGetValue(hole, out Vector3 holePos))
                    {
                        float delay = holesStartDelay + holeToScrewDelay + (screwMoveIndex * 0.1f * inv);

                        var screwLocal = screw;
                        var holePosLocal = holePos;
                        int currentIndex = screwMoveIndex;

                        DOVirtual.DelayedCall(delay, () =>
                        {
                            screwLocal.SetNumberTexture(currentIndex);
                            screwLocal.transform.DOMove(
                                new Vector3(holePosLocal.x, holePosLocal.y, screwLocal.transform.position.z),
                                0.2f * inv
                            );

                            // again enforce the same scale on the whole screw
                            var t = screwLocal.transform;
                            t.localScale = new Vector3(numberScale, numberScale, t.localScale.z);

                            screwLocal.Deselect();
                        });

                        screwMoveIndex++;
                    }
                }

                ScreenManager.GetPage<CrystalUIGame>()?.SetReplayButtonInteractable(false);

                float totalAnimTime = holesStartDelay + holeToScrewDelay + (visiblePairs.Count * 0.1f * inv) + 0.3f * inv;

                DOVirtual.DelayedCall(totalAnimTime, () =>
                {
                    int holesCount = Mathf.Max(1, activeBasehole);
                    var puzzle = QuestionGenerator.GenerateQuestion(holesCount, CrystalLevelController.DisplayedLevelIndex, screwIndex);

                    if (CrystalUIGame.QueText != null)
                    {
                        CurrentPuzzleAnswer = puzzle.answer;
                        SetQuestions(puzzle.question);
                        IsPuzzleModeActive = true;
                        CrystalLevelController.isRealGameFinish = false;

                        StartPeriodicShakeLoop();

                        ScreenManager.GetPage<CrystalUIGame>()?.SetReplayButtonInteractable(true);
                    }
                    else
                    {
                        ScreenManager.GetPage<CrystalUIGame>()?.SetReplayButtonInteractable(true);
                    }
                });
            });
        }
    }

    // --------------------- Question Generator ---------------------
    public static class QuestionGenerator
    {
        private static System.Random rand = new System.Random();

        public static (string question, int answer) GenerateQuestion(int holesCount, int level, int MaxDigit)
        {
            // FORCE always 2 digits
            holesCount = 2;
            int maxDigit = Mathf.Clamp(MaxDigit - 1, 0, 9);

            int maxValue = 99;          // 10–99 possible
            int maxDigits = 2;          // always 2 digits

            List<int> candidates = new List<int>();

            // only 2-digit numbers
            for (int n = 10; n <= maxValue; n++)
            {
                if (IsAnswerValidTwoDigits(n, maxDigit)
                    && !HasRepeatedDigits(n))    // no 11, 22, 33, ...
                {
                    candidates.Add(n);
                }
            }

            int answer;
            if (candidates.Count > 0)
                answer = candidates[rand.Next(candidates.Count)];
            else
                answer = GenerateValidNumber(2, maxDigit);   // fallback 2-digit

            (string q, int ans) = GenerateEquation(answer);
            return (q, ans);
        }

        private static bool HasRepeatedDigits(int value)
        {
            string s = value.ToString();
            HashSet<char> seen = new HashSet<char>();
            foreach (char c in s)
            {
                if (seen.Contains(c))
                    return true;
                seen.Add(c);
            }
            return false;
        }

        private static bool IsAnswerValidTwoDigits(int value, int maxDigit)
        {
            if (value < 10 || value > 99) return false;

            string s = value.ToString();
            if (s.Length != 2) return false;

            foreach (char c in s)
            {
                int d = c - '0';
                if (d < 0 || d > maxDigit) return false;
            }

            return true;
        }

        private static int GenerateValidNumber(int digits, int maxDigit)
        {
            List<int> availableDigits = new List<int>();
            for (int i = 0; i <= maxDigit; i++) availableDigits.Add(i);

            availableDigits = availableDigits.OrderBy(x => rand.Next()).ToList();

            if (digits > 1 && availableDigits[0] == 0)
            {
                int swapIndex = availableDigits.FindIndex(d => d != 0);
                int temp = availableDigits[0];
                availableDigits[0] = availableDigits[swapIndex];
                availableDigits[swapIndex] = temp;
            }

            int number = 0;
            for (int i = 0; i < digits; i++)
            {
                number = number * 10 + availableDigits[i];
            }

            return number;
        }

        private static bool IsAnswerValidForDigits(int value, int maxDigit, int maxDigits)
        {
            string s = value.ToString();
            if (s.Length > maxDigits) return false;

            foreach (char c in s)
            {
                int d = c - '0';
                if (d < 0 || d > maxDigit) return false;
            }

            return true;
        }

        private static (string, int) GenerateEquation(int answer)
        {
            int a, b;
            string q;

            if (answer == 0)
            {
                int choice = rand.Next(0, 3);
                switch (choice)
                {
                    case 0:
                        a = rand.Next(1, 10);
                        q = $"{a} - {a} = ?";
                        break;
                    case 1:
                        q = $"0 + 0 = ?";
                        break;
                    default:
                        q = $"1 - 1 = ?";
                        break;
                }
                return (q, 0);
            }

            if (rand.Next(0, 2) == 0)
            {
                b = rand.Next(0, Math.Min(answer, 10));
                a = answer - b;
                q = $"{a} + {b} = ?";
            }
            else
            {
                b = rand.Next(0, 10);
                a = answer + b;
                q = $"{a} - {b} = ?";
            }

            return (q, answer);
        }
    }

    // --------------------- SimpleShaker ---------------------
    public class SimpleShaker : MonoBehaviour
    {
        Coroutine _shakeRoutine;

        public void Shake(float duration = 0.35f, float posStrength = 0.015f, float rotStrength = 10f)
        {
            if (_shakeRoutine != null) StopCoroutine(_shakeRoutine);
            _shakeRoutine = StartCoroutine(DoShake(duration, posStrength, rotStrength));
        }

        IEnumerator DoShake(float duration, float posStrength, float rotStrength)
        {
            var t = transform;
            Vector3 startLocalPos = t.localPosition;
            Quaternion startLocalRot = t.localRotation;

            float time = 0f;
            while (time < duration)
            {
                time += Time.deltaTime;
                float normalized = Mathf.Clamp01(time / duration);
                float damper = 1f - normalized;

                Vector3 offset = UnityEngine.Random.insideUnitSphere * (posStrength * damper);
                Vector3 euler = new Vector3(
                    UnityEngine.Random.Range(-rotStrength, rotStrength) * damper,
                    UnityEngine.Random.Range(-rotStrength, rotStrength) * damper,
                    UnityEngine.Random.Range(-rotStrength, rotStrength) * damper
                );

                transform.localPosition = startLocalPos + offset;
                transform.localRotation = startLocalRot * Quaternion.Euler(euler);

                yield return null;
            }

            transform.localPosition = startLocalPos;
            transform.localRotation = startLocalRot;

            _shakeRoutine = null;
        }
    }
}

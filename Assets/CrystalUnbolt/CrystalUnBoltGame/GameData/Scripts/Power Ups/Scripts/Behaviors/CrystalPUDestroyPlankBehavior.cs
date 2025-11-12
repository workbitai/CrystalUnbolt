using System.Collections;
using UnityEngine;
using DG.Tweening; 
using UnityEngine.UI; 

namespace CrystalUnbolt
{
    public class CrystalPUDestroyPlankBehavior : CrystalPUBehavior
    {
        [Header("Particles")]
        [SerializeField] Particle particle;
        [SerializeField] bool usePlankShape = true;
        [SerializeField] bool usePlankColor = true;

        [Header("Hammer Animation")]
        [SerializeField] GameObject hammerPrefab;
        [SerializeField] Transform parentObject;
        [SerializeField] bool useLeftCornerAlignment = false;  

        [Tooltip("Set 0 for 'no rukna'.")]
        [SerializeField, Min(0f)] float preSpawnDelay = 0f;     

        [Header("Timing (Swipe -> Grow -> DHAM)")]
        [SerializeField, Min(0.01f)] float growUpDuration = 0.24f;   
        [SerializeField, Min(0.01f)] float slamDownDuration = 0.08f; 
        [SerializeField, Min(0.01f)] float settleDuration = 0.20f;   
        [SerializeField, Min(0.01f)] float fadeOutDuration = 0.28f;

        [Header("Scales")]
        [SerializeField] float scaleA_Small = 0.8f;   
        [SerializeField] float scaleB_Big = 1.5f;    
        [SerializeField] float scaleC_Impact = 0.6f;
        [SerializeField] float scaleD_Settle = 0.8f; 

        [Header("Hit Feel")]
        [SerializeField] bool useCameraShake = false; 
        [SerializeField, Min(0f)] float camShakeDuration = 0.12f;
        [SerializeField, Min(0f)] float camShakeStrength = 0.25f;
        [SerializeField] bool shakeHammerOnImpact = true;
        [SerializeField, Min(0f)] float hammerShakeDuration = 0.10f;
        [SerializeField, Min(0f)] float hammerShakeStrength = 0.06f;

        [Header("Haptics")]
        [SerializeField] bool useHaptics = true;
        [SerializeField, Range(0f, 1f)] float hapticStrength = 0.8f;     
        [SerializeField, Min(0f)] float hapticDurationMultiplier = 1.0f; 
        [SerializeField] bool vibrateOnGrow = false;                     
        [SerializeField] bool vibrateOnSlam = true;                 

        [Header("Render On Top")]
        [SerializeField] bool forceOnTop = true;                    
        [SerializeField] string sortingLayerName = "UI";            
        [SerializeField] int sortingOrderOnTop = 32767;             
        [SerializeField] bool useNestedCanvasForUI = true;          

        [Header("Depth Override (3D Mesh Fix)")]
        [SerializeField] bool forceZInFront = true;                 
        [SerializeField, Min(0.001f)] float zFrontOffset = 0.05f;   

        DG.Tweening.Sequence _hammerSeq;

        public override void Init()
        {
            CrystalParticlesController.RegisterParticle(particle);
        }

        public override bool Activate() => true;

        public override void OnSelected()
        {
            base.OnSelected();
            if (CrystalScrewController.SelectedScrew != null) CrystalScrewController.SelectedScrew.Deselect();
        }

        public override bool ApplyToElement(IClickableObject clickableObject, Vector3 clickPosition)
        {
            if (PowerUpLock.IsLocked) return false; 

            if (clickableObject is CrystalPlankController plank)
            {
                StartCoroutine(LockDuringHammerAnimation(plank.transform.position, plank));
                return true;
            }
            return false;
        }

        IEnumerator LockDuringHammerAnimation(Vector3 pos, CrystalPlankController plank)
        {
            PowerUpLock.IsLocked = true; 
            StartHammerAnimation(pos, plank);

            float total = growUpDuration + slamDownDuration + settleDuration + fadeOutDuration + 0.1f;
            yield return new WaitForSeconds(total);

            PowerUpLock.IsLocked = false; 
        }


        void StartHammerAnimation(Vector3 targetPos, CrystalPlankController plank)
        {
            if (_hammerSeq != null && _hammerSeq.IsActive()) _hammerSeq.Kill();

            Vector3 startPos = GetPowerUpButtonPosition();
            if (hammerPrefab == null)
            {
                PlayParticlesAndDestroy(plank, targetPos);
                return;
            }

            var hammer = Instantiate(hammerPrefab, startPos, Quaternion.identity);
            hammer.name = "HammerAnimation";
            var parent = parentObject;
            if (parent == null)
            {
                CrystalUIGame ui = ScreenManager.GetPage<CrystalUIGame>();
                parent = ui ? ui.transform : transform;
            }
            hammer.transform.SetParent(parent, true);

            hammer.transform.rotation = Quaternion.identity;
            hammer.transform.localScale = Vector3.one;

            if (forceOnTop) BringToFront(hammer);

            StartCoroutine(SimpleHammerAnimation(hammer, plank, targetPos));
        }

        IEnumerator SimpleHammerAnimation(GameObject hammer, CrystalPlankController plank, Vector3 targetPos)
        {
            Vector3 pos = targetPos;
            if (forceZInFront) pos = ToFrontOfCameraXY(targetPos, zFrontOffset);
            hammer.transform.position = pos;
            hammer.transform.localScale = Vector3.one * scaleA_Small;

            yield return StartCoroutine(ScaleOverTime(hammer.transform, scaleA_Small, scaleB_Big, growUpDuration));

            yield return StartCoroutine(ScaleOverTime(hammer.transform, scaleB_Big, scaleC_Impact, slamDownDuration));

            DoImpactFX(hammer.transform, plank, targetPos);

            yield return StartCoroutine(ScaleOverTime(hammer.transform, scaleC_Impact, scaleD_Settle, settleDuration));

            FadeAndCleanup(hammer);
        }

        IEnumerator ScaleOverTime(Transform target, float fromScale, float toScale, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float currentScale = Mathf.Lerp(fromScale, toScale, t);
                target.localScale = Vector3.one * currentScale;
                elapsed += Time.deltaTime;
                yield return null;
            }
            target.localScale = Vector3.one * toScale;
        }

        IEnumerator SimpleShake(Transform target, float duration, float strength)
        {
            Vector3 originalPos = target.position;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                Vector3 randomOffset = new Vector3(
                    Random.Range(-strength, strength),
                    Random.Range(-strength, strength),
                    0f
                );
                target.position = originalPos + randomOffset;
                elapsed += Time.deltaTime;
                yield return null;
            }

            target.position = originalPos;
        }

        Vector3 ToFrontOfCameraXY(Vector3 worldXY, float nearOffset)
        {
            var cam = Camera.main;
            if (cam == null) return worldXY;
            Vector3 sp = cam.WorldToScreenPoint(worldXY);
            sp.z = cam.nearClipPlane + Mathf.Max(0.001f, nearOffset);
            return cam.ScreenToWorldPoint(sp);
        }

        void BringToFront(GameObject hammer)
        {
            var parentCanvas = hammer.GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                var root = parentCanvas.rootCanvas;
                hammer.transform.SetParent(root.transform, true);
                hammer.transform.SetAsLastSibling();

                if (useNestedCanvasForUI)
                {
                    var localCanvas = hammer.GetComponent<Canvas>();
                    if (localCanvas == null) localCanvas = hammer.AddComponent<Canvas>();
                    localCanvas.overrideSorting = true;
                    localCanvas.sortingOrder = sortingOrderOnTop;

                    int layerId = SortingLayer.NameToID(sortingLayerName);
                    if (layerId != 0)
                    {
                        localCanvas.sortingLayerID = layerId;
                        localCanvas.sortingLayerName = sortingLayerName;
                    }
                }
            }

            var srs = hammer.GetComponentsInChildren<SpriteRenderer>(true);
            if (srs != null && srs.Length > 0)
            {
                int layerId = SortingLayer.NameToID(sortingLayerName);
                foreach (var sr in srs)
                {
                    if (layerId != 0) sr.sortingLayerID = layerId;
                    sr.sortingOrder = sortingOrderOnTop;
                }
            }

            var sg = hammer.GetComponent<UnityEngine.Rendering.SortingGroup>();
            if (sg != null)
            {
                int layerId = SortingLayer.NameToID(sortingLayerName);
                if (layerId != 0) sg.sortingLayerID = layerId;
                sg.sortingOrder = sortingOrderOnTop;
            }
        }

        void DoImpactFX(Transform hammerTr, CrystalPlankController plank, Vector3 targetPos)
        {
            if (useCameraShake && Camera.main != null && camShakeDuration > 0f && camShakeStrength > 0f)
            {
                var cam = Camera.main.transform;
                DG.Tweening.ShortcutExtensions.DOShakePosition(cam, camShakeDuration, camShakeStrength, 20, 90f, false, true);

                if (useHaptics && vibrateOnSlam)
                {
                    float strength01 = Mathf.Clamp01(hapticStrength * Mathf.InverseLerp(0.05f, 0.5f, camShakeStrength));
                    float dur = camShakeDuration * hapticDurationMultiplier;
                    TriggerHaptics(strength01, dur);
                }
            }
            else if (useHaptics && vibrateOnSlam)
            {
                TriggerHaptics(hapticStrength, 0.06f * hapticDurationMultiplier);
            }

            if (shakeHammerOnImpact && hammerShakeDuration > 0f && hammerShakeStrength > 0f)
            {
                StartCoroutine(SimpleShake(hammerTr, hammerShakeDuration, hammerShakeStrength));
            }

            PlayParticlesAndDestroy(plank, targetPos);
        }


        void TriggerHaptics(float strength01, float durationSec)
        {
            CrystalUniversalVibration.Vibrate(durationSec, strength01);
        }

        void FadeAndCleanup(GameObject hammer)
        {
            StartCoroutine(SimpleFadeOut(hammer));
        }

        IEnumerator SimpleFadeOut(GameObject hammer)
        {
            var sr = hammer.GetComponent<SpriteRenderer>();
            var graphics = hammer.GetComponentsInChildren<Graphic>(true);

            if (sr != null)
            {
                Color startColor = sr.color;
                float elapsed = 0f;
                while (elapsed < fadeOutDuration)
                {
                    float t = elapsed / fadeOutDuration;
                    float alpha = Mathf.Lerp(startColor.a, 0f, t);
                    sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                sr.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
            }
            else if (graphics != null && graphics.Length > 0)
            {
                Color[] startColors = new Color[graphics.Length];
                for (int i = 0; i < graphics.Length; i++)
                {
                    startColors[i] = graphics[i].color;
                }

                float elapsed = 0f;
                while (elapsed < fadeOutDuration)
                {
                    float t = elapsed / fadeOutDuration;
                    for (int i = 0; i < graphics.Length; i++)
                    {
                        float alpha = Mathf.Lerp(startColors[i].a, 0f, t);
                        graphics[i].color = new Color(startColors[i].r, startColors[i].g, startColors[i].b, alpha);
                    }
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                for (int i = 0; i < graphics.Length; i++)
                {
                    graphics[i].color = new Color(startColors[i].r, startColors[i].g, startColors[i].b, 0f);
                }
            }

            if (hammer != null) Object.Destroy(hammer);
        }

        void PlayParticlesAndDestroy(CrystalPlankController plankBehavior, Vector3 pos)
        {
            ParticleCase pc = particle.Play().SetPosition(pos);
            pc.ParticleSystem.Stop();
            pc.ParticleSystem.transform.localEulerAngles = plankBehavior.transform.localEulerAngles;

            Sprite plankSprite = plankBehavior.SpriteRenderer.sprite;

            if (usePlankColor)
            {
                var main = pc.ParticleSystem.main;
                main.startColor = plankBehavior.Color;
            }

            pc.ApplyToParticles((ParticleSystem ps) =>
            {
                if (usePlankShape)
                {
                    var shape = ps.shape;
                    shape.sprite = plankSprite;
                    shape.rotation = new Vector3(0, 180, 0);
                }
            });

            pc.ParticleSystem.Play();
            plankBehavior.DestroyPlank();
        }

        Vector3 GetPowerUpButtonPosition()
        {
            CrystalUIGame uiGame = ScreenManager.GetPage<CrystalUIGame>();
            if (uiGame != null && uiGame.PowerUpsUIController != null)
            {
                CrystalPUUIBehavior panel = uiGame.PowerUpsUIController.GetPanel(CrystalPUType.DestroyPlank);
                if (panel != null) return panel.transform.position;
            }
            return Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.8f, Screen.height * 0.1f, 10f));
        }

        static bool IsValid(Vector3 v) => !(float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z));

        public override bool IsSelectable() => true;
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace CrystalUnbolt
{
    public class CrystalScrewController : MonoBehaviour, IClickableObject
    {
        private static readonly int EXTRACT_CRYSTAL_TRIGGER = Animator.StringToHash("Extract Crystal");
        private static readonly int EXTRACT_CRYSTAL_PU_TRIGGER = Animator.StringToHash("Extract Crystal PU");
        private static readonly int SCREW_IN_TRIGGER = Animator.StringToHash("Screw In");

        private static readonly int EXTRACT_CRYSTAL_SPEED_MULTIPLIER_FLOAT = Animator.StringToHash("Extract Crystal Speed Multiplier");
        private static readonly int EXTRACT_CRYSTAL_PU_SPEED_MULTIPLIER_FLOAT = Animator.StringToHash("Extract Crystal PU Speed Multiplier");
        private static readonly int SCREW_IN_SPEED_MULTIPLIER_FLOAT = Animator.StringToHash("Screw In Speed Multiplier");

        private static readonly Vector3 DEFAULT_HIGHLIGHT_SIZE = Vector3.one;
        private const float HIGHLIGHT_BREATHING_SPEED = 1.2f;

        [SerializeField] Collider2D screwCollider;
        [SerializeField] SpriteRenderer shadowSpriteRenderer;
        [SerializeField] Transform visuals;
        [SerializeField] Animator animator;
        [Header("Particles")]
        [SerializeField] private ParticleSystem screwPickParticlesPrefab;
        [SerializeField] private ParticleSystem screwPlaceParticlesPrefab;
        private ParticleSystem screwPickParticlesInstance;
        private ParticleSystem screwPlaceParticlesInstance;

        [SerializeField] MeshRenderer screwRenderer;

        [SerializeField] Texture[] numberTextures;

        private Material runtimeMaterial; 

        private List<CrystalHoleController> connectedHoles = new List<CrystalHoleController>();

        public Vector3 Position => transform.position;

        private static CrystalScrewController selectedScrew;
        public static CrystalScrewController SelectedScrew => selectedScrew;

        private bool isHighlighted;
        private AnimCase moveTweenCase;

        private float highlightBreathingState;
        private Vector3 cachedVisualsLocalPos;

        public event GameCallback CrystalExtracted;
        public event GameCallback Selected;
        public event GameCallback Deselected;
        [SerializeField] private Texture defaultTexture;

        public bool IsPlaced { get; private set; } = false;
        private void Awake()
        {
            cachedVisualsLocalPos = visuals.localPosition;

            screwCollider.enabled = false;

            if (screwRenderer != null)
            {
                runtimeMaterial = new Material(screwRenderer.material);
                screwRenderer.material = runtimeMaterial;
            }

            screwPickParticlesInstance = CreateParticleInstance(screwPickParticlesPrefab);
            screwPlaceParticlesInstance = CreateParticleInstance(screwPlaceParticlesPrefab);
        }

        public void SetNumberTexture(int index)
        {
            if (runtimeMaterial == null || numberTextures == null) return;

            if (index >= 0 && index < numberTextures.Length)
            {
                runtimeMaterial.mainTexture = numberTextures[index];
                SetScrewNumber(index);
            }
            else
            {
                Debug.LogWarning($"[Screw] Invalid texture index: {index}");
            }
        }

        public void Init(List<CrystalBaseHole> baseHoles, List<CrystalPlankController> planks)
        {
            isHighlighted = false;

            CrystalExtracted = null;
            Selected = null;
            Deselected = null;

            shadowSpriteRenderer.color = CrystalGameManager.Data.ScrewShadowColor;
            shadowSpriteRenderer.transform.localScale = DEFAULT_HIGHLIGHT_SIZE;

            highlightBreathingState = 0;

            for (int i = 0; i < baseHoles.Count; i++)
            {
                CrystalBaseHole hole = baseHoles[i];

                if (Vector2.Distance(hole.transform.position, transform.position) <= 0.1f)
                {
                    hole.SetActive(true);
                    connectedHoles.Add(hole);
                    break;
                }
            }

            for (int i = 0; i < planks.Count; i++)
            {
                CrystalPlankController plank = planks[i];

                for (int j = 0; j < plank.Holes.Count; j++)
                {
                    CrystalPlankHole plankHole = plank.Holes[j];

                    if (Vector2.Distance(plankHole.transform.position, transform.position) <= 0.1f)
                    {
                        connectedHoles.Add(plankHole);
                        plankHole.ActivateJoint(screwCollider, transform.position);
                        break;
                    }
                }
            }
        }

        public void EnableCollider() => screwCollider.enabled = true;

        private void Update()
        {
            if (isHighlighted && CrystalGameManager.Data.HighlightBreathingEffect)
            {
                highlightBreathingState = Mathf.PingPong(HIGHLIGHT_BREATHING_SPEED * Time.time, 1);
                shadowSpriteRenderer.color = Color.Lerp(CrystalGameManager.Data.ScrewHighlightColor,
                                                        CrystalGameManager.Data.ScrewHighlightColor.SetAlpha(0.5f),
                                                        highlightBreathingState);
            }
        }

        private void ExtractCrystal()
        {
            for (int i = 0; i < connectedHoles.Count; i++)
            {
                var hole = connectedHoles[i];

                if (hole is CrystalPlankHole plankHole)
                {
                    plankHole.DisableJoint(screwCollider, true);
                }
                else
                {
                    (hole as CrystalBaseHole).SetActive(false);
                }

                if (hole is CrystalHoleController h && h.IsPuzzleHole)
                {
                    h.ClearPlacedNumber();
                }
            }


            connectedHoles.Clear();

            CrystalExtracted?.Invoke();
        }


        public void ExtractAndDiscardCrystal()
        {
            animator.SetFloat(EXTRACT_CRYSTAL_PU_SPEED_MULTIPLIER_FLOAT, CrystalGameManager.Data.CrystalExtractionPUAnimationSpeedMultiplier);
            animator.SetTrigger(EXTRACT_CRYSTAL_PU_TRIGGER);
        }

        public void OnExtractCrystalPUAnimEnded()
        {
            ExtractCrystal();
            Discard();
        }

        public void ChangeHoles(List<CrystalHoleController> newHoles)
        {
            ExtractCrystal();

            connectedHoles = newHoles;
            var newPosition = newHoles[^1].transform.position.SetZ(transform.position.z);
            var difference = newPosition - transform.position;

            visuals.localPosition = visuals.localPosition - difference;
            transform.position = newPosition;

            for (int i = 0; i < connectedHoles.Count; i++)
            {
                var hole = connectedHoles[i];
                Debug.Log($"[Screw Debug] Screw: {gameObject.name} | Hole: {hole.gameObject.name} | Position: {hole.transform.position}");

                if (connectedHoles[i] is CrystalPlankHole plankHole)
                {
                    plankHole.ActivateJoint(screwCollider, newPosition);
                }
                else
                {
                    (connectedHoles[i] as CrystalBaseHole).SetActive(true);
                }

                if (hole is CrystalHoleController h && h.IsPuzzleHole)
                {
                    h.SetPlacedNumber(ScrewNumber);
                    Debug.Log($"[Puzzle] Screw {ScrewNumber} placed in Puzzle Hole {h.name}");
                }
            }


#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif

            moveTweenCase.KillActive();
            moveTweenCase = visuals.DOLocalMove(cachedVisualsLocalPos, CrystalGameManager.Data.ScrewMovementDuration)
                .SetEasing(Ease.Type.SineInOut)
                .OnComplete(() => Deselect());
        }

        public int PlacedNumber { get; private set; }

        public void SetPlacedNumber(int num)
        {
            PlacedNumber = num;
            Debug.Log($"[Hole] {gameObject.name} me Screw Number = {num}");
            CrystalGameOverArranger.CheckGameOver();

        }

        public void OnObjectClicked(Vector3 clickPosition)
        {
            Debug.Log("IS REAL Game = >  " + CrystalLevelController.isRealGameFinish);
            if (CrystalLevelController.isRealGameFinish) return;
            if (moveTweenCase.ExistsAndActive()) return;

            if (SelectedScrew != null && SelectedScrew == this) Deselect();
            else Select();
        }

        public void Select()
        {
           
            if (selectedScrew != null) selectedScrew.Deselect();
            selectedScrew = this;

            animator.SetFloat(EXTRACT_CRYSTAL_SPEED_MULTIPLIER_FLOAT, CrystalGameManager.Data.CrystalExtractionAnimationSpeedMultiplier);
            animator.SetTrigger(EXTRACT_CRYSTAL_TRIGGER);

            SoundManager.PlaySound(SoundManager.AudioClips.screwPick);
            PlayParticles(screwPickParticlesInstance);

#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif

            Selected?.Invoke();
        }

        public void ForceSelect()
        {
            animator.SetFloat(EXTRACT_CRYSTAL_SPEED_MULTIPLIER_FLOAT, CrystalGameManager.Data.CrystalExtractionAnimationSpeedMultiplier);
            animator.SetTrigger(EXTRACT_CRYSTAL_TRIGGER);
            SoundManager.PlaySound(SoundManager.AudioClips.screwPick);
            PlayParticles(screwPickParticlesInstance);
            Selected?.Invoke();
        }

        public void Deselect()
        {
            animator.SetFloat(SCREW_IN_SPEED_MULTIPLIER_FLOAT, CrystalGameManager.Data.ScrewInAnimationSpeedMultiplier);
            animator.SetTrigger(SCREW_IN_TRIGGER);
            SetPlacedNumber(ScrewNumber);

            IsPlaced = true;
            SoundManager.PlaySound(SoundManager.AudioClips.screwPlace);
            PlayParticles(screwPlaceParticlesInstance);

            selectedScrew = null;
            Deselected?.Invoke();
        }

        public void Discard()
        {
            connectedHoles.Clear();
            gameObject.SetActive(false);

            moveTweenCase.KillActive();
            visuals.localPosition = cachedVisualsLocalPos;
            screwCollider.enabled = false;
        }

        public void Highlight()
        {
            if (isHighlighted) return;

            isHighlighted = true;
            shadowSpriteRenderer.color = CrystalGameManager.Data.ScrewHighlightColor;
            shadowSpriteRenderer.transform.localScale = DEFAULT_HIGHLIGHT_SIZE * CrystalGameManager.Data.HighlightSizeMultiplier;
        }

        public void Unhighlight()
        {
            if (!isHighlighted) return;

            isHighlighted = false;
            shadowSpriteRenderer.color = CrystalGameManager.Data.ScrewShadowColor;
            shadowSpriteRenderer.transform.localScale = DEFAULT_HIGHLIGHT_SIZE;
            highlightBreathingState = 0;
        }
        public int ScrewNumber { get; private set; }

        public void SetScrewNumber(int num)
        {
            ScrewNumber = num;
        }

        private ParticleSystem CreateParticleInstance(ParticleSystem prefab)
        {
            if (prefab == null) return null;

            var instance = Instantiate(prefab, transform);
            instance.transform.localPosition = Vector3.zero;
            instance.gameObject.SetActive(false);
            return instance;
        }

        private void PlayParticles(ParticleSystem targetParticles)
        {
            if (targetParticles == null) return;

            targetParticles.transform.localPosition = Vector3.zero;
            targetParticles.gameObject.SetActive(true);
            targetParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            targetParticles.Play(true);
        }

        public void ResetNewData()
        {
            PlacedNumber = -1;

            ScrewNumber = -1;

            if (runtimeMaterial != null && defaultTexture != null)
            {
                runtimeMaterial.mainTexture = defaultTexture;
            }
            Debug.Log("Visual   =>  " + visuals.name);
            Debug.Log("Visual   =>  " + visuals.localScale);
            visuals.localScale = Vector3.one * 1.20f;


        }

    }
}

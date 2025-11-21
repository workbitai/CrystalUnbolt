using System.Collections.Generic;
using UnityEngine;

namespace CrystalUnbolt
{
    public class CrystalPlankController : MonoBehaviour, IClickableObject
    {
        [SerializeField] SpriteRenderer spriteRenderer;
        public SpriteRenderer SpriteRenderer => spriteRenderer;

        [SerializeField] ColliderType colliderType;
        [SerializeField, HideIf("IsColliderTypePolygon")] List<Collider2D> colliders;
        [SerializeField, ShowIf("IsColliderTypePolygon")] PolygonCollider2D polygonCollider;

        public Vector3 Position => transform.position;
        public int SortingOrder => spriteRenderer.sortingOrder;
        public Color Color => spriteRenderer.color;

        public bool IsBeingDisabled { get; private set; }

        private Rigidbody2D rb;
        public Rigidbody2D Rigidbody
        {
            get
            {
                if (rb == null)
                    rb = GetComponent<Rigidbody2D>();
                return rb;
            }
        }

        private List<CrystalPlankHole> holes;
        public List<CrystalPlankHole> Holes => holes;

        private CrystalPlankLevelData data;
        public int Layer => data.PlankLayer;

        private AnimCase disableCase;

        private void Awake()
        {
            DisableColliders();
            StopSimulation();
        }

        public void Init(CrystalPlankLevelData data, Color color, int id)
        {
            this.data = data;

            transform.position = data.Position.SetZ(-0.01f * (data.PlankLayer + 1));
            transform.eulerAngles = data.Rotation;
            transform.localScale = data.Scale;
            spriteRenderer.color = color;
            spriteRenderer.sortingOrder = id * 2;

            disableCase.KillActive();
            IsBeingDisabled = false;

            rb.totalTorque = 0;
#if UNITY_6000
            rb.linearVelocity = Vector2.zero;
#else
            rb.velocity = Vector2.zero;
#endif
            rb.angularVelocity = 0;
            rb.totalForce = Vector2.zero;
        }

        public void SetHoles(List<Vector3> holePostions, CrystalSkinsManager CrystalSkinsManager)
        {
            holes = new List<CrystalPlankHole>();
            for (int i = 0; i < holePostions.Count; i++)
            {
                Vector3 holePosition = holePostions[i];
                holePosition.z = -0.005f;

                CrystalPlankHole hole = CrystalSkinsManager.GetPlankHole();
                hole.transform.SetParent(transform);
                hole.transform.localPosition = holePosition.Divide(transform.localScale);

                hole.Init(this);

                holes.Add(hole);
            }
        }

        public void EnableColliders()
        {
            if (colliderType == ColliderType.Primitive)
            {
                for (int i = 0; i < colliders.Count; i++)
                {
                    colliders[i].enabled = true;
                }
            }
            else
            {
                polygonCollider.enabled = true;
            }
        }

        public void DisableColliders()
        {
            if (colliderType == ColliderType.Primitive)
            {
                for (int i = 0; i < colliders.Count; i++)
                {
                    colliders[i].enabled = false;
                }
            }
            else
            {
                polygonCollider.enabled = false;
            }

            Rigidbody.totalTorque = 0;
#if UNITY_6000
            rb.linearVelocity = Vector2.zero;
#else
            rb.velocity = Vector2.zero;
#endif
            Rigidbody.angularVelocity = 0;
            Rigidbody.totalForce = Vector2.zero;
        }

        public void StopSimulation()
        {
            Rigidbody.simulated = false;
        }

        public void StartSimulation()
        {
            Rigidbody.simulated = true;
        }

        public void IgnorePlank(CrystalPlankController plankToIgnore)
        {
            if (plankToIgnore.colliderType == ColliderType.Primitive)
            {
                for (int i = 0; i < plankToIgnore.colliders.Count; i++)
                {
                    IgnoreCollider(plankToIgnore.colliders[i]);
                }
            }
            else
            {
                IgnoreCollider(plankToIgnore.polygonCollider);
            }
        }

        public void CollideWithPlank(CrystalPlankController plankToIgnore)
        {
            if (plankToIgnore.colliderType == ColliderType.Primitive)
            {
                for (int i = 0; i < plankToIgnore.colliders.Count; i++)
                {
                    StopIgnoringCollider(plankToIgnore.colliders[i]);
                }
            }
            else
            {
                StopIgnoringCollider(plankToIgnore.polygonCollider);
            }
        }

        public void IgnoreCollider(Collider2D collider)
        {
            if (colliderType == ColliderType.Primitive)
            {
                for (int i = 0; i < colliders.Count; i++)
                {
                    Physics2D.IgnoreCollision(collider, colliders[i], true);
                }
            }
            else
            {
                Physics2D.IgnoreCollision(collider, polygonCollider, true);
            }
        }

        public void StopIgnoringCollider(Collider2D collider)
        {
            if (colliderType == ColliderType.Primitive)
            {
                for (int i = 0; i < colliders.Count; i++)
                {
                    Physics2D.IgnoreCollision(collider, colliders[i], false);
                }
            }
            else
            {
                Physics2D.IgnoreCollision(collider, polygonCollider, false);
            }
        }

        public void OnHoleAboutToBeSnapped(CrystalPlankHole hole, Vector3 desiredHolePosition)
        {
            if (Time.fixedTime < 0.1f) return;

            CrystalPlankHole activeHole = null;
            for (int i = 0; i < holes.Count; i++)
            {
                CrystalPlankHole plankHole = holes[i];

                if (plankHole.IsActive)
                {
                    if (activeHole != null) return;
                    activeHole = plankHole;
                }
            }

            if (activeHole == null)
            {
                return;
            }

            float step = 0.5f;
            float distance = Vector2.Distance(desiredHolePosition, hole.Position);
            if (distance < 0.05f) return;

            while (Mathf.Abs(step) > 0.02f)
            {
                transform.RotateAround(activeHole.transform.position, Vector3.forward, step);

                float newDistance = Vector2.Distance(desiredHolePosition, hole.Position);

                if (newDistance > distance)
                {
                    step /= -2f;
                }

                distance = newDistance;
            }
        }

        public void OnHoleCrystalExtracted()
        {
            int counter = 0;
            for (int i = 0; i < holes.Count; i++)
            {
                CrystalPlankHole plankHole = holes[i];

                if (plankHole.IsActive)
                {
                    counter++;
                }
            }

            Rigidbody.bodyType = counter > 1 ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;

            Tween.NextFrame(OnHoleCrystalExtractedNextFrame);
        }

        private void OnHoleCrystalExtractedNextFrame()
        {
            int counter = 0;

            for (int i = 0; i < holes.Count; i++)
            {
                CrystalPlankHole plankHole = holes[i];

                if (plankHole.IsActive)
                {
                    counter++;
                }
            }

            if (counter == 1)
            {
                float sign = (Random.Range(0, 2) * 2 - 1);
                Rigidbody.AddTorque(100f * sign, ForceMode2D.Force);
            }
            else if (counter == 0)
            {
                // Plank has fully detached and will start falling
                CrystalLevelController.OnPlankStartFalling();
            }
        }

        public void OnObjectClicked(Vector3 clickPosition)
        {

        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Play plank collision sound when plank collides with any object
            if (SoundManager.AudioClips.plankCollision != null)
            {
                SoundManager.PlaySound(SoundManager.AudioClips.plankCollision);
            }
        }

        public void DestroyPlank()
        {
            CrystalLevelController.UpdateDestroyedPlanks();
            Discard(false);
        }

        public void Disable()
        {
            IsBeingDisabled = true;
            CrystalLevelController.UpdateDestroyedPlanks();
            disableCase = Tween.DelayedCall(0.3f, () =>
            {
                Discard(true);

                SoundManager.PlaySound(SoundManager.AudioClips.plankComplete);
            });
        }

        public void Discard(bool withParticle)
        {
            /*if (withParticle)
            {
                ParticleCase particleCase = CrystalParticlesController.PlayParticle("Confetti");

                Transform particleTransform = particleCase.ParticleSystem.transform;
                particleTransform.position = transform.position;
                particleTransform.rotation = Quaternion.FromToRotation(Vector3.up, (Vector3.up * 10 - transform.position.SetZ(0)).normalized);
            }*/

            for (int i = 0; i < holes.Count; i++)
            {
                holes[i].Discard();
            }

            holes.Clear();

            data = null;

            gameObject.SetActive(false);
            DisableColliders();
            StopSimulation();

            transform.position = Vector3.zero;

            disableCase.KillActive();

            Tween.NextFrame(() => IsBeingDisabled = false);
        }

        public bool IsFlying()
        {
            for (int i = 0; i < holes.Count; i++)
            {
                CrystalPlankHole hole = holes[i];

                if (hole.IsActive) return false;
            }

            return true;
        }

        public bool DoesPointOverlapsCollider(Vector3 point)
        {
            if (colliderType == ColliderType.Primitive)
            {
                bool result = false;

                for (int i = 0; i < colliders.Count; i++)
                {
                    result = colliders[i].OverlapPoint(point);

                    if (result)
                        return true;
                }

                return false;
            }
            else
            {
                return polygonCollider.OverlapPoint(point);
            }
        }

        protected bool IsColliderTypePolygon() => colliderType == ColliderType.Polygon;

        private enum ColliderType
        {
            Primitive = 0,
            Polygon = 1,
        }
    }
}
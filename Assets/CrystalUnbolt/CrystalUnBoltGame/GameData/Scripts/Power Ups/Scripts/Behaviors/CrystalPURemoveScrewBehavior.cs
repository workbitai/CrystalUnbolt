using UnityEngine;

namespace CrystalUnbolt
{

    public class CrystalPURemoveScrewBehavior : CrystalPUBehavior
    {
        [SerializeField] Particle particle;

        public override void Init()
        {
            CrystalParticlesController.RegisterParticle(particle);
        }

        public override bool Activate()
        {
            return true;
        }

        public override bool ApplyToElement(IClickableObject clickableObject, Vector3 clickPosition)
        {
            if (clickableObject is CrystalScrewController)
            {
                CrystalScrewController screwBehavior = (CrystalScrewController)clickableObject;
                if(screwBehavior != null)
                {
                    particle.Play().SetPosition(screwBehavior.transform.position);

                    screwBehavior.ExtractAndDiscardCrystal();

                    return true;
                }
            }

            return false;
        }

        public override void OnSelected()
        {
            base.OnSelected();

            if (CrystalScrewController.SelectedScrew != null) CrystalScrewController.SelectedScrew.Deselect();

            foreach (CrystalScrewController screw in CrystalLevelController.StageLoader.Screws)
            {
                screw.Highlight();
            }
        }

        public override void OnDeselected()
        {
            base.OnDeselected();

            foreach (CrystalScrewController screw in CrystalLevelController.StageLoader.Screws)
            {
                screw.Unhighlight();
            }
        }

        public override bool IsSelectable() => true;
    }
}

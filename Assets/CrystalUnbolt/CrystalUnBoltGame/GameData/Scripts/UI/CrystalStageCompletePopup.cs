using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalUnbolt
{
    public class CrystalStageCompletePopup : MonoBehaviour
    {
        private GameCallback onMaxFade;

        public void Show(GameCallback onMaxFade)
        {
            gameObject.SetActive(true);

            this.onMaxFade = onMaxFade;
        }

        public void OnMaxFade()
        {
            onMaxFade?.Invoke();
        }

        public void OnFadeDone()
        {
            gameObject.SetActive(false);
        }
    }
}

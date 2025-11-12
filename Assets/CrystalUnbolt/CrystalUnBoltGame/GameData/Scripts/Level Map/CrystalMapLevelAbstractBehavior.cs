using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalUnbolt.Map
{
    public abstract class CrystalMapLevelAbstractBehavior : MonoBehaviour
    {
        [SerializeField] protected Button button;
        [SerializeField] protected TMP_Text levelNumber;

        public int LevelId { get; protected set; }
        public static event SimpleIntCallback OnLevelClicked;

        protected virtual void Awake()
        {
            button.onClick.AddListener(OnButtonClicked);
        }

        public virtual void Init(int id)
        {
            LevelId = id;
            levelNumber.text = $"{id + 1}";

            if (id < CrystalMapBehavior.MaxLevelReached)
            {
                InitOpen();
            }
            else if (id == CrystalMapBehavior.MaxLevelReached)
            {
                InitCurrent();
            }
            else
            {
                InitClose();
            }
        }

        protected virtual void OnButtonClicked()
        {
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            OnLevelClicked?.Invoke(LevelId);
        }

        protected abstract void InitOpen();
        protected abstract void InitClose();
        protected abstract void InitCurrent();
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace CrystalUnbolt
{
    [CreateAssetMenu(fileName = "Audio Clips", menuName = "Data/Core/Audio Clips")]
    public class AudioClips : ScriptableObject
    {
        [BoxGroup("Gameplay", "Gameplay")]
        public AudioClip screwPick;
        [BoxGroup("Gameplay")]
        public AudioClip screwPlace;
        [BoxGroup("Gameplay")]
        public AudioClip plankComplete;
        [BoxGroup("Gameplay")]
        public AudioClip plankCollision;
        [BoxGroup("Gameplay")]
        public AudioClip tutorialComplete;

        [BoxGroup("Stages", "Stages")]
        public AudioClip levelComplete;
        [BoxGroup("Stages")]
        public AudioClip stageComplete;
        [BoxGroup("Stages")]
        public AudioClip levelFailed;


        [BoxGroup("UI", "UI")]
        public AudioClip buttonSound;
        [BoxGroup("UI")]
        public AudioClip dailyBonusClaim;
    }
}

// -----------------
// Audio Controller v 0.4
// -----------------
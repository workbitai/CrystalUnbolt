using UnityEngine;

namespace CrystalUnbolt
{
    /// <summary>
    /// Your Custom Audio Configuration
    /// Simple sound & music management
    /// </summary>
    [CreateAssetMenu(fileName = "My Audio Config", menuName = "MyGame/Audio Configuration")]
    public class MyAudioConfig : ScriptableObject
    {
        [Header("=== Music ===")]
        public AudioClip backgroundMusic;
        [Range(0f, 1f)] public float musicVolume = 0.7f;
        
        [Header("=== Sound Effects ===")]
        public AudioClip buttonClick;
        public AudioClip screwPick;
        public AudioClip screwPlace;
        public AudioClip plankFall;
        public AudioClip levelComplete;
        public AudioClip levelFailed;
        public AudioClip puzzleCorrect;
        public AudioClip puzzleWrong;
        public AudioClip coinReward;
        
        [Range(0f, 1f)] public float soundVolume = 1f;
        
        [Header("=== Settings ===")]
        public bool musicEnabled = true;
        public bool soundEnabled = true;
    }
}







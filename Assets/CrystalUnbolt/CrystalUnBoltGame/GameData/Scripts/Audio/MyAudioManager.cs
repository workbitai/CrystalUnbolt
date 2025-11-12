using UnityEngine;

namespace CrystalUnbolt
{
    /// <summary>
    /// Your Custom Audio Manager
    /// Simple & Clean audio playback
    /// </summary>
    public class MyAudioManager : MonoBehaviour
    {
        public static MyAudioManager Instance { get; private set; }
        
        [SerializeField] private MyAudioConfig audioConfig;
        
        private AudioSource musicSource;
        private AudioSource soundSource;
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Create audio sources
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            
            soundSource = gameObject.AddComponent<AudioSource>();
            soundSource.playOnAwake = false;
        }
        
        private void Start()
        {
            if (audioConfig != null && audioConfig.musicEnabled)
            {
                PlayMusic();
            }
        }
        
        public void PlayMusic()
        {
            if (audioConfig == null || audioConfig.backgroundMusic == null) return;
            
            musicSource.clip = audioConfig.backgroundMusic;
            musicSource.volume = audioConfig.musicVolume;
            musicSource.Play();
        }
        
        public void StopMusic()
        {
            musicSource.Stop();
        }
        
        public void PlaySound(AudioClip clip)
        {
            if (audioConfig == null || !audioConfig.soundEnabled) return;
            if (clip == null) return;
            
            soundSource.PlayOneShot(clip, audioConfig.soundVolume);
        }
        
        public void PlayButtonClick()
        {
            PlaySound(audioConfig?.buttonClick);
        }
        
        public void PlayScrewPick()
        {
            PlaySound(audioConfig?.screwPick);
        }
        
        public void PlayScrewPlace()
        {
            PlaySound(audioConfig?.screwPlace);
        }
        
        public void PlayPlankFall()
        {
            PlaySound(audioConfig?.plankFall);
        }
        
        public void PlayLevelComplete()
        {
            PlaySound(audioConfig?.levelComplete);
        }
        
        public void PlayLevelFailed()
        {
            PlaySound(audioConfig?.levelFailed);
        }
        
        public void PlayPuzzleCorrect()
        {
            PlaySound(audioConfig?.puzzleCorrect);
        }
        
        public void PlayPuzzleWrong()
        {
            PlaySound(audioConfig?.puzzleWrong);
        }
        
        public void PlayCoinReward()
        {
            PlaySound(audioConfig?.coinReward);
        }
        
        public void SetMusicEnabled(bool enabled)
        {
            if (audioConfig != null)
                audioConfig.musicEnabled = enabled;
                
            if (enabled)
                PlayMusic();
            else
                StopMusic();
        }
        
        public void SetSoundEnabled(bool enabled)
        {
            if (audioConfig != null)
                audioConfig.soundEnabled = enabled;
        }
    }
}


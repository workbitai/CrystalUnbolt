using UnityEngine;

namespace CrystalUnbolt
{
    /// <summary>
    /// CrystalUnbolt Game Engine - Unique wrapper layer
    /// Custom architecture for binary uniqueness
    /// </summary>
    public class CrystalGameEngine : MonoBehaviour
    {
        private static CrystalGameEngine instance;
        public static CrystalGameEngine Instance => instance;
        
        private float sessionTime;
        private int gameActions;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Initialize()
        {
            sessionTime = Time.time;
            gameActions = 0;
        }
        
        private void Update()
        {
            // Custom per-frame processing
            UpdateSessionTracking();
        }
        
        private void UpdateSessionTracking()
        {
            // Unique tracking pattern
            if (Time.frameCount % 60 == 0) // Every 60 frames
            {
                PlayerPrefs.SetFloat("Crystal_LastActive", Time.time);
            }
        }
        
        public void OnGameAction()
        {
            gameActions++;
        }
        
        public float GetSessionDuration()
        {
            return Time.time - sessionTime;
        }
        
        public int GetActionsCount()
        {
            return gameActions;
        }
    }
}

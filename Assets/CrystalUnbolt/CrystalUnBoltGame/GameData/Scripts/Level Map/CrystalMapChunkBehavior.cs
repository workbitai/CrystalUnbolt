using System.Collections.Generic;
using UnityEngine;

namespace CrystalUnbolt.Map
{
    public class CrystalMapChunkBehavior : MonoBehaviour
    {
        [SerializeField] SpriteRenderer background;
        [SerializeField] GameObject bottom;

        [SerializeField] List<CrystalMapLevelBehavior> levels;
        public int LevelsCount => levels.Count;

        public int ChunkId { get; private set; }

        public CrystalMapBehavior Map { get; private set; }
        public float Height => background.size.y * transform.localScale.y;
        public float AdjustedHeight => Height / Map.MapVisibleRectHeight;

        private CrystalMapLevelBehavior FirstDisabledLevel { get; set; }
        public bool HasDisabledLevels => FirstDisabledLevel != null;
        public float FirstDisabledLevelPostion => Position + (FirstDisabledLevel.transform.localPosition.y + Height / 2) / Map.MapVisibleRectHeight;
        public float FirstDisabledLevelLocalPostion => (FirstDisabledLevel.transform.localPosition.y + Height / 2) / Map.MapVisibleRectHeight;

        public float CurrentLevelPosition { get; private set; }
        public float Position { get; private set; }
        public int StartLevelCount { get; private set; }

        public void SetPosition(float y)
        {
            Position = y;
            transform.SetPositionY(y * Map.MapVisibleRectHeight + Height / 2 + Camera.main.transform.position.y - Map.MapVisibleRectHeight / 2);
        }

        public void SetMap(CrystalMapBehavior map)
        {
            Map = map;
        }

        public void Init(int chunkId, int startLevelCount)
        {
            ChunkId = chunkId;
            StartLevelCount = startLevelCount;

            CurrentLevelPosition = -1;

            transform.localScale = Vector3.one * Map.MapVisibleRectWidth / background.size.x;

            bool reachedLastLevel = false;
            FirstDisabledLevel = null;

            for (int i = 0; i < levels.Count; i++)
            {
                CrystalMapLevelBehavior level = levels[i];
                int levelId = startLevelCount + i;

                if (!CrystalGameManager.Data.InfiniteLevels && levelId >= CrystalLevelController.Database.AmountOfLevels)
                {
                    level.gameObject.SetActive(false);

                    if (!reachedLastLevel)
                    {
                        reachedLastLevel = true;
                        FirstDisabledLevel = level;
                    }
                }
                else
                {
                    level.Init(levelId);

                    if (levelId == CrystalMapBehavior.MaxLevelReached)
                    {
                        CurrentLevelPosition = (level.transform.position.y + Height / 2) / Map.MapVisibleRectHeight;
                    }
                }
            }

            background.receiveShadows = true;

            if (bottom != null)
                bottom.SetActive(startLevelCount == 0);

            // ✅ Flip background every alternate chunk
            if (ChunkId % 2 == 1)
            {
                Vector3 scale = background.transform.localScale;
                scale.y = -Mathf.Abs(scale.y);
                background.transform.localScale = scale;
            }
            else
            {
                Vector3 scale = background.transform.localScale;
                scale.y = Mathf.Abs(scale.y);
                background.transform.localScale = scale;
            }
        }


        private void CalculateNarrowScreenScale()
        {
            transform.localScale = Vector3.one * Map.MapVisibleRectWidth / background.size.x;
        }

        private void CalculateWideScreenScale()
        {

        }
    }
}


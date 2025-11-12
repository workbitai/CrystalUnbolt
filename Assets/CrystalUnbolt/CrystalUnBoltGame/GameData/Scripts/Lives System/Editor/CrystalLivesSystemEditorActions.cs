using UnityEditor;
using UnityEngine;

namespace CrystalUnbolt
{
    public static class CrystalLivesSystemEditorActions
    {
        [MenuItem("Actions/Lives System/Full Lives")]
        private static void FullLives()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Action works only in play mode!");

                return;
            }

            CrystalLivesSystem.AddLife(int.MaxValue, false);

            Debug.Log("FullLives action performed");
        }

        [MenuItem("Actions/Lives System/No Lives")]
        private static void NoLives()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Action works only in play mode!");

                return;
            }

            CrystalLivesSystem.TakeLife(int.MaxValue);

            Debug.Log("NoLives action performed");
        }

        [MenuItem("Actions/Lives System/Take Life")]
        private static void TakeLife()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Action works only in play mode!");

                return;
            }

            CrystalLivesSystem.TakeLife();

            Debug.Log("TakeLife action performed");
        }

        [MenuItem("Actions/Lives System/Add Life")]
        private static void AddLife()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Action works only in play mode!");

                return;
            }

            CrystalLivesSystem.AddLife();

            Debug.Log("AddLife action performed");
        }

        [MenuItem("Actions/Lives System/Show Add Life Panel")]
        private static void ShowAddLifePanel()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Action works only in play mode!");

                return;
            }

            if (!CrystalUIAddLivesPanel.Exists())
            {
                Debug.Log("CrystalUIAddLivesPanel page doesn't exist!");

                return;
            }

            CrystalUIAddLivesPanel.Show((bool rewardedVideoWatched) =>
            {
                Debug.Log("Panel Closed; RV watched: " + rewardedVideoWatched);
            });
        }

        [MenuItem("Actions/Lives System/Enable Infinite Mode (30 seconds)")]
        private static void EnableInfiniteMode()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Action works only in play mode!");

                return;
            }

            CrystalLivesSystem.EnableInfiniteMode(5);

            Debug.Log("EnableInfiniteMode action performed");
        }

        [MenuItem("Actions/Lives System/Disable Infinite Mode")]
        private static void DisableInfiniteMode()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Action works only in play mode!");

                return;
            }

            CrystalLivesSystem.DisableInfiniteMode();

            Debug.Log("DisableInfiniteMode action performed");
        }
    }
}

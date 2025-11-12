using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

namespace CrystalUnbolt
{
    public static class GameLoading
    {
        private const float MINIMUM_LOADING_TIME = 2.0f;

        private static AsyncOperation loadingOperation;

        private static bool isReadyToHide;
        private static bool manualControlMode;

        private static string loadingMessage;
        private static List<LoadingTask> loadingTasks = new List<LoadingTask>();

        public static event LoadingCallback OnLoading;
        public static event Action OnLoadingFinished;

        public static void SetLoadingMessage(string message)
        {
            loadingMessage = message;

            float progress = 0.0f;
            if (loadingOperation != null)
                progress = loadingOperation.progress;

            OnLoading?.Invoke(progress, message);
        }

        public static void AddTask(LoadingTask loadingTask)
        {
            loadingTasks.Add(loadingTask);
        }

        private static IEnumerator LoadSceneCoroutine(GameCallback onSceneLoaded = null)
        {
            isReadyToHide = false;

            float realtimeSinceStartup = Time.realtimeSinceStartup;

            int taskIndex = 0;
            while(taskIndex < loadingTasks.Count)
            {
                if(!loadingTasks[taskIndex].IsActive)
                    loadingTasks[taskIndex].Activate();

                if (loadingTasks[taskIndex].IsFinished)
                {
                    taskIndex++;
                }

                yield return null;
            }

            int sceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (SceneManager.sceneCount < sceneIndex)
                Debug.LogError("[Loading]: First scene is missing!");

            float minimumFinishTime = realtimeSinceStartup + MINIMUM_LOADING_TIME;

            loadingOperation = SceneManager.LoadSceneAsync(sceneIndex);
            loadingOperation.allowSceneActivation = false;

            while (!loadingOperation.isDone || realtimeSinceStartup < minimumFinishTime)
            {
                yield return null;

                realtimeSinceStartup = Time.realtimeSinceStartup;

                OnLoading?.Invoke(1.0f, loadingMessage);

                if (loadingOperation.progress >= 0.9f)
                {
                    // Wait for 0.3–0.5 sec before activating new scene
                    yield return new WaitForSecondsRealtime(0.5f);
                    loadingOperation.allowSceneActivation = true;
                }
            }

            if(manualControlMode)
            {
                // Debug check if MarkAsReadyToHide is implemented
                Tween.DelayedCall(10, () =>
                {
                    if (!isReadyToHide)
                        Debug.LogError("[Loading]: Seems like you forget to call MarkAsReadyToHide method to finish the loading process.");
                });

                while (!isReadyToHide)
                {
                    yield return null;
                }
            }

            OnLoading?.Invoke(1.0f, "Done");

            yield return null;

            if (onSceneLoaded != null)
                onSceneLoaded.Invoke();

            OnLoadingFinished?.Invoke();
        }

        private static IEnumerator SimpleLoadCoroutine(GameCallback onSceneLoaded = null)
        {
            float realtimeSinceStartup = Time.realtimeSinceStartup;

            int taskIndex = 0;
            while (taskIndex < loadingTasks.Count)
            {
                if (!loadingTasks[taskIndex].IsActive)
                    loadingTasks[taskIndex].Activate();

                if (loadingTasks[taskIndex].IsFinished)
                {
                    taskIndex++;
                }

                yield return null;
            }

            if (onSceneLoaded != null)
                onSceneLoaded.Invoke();

            OnLoadingFinished?.Invoke();
        }

        public static void MarkAsReadyToHide()
        {
            isReadyToHide = true;
        }

        public static void EnableManualControlMode()
        {
            manualControlMode = true;
        }

        public static void LoadGameScene(GameCallback onSceneLoaded = null)
        {
            SetLoadingMessage("Loading..");

            Tween.InvokeCoroutine(LoadSceneCoroutine(onSceneLoaded));
        }

        public static void SimpleLoad(GameCallback onSceneLoaded = null)
        {
            Tween.InvokeCoroutine(SimpleLoadCoroutine(onSceneLoaded));
        }

        public delegate void LoadingCallback(float state, string message);
    }
}
using UnityEngine.SceneManagement;

namespace CoreCraft.Core
{
    public static class Loader
    {
        private static string _targetSceneName;

        public static void Load(string targetSceneName)
        {
            // Set target scene to load and load the loading scene beforehand.
            _targetSceneName = targetSceneName;

            SceneManager.LoadScene("loading_scene");
        }

        public static void LoaderCallback()
        {
            // Load actual game scene on first update call.
            SceneManager.LoadScene(_targetSceneName);
        }
    }
}
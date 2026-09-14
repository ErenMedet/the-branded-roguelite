using UnityEngine.SceneManagement;

namespace Branded.Meta
{
    // Scene changes go through here so progress is always on disk first.
    public static class SceneFlow
    {
        public const string Hub = "Hub_GodotForge";
        public const string Run = "Greybox_Asama1";

        public static void Load(string scene)
        {
            Progress.Save();
            SceneManager.LoadScene(scene);
        }
    }
}

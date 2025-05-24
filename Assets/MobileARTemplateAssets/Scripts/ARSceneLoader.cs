using UnityEngine;
using UnityEngine.SceneManagement;

public class ARSceneLoader : MonoBehaviour
{
    public string sceneName = "ARScene"; // Geçilecek sahne ismi

    public void LoadARScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}

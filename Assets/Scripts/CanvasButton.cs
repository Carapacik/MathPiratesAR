using UnityEngine;
using UnityEngine.SceneManagement;

public class CanvasButton : MonoBehaviour
{
    public void SelectScene(bool isHeartScene)
    {
        ScenesInfo.IsHeartScene = isHeartScene;
        SceneManager.LoadScene(1);
    }
}
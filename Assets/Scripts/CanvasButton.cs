using UnityEngine;
using UnityEngine.SceneManagement;

public class CanvasButton : MonoBehaviour
{
    public void SelectScene(bool isHeartScene)
    {
        ScenesInfo.CrossSceneInformation = isHeartScene;
        SceneManager.LoadScene("ShipScene");
    }
}
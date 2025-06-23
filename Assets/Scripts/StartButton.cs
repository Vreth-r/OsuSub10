using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    public void LoadGame()
    {
        FadeController.TransitionToScene("SongSelect");
    }
}

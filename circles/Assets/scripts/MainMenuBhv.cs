using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuBhv : MonoBehaviour
{
    public void OnPlayClick()
    {
        SceneManager.LoadScene(1);
    }

    public void OnWatchClick()
    {
        SceneManager.LoadScene(2);
    }

    public void OnExitClick()
    {
        #if UNITY_STANDALONE
            Application.Quit();
        #endif
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}

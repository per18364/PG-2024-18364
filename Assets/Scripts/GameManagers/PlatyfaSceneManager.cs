using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class PlatyfaSceneManager : MonoBehaviour
{
    private static PlatyfaSceneManager _instance = null;
    public static PlatyfaSceneManager Instance
    {
        get
        {
            if (_instance == null)
                Debug.LogError("PlatyfaSceneManager.cs error: The instance is Null");            

            return _instance;
        }
    }

    [SerializeField] private string webPageUrl;

    private void Awake() 
    {
        _instance = this;
    }

    public void CreditsInitial()
    {
        SceneManager.LoadScene("CreditsInitial");
    }

    public void LoadLevel1Prototype()
    {
        SceneManager.LoadScene("Level1");
        AudioManager.Instance?.InitializeMusic(FMODEvents.instance.musicLevel1);
    }

    public void LoadLevel2Prototype()
    {
        SceneManager.LoadScene("Level2");
        AudioManager.Instance?.InitializeMusic(FMODEvents.instance.musicBossOwlLevel2);
    }

    public void LogoScreen()
    {
        SceneManager.LoadScene("LogoScreen");
        AudioManager.Instance?.InitializeMusic(FMODEvents.instance.musicMainTitle);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void SettingsMenu()
    {
        SceneManager.LoadScene("Settings");
    }

    public void ControllersScene() {
        SceneManager.LoadScene("Controllers");
    }

    public void AccountSelectionScreen() 
    {
        SceneManager.LoadScene("AccountSelection");
    }

    public void RestartCurrentScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
        AudioManager.Instance?.StartMusicManager(scene.name);
    }

    public void VisitWebPage()
    {
        Application.OpenURL(webPageUrl);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

}

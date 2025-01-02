using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMng_logo : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.anyKey)
        {
            AudioManager.Instance?.PlayUISound(FMODEvents.instance.startSound);
            SceneManager.LoadScene("MainMenu");
            AudioManager.Instance?.InitializeMusic(FMODEvents.instance.musicMainMenu);
        }
    }
}

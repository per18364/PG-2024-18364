using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Credits : MonoBehaviour
{
    void Start()
    {
        StartCoroutine("CreditsEscene");
    }

    IEnumerator CreditsEscene()
    {                
        yield return new WaitForSeconds(11f);
        PlatyfaSceneManager.Instance.LogoScreen();
    }
}

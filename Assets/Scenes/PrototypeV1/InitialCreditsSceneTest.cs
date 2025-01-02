using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialCreditsSceneTest : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(GoToTheGame());
    }
    
    private IEnumerator GoToTheGame()
    {
        yield return new WaitForSeconds(5f);
        PlatyfaSceneManager.Instance.CreditsInitial();
    }
}

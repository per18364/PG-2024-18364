using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerSlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI username;
    [SerializeField] private TextMeshProUGUI email;
    [SerializeField] private TextMeshProUGUI chapter;
    [SerializeField] private TextMeshProUGUI time;

    public void Init(string usernameText, string emailText)
    {
        username.text = usernameText;
        email.text = emailText;
        chapter.text = "Capítulo 1";
        time.text = "N/A";
    }

    public void StartGame()
    {
        PlayerPrefs.SetString("currentPlayer", username.text);
        AudioManager.Instance?.PlayUISound(FMODEvents.instance.startSound);
        PlatyfaSceneManager.Instance?.LoadLevel1Prototype();
    }
}

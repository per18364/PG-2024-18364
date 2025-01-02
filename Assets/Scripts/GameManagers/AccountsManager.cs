using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccountsManager : MonoBehaviour
{
    [SerializeField] private GameObject _prefabPlayerSlot;

    private void Start() 
    {
        foreach(KeyValuePair<string, PlayerSesionData> player in SesionData.Instance.PlayersSessions)
        {            
            CreateSlotPlayer(player.Key, player.Value.userEmail);
        }
    }    

    public void CreateSlotPlayer(string usernameText, string email)
    {
        GameObject newSlot = Instantiate(_prefabPlayerSlot, transform);
        PlayerSlot playerSlot = newSlot.GetComponent<PlayerSlot>();
        playerSlot.Init(usernameText, email);
    }
}

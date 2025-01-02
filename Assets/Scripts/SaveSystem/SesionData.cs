using System.Collections.Generic;
using UnityEngine;

public class SesionData : MonoBehaviour
{
    private static SesionData _instance;
    public static SesionData Instance { get => _instance; }

    private void Awake() 
    {
        GetPlayerData();

        if(_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }

        Destroy(gameObject);
    }
        
    [SerializeField] private Dictionary<string, PlayerSesionData> _playersSessions;

    public Dictionary<string, PlayerSesionData> PlayersSessions { get { return _playersSessions; } }
    
    public PlayerSesionData getCurrentPlayerSession()
    {
        if(!PlayerPrefs.HasKey("currentPlayer"))
        {
            return null;
        }

        string currentPlayer = PlayerPrefs.GetString("currentPlayer");

        if(!_playersSessions.ContainsKey(currentPlayer))
        {
            Debug.LogWarning("No player is currently logged in");
            return null;
        }
        
        return _playersSessions[currentPlayer];
    }

    private void GetPlayerData() 
    {                
        GameData gameData = SaveManager.LoadPlayerData();

        if(gameData == null)
        {
            _playersSessions = new Dictionary<string, PlayerSesionData>();
            return;
        }

        _playersSessions = gameData.sesionsData;
        
        if(_playersSessions.Count == 0)
        {
            Debug.Log("Data is empty");
            return;
        }

        foreach(KeyValuePair<string, PlayerSesionData> player in _playersSessions)
        {
            Debug.Log($"Player ID: {player.Key}, User Name: {player.Value.userName}, User Email: {player.Value.userEmail}");
        }
    }

    public bool AddNewPlayerReady(string userName, string userEmail, string token)
    {        
        if(_playersSessions.ContainsKey(userName))
        {            
            _playersSessions[userName] = new PlayerSesionData(userEmail, userEmail, token);
            PlayerPrefs.SetString("currentPlayer", userName);
            return false;
        }
        
        PlayerSesionData newPlayer = new PlayerSesionData(userName, userEmail, token);
        _playersSessions.Add(userName, newPlayer);
        PlayerPrefs.SetString("currentPlayer", userName);
        return true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.Post.html

[System.Serializable]
public class LoginData
{
    public string username;
    public string password;
}

[System.Serializable]
public class RegisterData
{
    public string username;
    public string name;
    public string email;    
    public string password;
    public string country;
    public string language;
    public string game_lenguage;
}

[System.Serializable]
public class PlayerData
{
    public string level;
    public int duration_level;
    public string game_result;
    public int kills;
    public int jumps;
    public int damage_received;
    public int frequency_barringtonia;
    public int frequency_spaghetti;
    public int frequency_jelly;
    public int frequency_hot_tea;
    public int frequency_cake;
    public int frequency_melee_attack;
    public int impact_barringtonia;
    public int impact_spaggetti;
    public int impact_jelly;
    public int impact_hot_tea;
    public int impact_cake;
    public int impact_melee_attack;
}

[System.Serializable]
public class ApiLoginResponse
{
    public string success;
    public string token;
}

public class ApiController : MonoBehaviour
{
    private static ApiController _instance;
    public static ApiController Instance
    {
        get
        {
            if (_instance == null)
                Debug.LogError("ApiController.cs error: The instance is Null");

            return _instance;
        }
    }

    #region Variables

    [SerializeField] private ApiConfig _apiConfig;
    private bool IsUrlEmpty () => string.IsNullOrEmpty(_apiConfig.apiUrl);
    private bool IsUserEmpty () => string.IsNullOrEmpty(PlayerPrefs.HasKey("currentPlayer") ? PlayerPrefs.GetString("currentPlayer") : null);

    #endregion

    private void Awake() 
    {
        _instance = this;
    }

    private void Start()
    {
        if (IsUrlEmpty())
        {
            Debug.LogError("Failed to retrieve API URL");
        }

        if(IsUserEmpty())
        {
            Debug.Log("No user logged in");
        }
    }

    #region Login Method

    public void Login(string username, string password, Action<Dictionary<string, string>> onResponse)
    {
        if(IsUrlEmpty())
        {
            Debug.LogError("Failed to retrieve API URL");
            return;
        }
        
        LoginData loginData = new LoginData
        {
            username = username,
            password = password
        };

        string jsonString = JsonUtility.ToJson(loginData);

        StartCoroutine(CallEndpoint("api/players/login", jsonString, onResponse));
    }
    
    # endregion

    #region Register Method

    public void Register(RegisterData registerData, Action<Dictionary<string, string>> onResponse) // Create New User
    {        
        if(IsUrlEmpty())
        {
            Debug.LogError("Failed to retrieve API URL");
            return;
        }
        
        string jsonString = JsonUtility.ToJson(registerData);

        StartCoroutine(CallEndpoint("api/players/register", jsonString, onResponse));
    }

    private IEnumerator CallEndpoint(string endpointName, string jsonPayload, Action<Dictionary<string, string>> onResponse)
    {
        UnityWebRequest request = new UnityWebRequest(_apiConfig.apiUrl + endpointName, "POST");
        byte[] body = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string response = request.downloadHandler.text; 

            if (string.IsNullOrEmpty(response))
            {
                Debug.LogError("Response is null or empty.");
                onResponse?.Invoke(null);
                yield break;
            }           
            
            try
            {
                Dictionary<string, string> responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
                onResponse?.Invoke(responseData);
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to parse JSON: " + ex.Message);
                onResponse?.Invoke(null);
            }
        }
        else
        {
            Debug.LogError("Request failed: " + request.error);
            onResponse?.Invoke(null);
        }

        request.Dispose();
    }

    # endregion

    #region Save Game Session Method

    public void SaveGameSession(Dictionary<PlayerDataEnum, int> sessionData, LevelEnum currentLevel, LevelResultEnum levelResult)
    {
        if(IsUrlEmpty())
        {
            Debug.LogError("Failed to retrieve API URL");
            return;
        }
        
        if(IsUserEmpty())
        {
            Debug.Log("User was not founded. Please login or register first");
            return;
        }        
        
        PlayerData playerData = new PlayerData
        {
            level = currentLevel.ToString(),
            duration_level = sessionData[PlayerDataEnum.minutes],
            game_result = levelResult.ToString(),
            kills = sessionData[PlayerDataEnum.kills],
            jumps = sessionData[PlayerDataEnum.jumps],
            damage_received = sessionData[PlayerDataEnum.damage_received],
            frequency_barringtonia = sessionData[PlayerDataEnum.frequency_barringtonia],
            frequency_spaghetti = sessionData[PlayerDataEnum.frequency_spaggetti],
            frequency_jelly = sessionData[PlayerDataEnum.frequency_jelly],
            frequency_hot_tea = sessionData[PlayerDataEnum.frequency_hot_tea],
            frequency_cake = sessionData[PlayerDataEnum.frequency_cake],
            frequency_melee_attack =sessionData[PlayerDataEnum.frequency_melee_attack],
            impact_barringtonia = sessionData[PlayerDataEnum.impact_barringtonia],
            impact_spaggetti = sessionData[PlayerDataEnum.impact_spaggetti],
            impact_jelly = sessionData[PlayerDataEnum.impact_jelly],
            impact_hot_tea = sessionData[PlayerDataEnum.impact_hot_tea],
            impact_cake = sessionData[PlayerDataEnum.impact_cake],
            impact_melee_attack = sessionData[PlayerDataEnum.impact_melee_attack],
        };

        string jsonString = JsonUtility.ToJson(playerData);

        StartCoroutine(CallEndpointSaveSession(jsonString));
    }

    private IEnumerator CallEndpointSaveSession(string jsonPayload)
    {                
        UnityWebRequest request = new UnityWebRequest(_apiConfig.apiUrl + "api/players/game_sessions", "POST");

        byte[] body = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        PlayerSesionData sessionData = SesionData.Instance.getCurrentPlayerSession();

        request.SetRequestHeader("Authorization", "Bearer " + sessionData.token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string response = request.downloadHandler.text;
            Debug.Log("Response: " + response);
        }
        else
        {
            Debug.LogError("Request failed: " + request.error);
        }

        request.Dispose();
    }

    # endregion
}

// https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.Post.html
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputHandler : MonoBehaviour
{        
    [Header("Create User Input Handlers")]
    [SerializeField] private TMP_InputField usernameInputCreate;
    [SerializeField] private TMP_InputField emailInputCreate;
    [SerializeField] private TMP_InputField passwordInputCreate;
    
    [Header("Login User Input Handlers")]
    [SerializeField] private TMP_InputField usernameInputLogin;
    [SerializeField] private TMP_InputField passwordInputLogin;

    [Header("UI Elements")]
    [SerializeField] private GameObject AccountCreationScreen;
    [SerializeField] private GameObject AccountLoginScreen;
    [SerializeField] private GameObject AccountScreen_ERROR;
    [SerializeField] private GameObject AccountScreen_SUCCESS;
    [SerializeField] private TextMeshProUGUI successText;
    [SerializeField] private TextMeshProUGUI buttonSuccessText;

    [Header("Additional Screens")]
    [SerializeField] private GameObject loadingScreen;

    [Header("First Buttons List")]
    [SerializeField] private GameObject firstCampForAccountSelection;
    [SerializeField] private GameObject firstCampForLogin;
    [SerializeField] private GameObject firstCampForRegister;

    // Start is called before the first frame update
    void Start()
    {
        AccountCreationScreen.SetActive(false);
        AccountLoginScreen.SetActive(false);
        AccountScreen_SUCCESS.SetActive(false);
        AccountScreen_ERROR.SetActive(false);
        loadingScreen.SetActive(false);
    }

    public void ToggleAccountSelection() 
    {
        AccountCreationScreen.SetActive(!AccountCreationScreen.activeSelf);
        if(AccountCreationScreen.activeSelf)
        {
            // Limpiar el objeto seleccionado
            EventSystem.current.SetSelectedGameObject(null);
            // Implementar un nuevo objeto seleccionado
            EventSystem.current.SetSelectedGameObject(firstCampForRegister);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstCampForAccountSelection);
        }
        AccountScreen_SUCCESS.SetActive(false);
        AccountScreen_ERROR.SetActive(false);
    }

    public void ToggleLoginAccount()
    {
        AccountLoginScreen.SetActive(!AccountLoginScreen.activeSelf);
        if(AccountLoginScreen.activeSelf)
        {
            // Limpiar el objeto seleccionado
            EventSystem.current.SetSelectedGameObject(null);
            // Implementar un nuevo objeto seleccionado
            EventSystem.current.SetSelectedGameObject(firstCampForLogin);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstCampForAccountSelection);
        }
        AccountScreen_SUCCESS.SetActive(false);
        AccountScreen_ERROR.SetActive(false);
    }

    public void CreateAccount() 
    {
        string userName =  usernameInputCreate.text;
        string userPassword = passwordInputCreate.text;
        string userEmail = emailInputCreate.text;

        if(userName.Trim() == "" || userPassword.Trim() == "" || userEmail.Trim() == "")
        {
            return; // Will be not allowed userName or passowrd empty.
        }

        loadingScreen.SetActive(true);
        
        RegisterData newUser = new RegisterData
        {
            username = userName,
            name = userName,
            email = userEmail,
            password = userPassword,
            country = "GT",
            language = "Spanish",
            game_lenguage = "Spanish",
        };

        ApiController.Instance.Register(newUser, (response) => {
            loadingScreen.SetActive(false);

            if(response == null)
            {
                Debug.LogError("Login failed or response is null.");
                AccountScreen_ERROR.SetActive(true);
                return;
            }

            bool sucess = bool.Parse(response["success"]);

            if(!sucess)
            {
                AccountScreen_ERROR.SetActive(true);
                return;
            }

            successText.text = "¡Cuenta Creada con Éxito! Ahora porfavor inicia sesión";
            buttonSuccessText.text = "Continuar";
            AccountScreen_SUCCESS.SetActive(true);
        });    
    }

    public void UserLogin()
    {
        string userName = usernameInputLogin.text;
        string userPassword = passwordInputLogin.text;

        if(userName.Trim() == "" || userPassword.Trim() == "")
        {
            return; // Will be not allowed userName or passowrd empty.
        }
        
        loadingScreen.SetActive(true);

        ApiController.Instance.Login(userName, userPassword, (response) => 
        {
            loadingScreen.SetActive(false);

            if(response == null)
            {
                Debug.LogError("Login failed or response is null.");
                AccountScreen_ERROR.SetActive(true);
                return;
            }
            
            bool sucess = bool.Parse(response["success"]);
            string token = response["token"];

            if(!sucess)
            {
                AccountScreen_ERROR.SetActive(true);
                return;
            }
            
            bool isPlayerAdded = SesionData.Instance.AddNewPlayerReady(userName, "LOGGED USER", token);

            if(isPlayerAdded)
                successText.text = "¡Jugador logeado y registrado con éxito!";
            else
                successText.text = "El jugador ya fue logeado, datos actualizados";

            buttonSuccessText.text = "¡A Jugar!";
            SaveManager.SavePlayerData();
            AccountScreen_SUCCESS.SetActive(true);
        });
    }    
}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public abstract class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
                Debug.LogError("GameManager.cs error: The instance is Null");

            return _instance;
        }
    }

    [Header("Game Manager Properties")]
    protected Dictionary<PlayerDataEnum, int> sessionData = new Dictionary<PlayerDataEnum, int>();
    [SerializeField] private GameObject _gameOverUIPanel;
    [SerializeField] private GameObject _stageClearUIPanel;
    [SerializeField] private GameObject _HUD;

    protected LevelEnum _currentLevel;
    [SerializeField] private GameObject _gameOverUIPanelObjects;
    [SerializeField] private LevelEnum currentLevel;
    private bool _playing = true;

    private bool _dataSaved;
    [SerializeField] private GameObject _pauseMenuPanel;
    [SerializeField] private GameObject firstButtonGameOver;
    private bool _isPaused = false;

    public bool Playing { get { return _playing; }}
    public bool Paused { get { return _isPaused;}}

    private void Awake() 
    {
        Time.timeScale = 1.0f;
        _instance = this;

        foreach (PlayerDataEnum playerdataEnum in Enum.GetValues(typeof(PlayerDataEnum)))
        {
            sessionData.Add(playerdataEnum, 0);
        }
    }

    private void Start()
    {
        _dataSaved = false;

        if(_gameOverUIPanel == null)
        {
            Debug.LogError("GameManager.cs: Game Over UI Panel is Null.");
        } else
        {
            _gameOverUIPanel.SetActive(false);
        }

        if (_pauseMenuPanel == null)
        {
            Debug.LogError("GameManager.cs: Pause Menu UI Panel is Null");
        } else
        {
            _pauseMenuPanel.SetActive(false);
        }

        if (_stageClearUIPanel == null) {
            Debug.LogError("GameManager.cs: Stage Clear Panel is Null");
        } else {
            _stageClearUIPanel.SetActive(false);
        }

        if (_HUD == null) {
            Debug.LogError("GameManager.cs: Stage Clear Panel is Null");
        } else {
            _HUD.SetActive(true);
        }

        Initialize();
    }

    public virtual void Update()
    {
        if (Input.GetButtonDown("StartButton") && _playing)
        {
            PauseGame();
        }

        if(!_playing && _stageClearUIPanel.activeSelf && (
                Input.GetButtonDown("StartButton") || Input.GetButtonDown("Jump")
            )
        )
        {
            PlatyfaSceneManager.Instance.LoadLevel2Prototype();
        }
    }

    public void PauseGame()
    {
        if (_isPaused)
        {
            // continue playing
            _isPaused = false;
            Time.timeScale = 1.0f;
            _pauseMenuPanel.SetActive(false);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            // pause game
            _isPaused = true;
            Time.timeScale = 0;
            _pauseMenuPanel.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void GameOver()
    {
        if(!_playing) return;
        
        Debug.Log("GAME OVER");
        _playing = false;

        _gameOverUIPanel.SetActive(true);
        RectTransform _maskRectTransform = _gameOverUIPanel.transform.Find("Mask")?.GetComponent<RectTransform>();

        if (_maskRectTransform != null)
            _maskRectTransform.LeanScale(new Vector3(0, 0, 0), 2f);
        
        Transform objectsPanelTransform = _gameOverUIPanel.transform.Find("ObjectsPanel");
        StartCoroutine(TemporalGameOver());    
        AudioManager.Instance?.InitializeMusic(FMODEvents.instance.musicGameOver);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstButtonGameOver);
    }

    public void StageClear() {
        Debug.Log("STAGE CLEAR");
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        _playing = false;

        _HUD.SetActive(false);
        _stageClearUIPanel.SetActive(true);

        RectTransform potRectTransform = _stageClearUIPanel.transform.Find("pot")?.GetComponent<RectTransform>();
        RectTransform pipesTopRectTransform = _stageClearUIPanel.transform.Find("pipesTop")?.GetComponent<RectTransform>();
        GameObject titleObject = _stageClearUIPanel.transform.Find("Title")?.gameObject;
        RectTransform titleRectTransform = _stageClearUIPanel.transform.Find("Title")?.GetComponent<RectTransform>();
        RectTransform timerRectTransform = _stageClearUIPanel.transform.Find("timer")?.GetComponent<RectTransform>();

        if (potRectTransform != null && pipesTopRectTransform != null)
        {
            // Posiciones finales
            Vector3 potTargetPos = new Vector3(0, 0, 0); // Top-left corner
            Vector3 pipesTopTargetPos = new Vector3(0, 0, 0); // Bottom-right corner

            LeanTween.move(potRectTransform, potTargetPos, 1f).setEase(LeanTweenType.easeInOutQuad);
            LeanTween.move(pipesTopRectTransform, pipesTopTargetPos, 1f).setEase(LeanTweenType.easeInOutQuad);
        }

        if (titleRectTransform != null && timerRectTransform != null) {
            // Animate Title sliding in from the right
            LeanTween.move(titleRectTransform, Vector3.zero, 1f).setEase(LeanTweenType.easeOutQuad);
            LeanTween.move(timerRectTransform, Vector3.zero, 1f).setEase(LeanTweenType.easeOutQuad);
        }

        StartCoroutine(ProvisionalThanksScene());
    }

    private IEnumerator ProvisionalThanksScene()
    {
        yield return new WaitForSeconds(70f);
        PlatyfaSceneManager.Instance.LoadLevel2Prototype();
    }

    protected abstract void Initialize();

    private IEnumerator TemporalGameOver()
    {
        // _gameOverUIPanelObjects.SetActive(true);        
        _gameOverUIPanelObjects.SetActive(true);  
        yield return new WaitForSeconds(3f);
        _gameOverUIPanelObjects.transform.Find("HatAndGoogles")?.gameObject.SetActive(true); 
        // Time.timeScale = 0.0f;
        RectTransform gameOverTitle = _gameOverUIPanelObjects.transform.Find("GameOverText")?.gameObject.GetComponent<RectTransform>(); 
        if (gameOverTitle != null) {
            LeanTween.move(gameOverTitle, new Vector3(390, 110, 0f), 1f).setEase(LeanTweenType.easeOutQuad);
        }
        yield return new WaitForSeconds(1.5f);
        _gameOverUIPanelObjects.transform.Find("GameOverButtons")?.gameObject.SetActive(true); 

        // GameObject btnRestart = _gameOverUIPanelObjects.transform.Find("Restart Btn")?.gameObject;
        // TextMeshProUGUI btnRestartText = btnRestart?.GetComponentInChildren<TextMeshProUGUI>();
        // if (btnRestart != null) {
        //     LeanTween.move(btnRestart, targetPos, 1f).setEase(LeanTweenType.easeOutQuad); // Move to position
        //     LeanTween.alphaCanvas(buttonCanvasGroup, 1f, 1f).setEase(LeanTweenType.easeOutQuad);

        // }

        // GameObject btnQuit = _gameOverUIPanelObjects.transform.Find("Quit Level Btn")?.gameObject;
        // if (btnQuit != null) {

        // }
        // SceneManager.LoadScene("Level1");

    }
    
    public void PlayerDataUpdate(PlayerDataEnum playerData)
    {
        sessionData[playerData] = sessionData[playerData] + 1;
        // print(playerData.ToString() + " : " + sessionData[playerData]);
    }

    public void ProcessResultsAndSaveGameSession(LevelResultEnum levelResult)
    {                
        if(_dataSaved) 
        {
            print("The data was saved, you cant save two times in the same session");
            return;
        }

        // Call the endpoint and save the recolected data
        print("The data will be saved " + levelResult.ToString() + " in " + _currentLevel.ToString());
        
        if(ApiController.Instance != null) ApiController.Instance.SaveGameSession(sessionData, _currentLevel, levelResult);

        _dataSaved = true;
    }
}

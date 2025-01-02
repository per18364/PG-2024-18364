///#########################################################################################
/// <author> Cristian Laynez </author>
/// <copyright> Copyright 2024, © Goldenfy Games </copyright>
/// <version> 1.0.0 </version>
/// <license> GPL </license>
/// <maintainer> Goldenfy Games </maintainer>
/// <status> Prototiping </status>
/// 
/// <class> Audio Maneger </class>
/// <summary>
/// Script for get the control of all the sounds from the Audio/Resources file.
/// </summary>
///#########################################################################################

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    [Header("Volume")]
    [Range(0, 1)] [SerializeField] private float masterVolume = 1;
    [Range(0, 1)] [SerializeField] private float musicVolume = 1;
    [Range(0, 1)] [SerializeField] private float ambienceVolume = 1;
    [Range(0, 1)] [SerializeField] private float SFXVolume = 1;

    // =======================================================
    // All the buses for the music
    private Bus masterBus;
    private Bus musicBus;
    // private Bus ambienceBus;
    private Bus sfxBus;
    
    private List<EventInstance> _eventInstances;
    private List<StudioEventEmitter> _eventEmitters;
    private EventInstance _ambienceEventInstance;
    private EventInstance _musicEventInstance;
    
    public static AudioManager Instance { get; private set; }
    
    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        // Instantiate The Audio Manager
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Events Instances
        _eventInstances = new List<EventInstance>();
        _eventEmitters = new List<StudioEventEmitter>();

        // bus:/Music
        masterBus = RuntimeManager.GetBus("bus:/");
        musicBus = RuntimeManager.GetBus("bus:/Music");
        // ambienceBus = RuntimeManager.GetBus("bus:/Ambience");
        sfxBus = RuntimeManager.GetBus("bus:/SFX");     

        Scene scene = SceneManager.GetActiveScene();
        StartMusicManager(scene.name);
    }

    public void StartMusicManager(string sceneName)
    {                
        StopMusic();
        
        if(sceneName == "Level1")
        {
            InitializeMusic(FMODEvents.instance.musicLevel1);
        }

        if(sceneName == "Level2")
        {
            InitializeMusic(FMODEvents.instance.musicBossOwlLevel2);
        }

        if(sceneName == "LogoScreen" || sceneName == "CreditsInitial")
        {
            InitializeMusic(FMODEvents.instance.musicMainTitle); 
        }

        if(sceneName == "MainMenu" || sceneName == "AccountSelection" || sceneName == "Settings")
        {
            InitializeMusic(FMODEvents.instance.musicMainMenu); 
        }
    }

    private void Update()
    {
        masterBus.setVolume(masterVolume);
        musicBus.setVolume(musicVolume);
        // ambienceBus.setVolume(ambienceVolume);
        sfxBus.setVolume(SFXVolume);
    }

    // private void InitializeAmbience(EventReference ambienteEventReference) // ! I Think this should need to be public
    // {
    //     _ambienceEventInstance = CreateEventInstance(ambienteEventReference);
    //     _ambienceEventInstance.start();
    // }

    public void InitializeMusic(EventReference musicEventReference)
    {
        StopMusic();
        _musicEventInstance = CreateEventInstance(musicEventReference);
        _musicEventInstance.start();
    }

    public void SetAmbienceParameter(string parameterName, float parameterValue)
    {
        _ambienceEventInstance.setParameterByName(parameterName, parameterValue);
    }

    public void SetMusicArea(MusicArea area)
    {
        _musicEventInstance.setParameterByName("Stage", (float)area);
    }

    public void StopMusic()
    {
        _musicEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _musicEventInstance.release();
    }

    public void PlayOneShot(EventReference sound, Vector3 worldPos)
    {
        RuntimeManager.PlayOneShot(sound, worldPos);
    }

    public void PlayUISound(EventReference sound)
    {
        if (sound.IsNull)
        {
            Debug.LogWarning("AudioManager: Attempted to play a UI sound, but the EventReference is null.");
            return;
        }

        RuntimeManager.PlayOneShot(sound);
    }

    public EventInstance CreateEventInstance(EventReference eventReference)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        _eventInstances.Add(eventInstance);
        return eventInstance;
    }

    public StudioEventEmitter InitializeEventEmitter(EventReference eventReference, GameObject emitterGameObject)
    {
        StudioEventEmitter emitter = emitterGameObject.GetComponent<StudioEventEmitter>();
        emitter.EventReference = eventReference;
        _eventEmitters.Add(emitter);
        return emitter;
    }

    // private void CleanUp()
    // {
    //     foreach (EventInstance eventInstance in _eventInstances)
    //     {
    //         eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    //         eventInstance.release();
    //     }

    //     foreach (StudioEventEmitter emitter in _eventEmitters)
    //     {
    //         emitter.Stop();
    //     }
    // }

    // private void OnDestroy()
    // {
    //     // CleanUp();
    // }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public struct InstructionForTutorial
{
    public GameObject popUpUI;
    public UnityEvent eventToFullfill;
}

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial Scential Elements")]
    // [SerializeField] private GameObject[] popUps;
    [SerializeField] private InstructionForTutorial[] popUps;
    [SerializeField] private bool startInBeggining = true;
    [SerializeField] private ThirdPersonMovement player;
    [SerializeField] private FoodBox foodBox;
    private int _popUpIndex;    

    // Rules to Set
    [Header("For Complement Rules for the Tutorial")]
    private float _enoughMovement = 2, _movement = 0;
    private float _enoughCameraMovement = 2, _cameraMovement = 0;
    [SerializeField] private GameObject _firstEnemyToDefeat, _secondEnemyToDefeat;
    [SerializeField] private WheelWoodInteraction _wheel;
    private float _enoughRun = 2, _run = 0;

    private void Start()
    {
        _popUpIndex = startInBeggining ? 0 : -1;
        
        if(popUps.Length > 0 && startInBeggining)
        {
            popUps[0].popUpUI.SetActive(true);
        }

        _firstEnemyToDefeat?.SetActive(false);
        _secondEnemyToDefeat?.SetActive(false);
    }

    public void EnableFirstTutorial()
    {
        _popUpIndex = 0;

        if(popUps.Length == 0) return;
        
        popUps[_popUpIndex].popUpUI.SetActive(true);
    }

    private void Update() 
    {
        if(_popUpIndex == -1) return;
        
        for (int i = 0; i < popUps.Length; i++)
        {
            popUps[i].popUpUI.SetActive(i == _popUpIndex);
        }

        ActionsExpected();        
    }

    private void ActionsExpected()
    {
        if(_popUpIndex >= popUps.Length) return;
        
        InstructionForTutorial currentTutorial = popUps[_popUpIndex];
        UnityEvent currentEventToTrow = currentTutorial.eventToFullfill;
        
        switch (_popUpIndex)
        {
            case 0: // Character Movement
                if( Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
                    _movement += Time.deltaTime;

                if(_movement > _enoughMovement) 
                {
                    currentEventToTrow.Invoke();
                    _popUpIndex++;
                };
                break;

            case 1: // Move Camera             
                if(Input.GetAxisRaw("Mouse X") != 0 || Input.GetAxisRaw("Mouse Y") != 0)
                    _cameraMovement += Time.deltaTime;

                if(_cameraMovement > _enoughCameraMovement)
                {
                    currentEventToTrow.Invoke();
                    _popUpIndex++;
                } 
                break;

            case 2: // Shoot and Defeat the Enemy
                // Not Allow to use Meele Attack                
                if (_firstEnemyToDefeat != null && !_firstEnemyToDefeat.activeInHierarchy)
                {
                    _firstEnemyToDefeat.SetActive(true);
                }

                // Check if the enemy has been destroyed
                if (_firstEnemyToDefeat == null || !_firstEnemyToDefeat.activeInHierarchy)
                {
                    currentEventToTrow.Invoke();
                    _popUpIndex++;
                }
                break;

            case 3: // Meele Attack and Defeat Second Enemy
                // Not Allow to Shoot
                if (_secondEnemyToDefeat != null && !_secondEnemyToDefeat.activeInHierarchy)
                {
                    _secondEnemyToDefeat.SetActive(true);
                }

                // Check if the second enemy has been destroyed
                if (_secondEnemyToDefeat == null || !_secondEnemyToDefeat.activeInHierarchy)
                {
                    currentEventToTrow.Invoke();
                    _popUpIndex++;
                }
                break;

            case 4:
                // Change to other munition
                if(foodBox.BulletSelected() == MunitionType.SPAGGETTI)
                {
                    currentEventToTrow.Invoke();
                    _popUpIndex++;
                }
                break;

            case 5:
                // Explain What does the Spaguetti Munition 
                if(_wheel.GetIsTurned())
                {
                    currentEventToTrow.Invoke();
                    _popUpIndex++;
                }
                break;

            case 6: // Run
                if(player.IsRunning) _run += Time.deltaTime;

                if(_run > _enoughRun)
                {
                    currentEventToTrow.Invoke();
                    _popUpIndex++;
                }
                break;

            case 7: // Jump
                if(Input.GetButtonDown("Jump") && player.IsGrounded)
                {
                    currentEventToTrow.Invoke();
                    _popUpIndex++;
                }
                break;

            default:
                break;
        }        
    }
}

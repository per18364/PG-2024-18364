using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using System.Threading;

public class FMODEvents : MonoBehaviour
{
    [field: Header("Music")]
    [field: SerializeField] public EventReference musicMainTitle { get; private set; }
    [field: SerializeField] public EventReference musicMainMenu { get; private set; }   
    [field: SerializeField] public EventReference musicLevel1 { get; private set; }
    [field: SerializeField] public EventReference musicVictory { get; private set; }
    [field: SerializeField] public EventReference musicGameOver { get; private set; }
    [field: SerializeField] public EventReference musicBossOwlLevel2 { get; private set; }
    
    [field: Header("UI SFX")]
    [field: SerializeField] public EventReference backSound { get; private set; }
    [field: SerializeField] public EventReference changeOptionSound { get; private set; }
    [field: SerializeField] public EventReference startSound { get; private set; }
    [field: SerializeField] public EventReference confirmChangeSound { get; private set; }
    [field: SerializeField] public EventReference confirmSound { get; private set; }
    [field: SerializeField] public EventReference moveMenuSound { get; private set; }
    

    [field: Header("Gauss SFX")]
    [field: SerializeField] public EventReference gaussJump { get; private set; }
    [field: SerializeField] public EventReference gaussSteps { get; private set; }
    [field: SerializeField] public EventReference gaussClaws { get; private set; }

    [field: Header("Boss Dingo SFX")]
    [field: SerializeField] public EventReference bossIntroduction { get; private set; }
    [field: SerializeField] public EventReference bossHurtSound { get; private set; }
    [field: SerializeField] public EventReference bossKO { get; private set; }

    [field: Header("Dingo Enemy SFX")]
    [field: SerializeField] public EventReference dingoEnemyDie { get; private set; }
    [field: SerializeField] public EventReference dingoEnemySteps { get; private set; }
    
    [field: Header("Weapons SFX")]
    [field: SerializeField] public EventReference foodArmShootNormal { get; private set; }
    [field: SerializeField] public EventReference projectileImpact { get; private set; }
    [field: SerializeField] public EventReference swordSwing { get; private set; }

    [field: Header("More SFX")]
    [field: SerializeField] public EventReference brokeMunitionBox { get; private set; }

    public static FMODEvents instance { get; private set; }

    private void Awake() 
    {
        if(instance == null)
        {
            Debug.LogWarning("Found more than one FMOD Events instance in the scene.");
        }
        instance = this;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManagerLevel1 : GameManager
{
    [Header("Properties For Level 1")]    
    [SerializeField] private UnityEvent actionToDoAfterDefeatEnemies;
    private bool _enemiesFinished = false;

    protected override void Initialize()
    {
        _currentLevel = LevelEnum.level1;
    }    
    
    public override void Update() 
    {
        base.Update();

        if(!_enemiesFinished && sessionData[PlayerDataEnum.kills] == 9)
        {
            actionToDoAfterDefeatEnemies.Invoke();
            AudioManager.Instance?.SetMusicArea(MusicArea.Finish);
            _enemiesFinished = true;
        }
    }
}

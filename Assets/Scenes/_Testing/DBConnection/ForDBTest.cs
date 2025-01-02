using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForDBTest : MonoBehaviour
{
    // This code was created only for manipulate the data from an nevel and send the information
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            GameManagerLevel1.Instance.ProcessResultsAndSaveGameSession(LevelResultEnum.canceled);
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            GameManagerLevel1.Instance.PlayerDataUpdate(PlayerDataEnum.jumps);
        }

        if(Input.GetKeyDown(KeyCode.Z))
        {
            GameManagerLevel1.Instance.PlayerDataUpdate(PlayerDataEnum.frequency_barringtonia);
        }

        if(Input.GetKeyDown(KeyCode.X))
        {
            GameManagerLevel1.Instance.PlayerDataUpdate(PlayerDataEnum.frequency_spaggetti);
        }

        if(Input.GetKeyDown(KeyCode.C))
        {
            GameManagerLevel1.Instance.PlayerDataUpdate(PlayerDataEnum.frequency_jelly);
        }

        if(Input.GetKeyDown(KeyCode.V))
        {
            GameManagerLevel1.Instance.PlayerDataUpdate(PlayerDataEnum.frequency_hot_tea);
        }

        if(Input.GetKeyDown(KeyCode.B))
        {
            GameManagerLevel1.Instance.PlayerDataUpdate(PlayerDataEnum.frequency_cake);
        }

        if(Input.GetKeyDown(KeyCode.F))
        {
            GameManagerLevel1.Instance.PlayerDataUpdate(PlayerDataEnum.frequency_melee_attack);
        }
    }
}

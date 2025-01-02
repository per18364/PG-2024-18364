using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    public Dictionary<string, PlayerSesionData> sesionsData;

    public GameData(Dictionary<string, PlayerSesionData> pSesionsData)
    {
        sesionsData = pSesionsData;
    }
}

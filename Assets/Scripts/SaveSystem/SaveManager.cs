using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveManager
{
    private static string dataPath = Application.persistentDataPath + "/PlayersData.save";
    
    public static GameData LoadPlayerData()
    {
        if (!File.Exists(dataPath))
        {
            Debug.Log("No se encontró el Archivo de datos Guardados");
            return null;
        }

        try
        {
            using (FileStream fileStream = new FileStream(dataPath, FileMode.Open))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                GameData gameData = (GameData)binaryFormatter.Deserialize(fileStream);
                return gameData;
            }
        }
        catch (SerializationException e)
        {
            Debug.LogError("Failed to load data. The file might be corrupted. Error: " + e.Message);
            File.Delete(dataPath); // Optionally delete the corrupted file.
            return null;
        }
    }

    public static void SavePlayerData()
    {
        GameData gameData = new GameData(SesionData.Instance.PlayersSessions);
        using (FileStream fileStream = new FileStream(dataPath, FileMode.Create))
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(fileStream, gameData);
        }
    }
}

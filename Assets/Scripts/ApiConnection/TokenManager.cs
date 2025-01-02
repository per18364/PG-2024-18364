using UnityEngine;

public static class TokenManager
{
    private static readonly string tokenKey = "userToken";

    public static void SetToken(string token)
    {
        string encryptedToken = EncryptionHelper.Encrypt(token);
        PlayerPrefs.SetString(tokenKey, encryptedToken);
        PlayerPrefs.Save();
    }

    public static string GetToken()
    {
        if (PlayerPrefs.HasKey(tokenKey))
        {
            string encryptedToken = PlayerPrefs.GetString(tokenKey);
            return EncryptionHelper.Decrypt(encryptedToken);
        }
        return null;
    }
}

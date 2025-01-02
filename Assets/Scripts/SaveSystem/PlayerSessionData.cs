using System;

[Serializable]
public class PlayerSesionData
{
    public string userName;
    public string userEmail;
    public string charapter;
    public string totalTime;
    public string token;

    public PlayerSesionData(string userName, string userEmail, string token)
    {
        this.userName = userName;
        this.userEmail = userEmail;
        charapter = "Capíulo 01";
        totalTime = "00:00:00";
        this.token = token;
    }

    public PlayerSesionData(string userName, string userEmail, string charapter, string totalTime, string token)
    {
        this.userName = userName;
        this.userEmail = userEmail;
        this.charapter = charapter;
        this.totalTime = totalTime;
        this.token = token;
    }
}

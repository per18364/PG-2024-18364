using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ApiConfig", menuName = "ScriptableObjects/ApiConfig", order = 1)]
public class ApiConfig : ScriptableObject
{
    public string apiUrl;
}
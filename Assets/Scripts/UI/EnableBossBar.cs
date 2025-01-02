using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableBossBar : MonoBehaviour
{
    [SerializeField] private GameObject _bossBar;
    private bool _triggerActivaded = false;

    private void Start() 
    {
        _bossBar.SetActive(false);
    }

    private void OnTriggerEnter(Collider other) 
    {
        if(other.gameObject.tag == "Player" && !_triggerActivaded)
        {
            _bossBar.SetActive(true);
            _triggerActivaded = true;
        }
    }
}

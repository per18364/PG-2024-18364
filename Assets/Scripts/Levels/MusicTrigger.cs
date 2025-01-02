using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

public class MusicTrigger : MonoBehaviour
{
    [SerializeField] private EventReference _musicToSet;
    private bool _triggerActivaded = false;
    
    private void OnTriggerEnter(Collider other) 
    {
        if(other.gameObject.tag == "Player" && !_triggerActivaded)
        {
            AudioManager.Instance?.InitializeMusic(_musicToSet);
            _triggerActivaded = true;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicAreaTrigger : MonoBehaviour
{
    [SerializeField] private MusicArea _area;
    private bool _triggerActivaded = false;
    
    private void OnTriggerEnter(Collider other) 
    {
        if(other.gameObject.tag == "Player" && !_triggerActivaded)
        {
            AudioManager.Instance?.SetMusicArea(_area);
            _triggerActivaded = true;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class SoundForProjectil : MonoBehaviour
{
    [SerializeField] private EventReference _sound;
    [SerializeField] private float _seconds;
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpecialSound());
    }

    private IEnumerator SpecialSound()
    {
        while(true)
        {
            AudioManager.Instance?.PlayOneShot(_sound, transform.position);
            yield return new WaitForSeconds(_seconds);
        }
    }
}

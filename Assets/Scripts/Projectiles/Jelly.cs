using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jelly : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) 
    {
        
    }
    
    private void OnCollisionEnter(Collision other) 
    {
        Destroy(gameObject);
    }
}

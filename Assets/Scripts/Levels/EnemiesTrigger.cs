using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesTrigger : MonoBehaviour
{
    [SerializeField] private List<GameObject> _enemies = new List<GameObject>();
    private bool _spawndedTrigger = false;
    
    private void OnTriggerEnter(Collider other) 
    {
        if(other.gameObject.tag == "Player" && !_spawndedTrigger)
        {
            foreach (GameObject enemy in _enemies)
            {
                enemy.SetActive(true);
            }

            _spawndedTrigger = true;
        }
    }
}

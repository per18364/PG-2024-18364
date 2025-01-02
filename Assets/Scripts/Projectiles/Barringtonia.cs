using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Barringtonia : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Standar Damage
        if (other.gameObject.CompareTag("Enemy"))
        {
            AudioManager.Instance?.PlayOneShot(FMODEvents.instance.projectileImpact, transform.position);
            bool impacted = other.gameObject.GetComponent<Enemy>().BarringtoniaDamage();
            if (impacted) Destroy(gameObject);
        }       
    }
}

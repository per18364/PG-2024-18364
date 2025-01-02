using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaghettiScript : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Standar Damage
        if (other.gameObject.CompareTag("Enemy"))
        {
            AudioManager.Instance?.PlayOneShot(FMODEvents.instance.projectileImpact, transform.position);
            bool impacted = other.gameObject.GetComponent<Enemy>().GetTangled();
            if (impacted) Destroy(gameObject);
        }
    }
}

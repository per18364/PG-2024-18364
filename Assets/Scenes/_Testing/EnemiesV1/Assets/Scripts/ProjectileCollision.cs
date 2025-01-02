using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProyectileCollision : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private EnemyAI enemyAI;
    private GausLife lifeUI;

    private int enemyCollisionCount = 0;
    // Start is called before the first frame update

    public void SetOwner(EnemyAI enemyAI)
    {
        this.enemyAI = enemyAI;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        lifeUI = GameObject.FindObjectOfType<GausLife>();
        //lifeUI.ChangeLife(5); // Vida default
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (enemyAI != null)
            {
                Debug.Log("El proyectil ha entrado en el trigger del jugador");

                enemyAI?.StopBoomerangCoroutine();

                Vector3 directionToEnemy = (enemyAI.transform.position - transform.position).normalized;
                rb.velocity = Vector3.zero;  // Anula la velocidad
                rb.angularVelocity = Vector3.zero;  // Anula la velocidad angular
                rb.isKinematic = false;  // Habilita la gravedad y otras respuestas físicas
                rb.useGravity = false;  // Asegúrate de que la gravedad está activada

                rb.AddForce(directionToEnemy * enemyAI.projectileSpeed * enemyAI.projectileSpeed * enemyAI.projectileSpeed, ForceMode.VelocityChange);


                Destroy(gameObject, enemyAI.destroyTime);
            }

            ThirdPersonMovement player = other.gameObject.GetComponent<ThirdPersonMovement>();

            if (player != null)
            {
                if (!player.Invulnerable)
                {
                    player.TakeDamage(1);
                    Debug.Log("Health " + player.Health.ToString());
                    lifeUI.ChangeLife(player.Health);
                }
            }
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        // Debug.Log(enemyAI);
        if (collider.gameObject.CompareTag("Player") && enemyAI != null)
        {
            Debug.Log("El proyectil ha entrado en el trigger del jugador");

            enemyAI?.StopBoomerangCoroutine();

            Vector3 directionToEnemy = (enemyAI.transform.position - transform.position).normalized;
            rb.velocity = Vector3.zero;  // Anula la velocidad
            rb.angularVelocity = Vector3.zero;  // Anula la velocidad angular
            rb.isKinematic = false;  // Habilita la gravedad y otras respuestas físicas
            rb.useGravity = false;  // Asegúrate de que la gravedad está activada

            rb.AddForce(directionToEnemy * enemyAI.projectileSpeed * enemyAI.projectileSpeed, ForceMode.VelocityChange);


            Destroy(gameObject, enemyAI.destroyTime);
        }
        else if (collider.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("El proyectil ha entrado en el trigger del enemigo");
            enemyCollisionCount++;
            if (enemyCollisionCount >= 2)
            {
                Destroy(gameObject);
                enemyCollisionCount = 0;
            }
        }
    }
}

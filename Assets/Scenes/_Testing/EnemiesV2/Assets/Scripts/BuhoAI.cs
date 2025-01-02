using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuhoAI : Enemy, IDamageable
{
    // Static list to store all enemy instances
    public static List<BuhoAI> allBuhoEnemies = new List<BuhoAI>();

    [SerializeField] private Transform player;
    [SerializeField] private LayerMask whatIsPlayer, whatIsGround;
    [SerializeField] private GameObject playerPlane;

    // ANN relacionada con el búho
    ANN ann;
    double sumSquareError = 0;

    // Parámetros de la red neuronal
    [SerializeField] private int numInputs = 3; // Ejemplo: Distancia al jugador, número de veces que el búho ha sido derrotado, número de búhos ya despertados
    [SerializeField] private int numOutputs = 1; // Ejemplo: Número de búhos extra a despertar
    [SerializeField] private int numHidden = 1;
    [SerializeField] private int numNeuronsPerHidden = 5;
    [SerializeField] private double learningRate = 0.9;

    // States
    [SerializeField] private float sightRange, attackRange, /*meleettackRange,*/ safeDistance, maxProximity;
    [SerializeField] private bool playerInSightRange, playerInAttackRange;//, playerInMeleeRange;
    [SerializeField] private float visionConeAngle = 45f; // Ángulo de visión del enemigo [0, 180]
    [SerializeField] private int steps = 10; // Número de líneas para dibujar el cono de visión
    [SerializeField] private float flightSpeed = 15f;
    [SerializeField] private float altitude = 10f;

    private enum State
    {
        Dormido,
        Persiguiendo,
        Atacando,
        Patrullando
    }

    [SerializeField] private State state;
    private Vector3 patrolCenter;
    private Vector3 patrolPoint;


    private Rigidbody rb;

    public int Health { get; set; }
    // public int defeatedCount = 0; // Número de veces que el búho ha sido derrotado
    public static int totalAwakenedBuho = 0; // Número de búhos que se han despertado
    public static int defeatedCount = 0;  // Variable compartida entre todos los búhos

    [SerializeField] private float timeAwake = 0f;
    [SerializeField] private bool isAwake = false; // Indica si el búho está despierto

    //Attacking
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float reloadTime = 2f; // Tiempo de recarga
    [SerializeField] private bool isReloading = false; // Verifica si está recargando
    private float reloadTimer = 0f; // Temporizador de recarga

    //Tangled
    [SerializeField] private bool isTangled = false;
    private bool onGround = false;  // Indica si ya ha tocado el suelo
    private float timeTangled = 0f; // Temporizador de desenredo
    [SerializeField] private float maxTangledTime = 4f; // Tiempo máximo enredado antes de desenredarse
    private float fallStartHeight; // Altura desde la que empieza a caer

    //Animation Connection
    private BuhoAnimations _animations;

    private void Awake()
    {
         _animations = GetComponent<BuhoAnimations>();

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("BuhoAI: Rigidbody not found");
        }
        // Inicializar la red neuronal
        ann = new ANN(numInputs, numOutputs, numHidden, numNeuronsPerHidden, learningRate);
        Health = base.healt;

        patrolCenter = playerPlane.transform.position;

        // Añadir el búho a la lista de enemigos
        allBuhoEnemies.Add(this);
    }

    private void OnDestroy()
    {
        // Eliminar el búho de la lista de enemigos
        allBuhoEnemies.Remove(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        rb.useGravity = false;
    }

    public override void Update()
    {
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer) /*&& IsPlayerInVisionCone(player, visionConeAngle) && !IsPlayerBehindWall(player, sightRange, whatIsGround)*/;
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer) && IsPlayerInVisionCone(player, visionConeAngle); // VERIFICAR LAS LAYERS PARA LAS PAREDES && !IsPlayerBehindWall(player, sightRange, whatIsGround);

        // Definir las entradas para la ANN
        List<double> inputs = new List<double>
        {
            defeatedCount,
            totalAwakenedBuho,
            timeAwake
        };

        // Obtener las salidas de la ANN
        List<double> outputs = ann.Go(inputs, null);  // Pasamos las entradas y obtenemos la predicción de la red neuronal
                                                      // int extraBuhoToAwaken = (int)outputs[0];  // El número de búhos adicionales a despertar
                                                      // int extraBuhoToAwaken = Mathf.RoundToInt((float)outputs[0]);
                                                      // Limitar la salida a un rango de 0 a 5 búhos extra para despertar
        int extraBuhoToAwaken = Mathf.Clamp(Mathf.RoundToInt((float)outputs[0]), 0, allBuhoEnemies.Count - 1);

        // Lógica de comportamiento del búho
        switch (state)
        {
            case State.Dormido:
                if (playerInAttackRange)
                {
                    state = State.Atacando;
                }
                else if (playerInSightRange)
                {
                    state = State.Persiguiendo;
                    totalAwakenedBuho++;  // Despertar al búho actual
                    totalAwakenedBuho += extraBuhoToAwaken;  // Despertar búhos adicionales según la ANN
                    Debug.Log("Awakening " + extraBuhoToAwaken + " extra buhos.");
                    WakeUpExtraBuho(extraBuhoToAwaken, maxProximity);
                }
                break;
            case State.Persiguiendo:
                if (playerInAttackRange)
                {
                    state = State.Atacando;
                }
                else if (!playerInSightRange && !playerInAttackRange)
                {
                    state = State.Patrullando;
                }
                FlyTowardsPlayer();
                break;
            case State.Atacando:
                if (!playerInAttackRange && playerInSightRange)
                {
                    state = State.Persiguiendo;
                }
                else if (!playerInSightRange)
                {
                    state = State.Patrullando;
                }
                // Lógica de disparo
                if (!isReloading && !isTangled)
                {
                    AttackDefault(); // Disparar al jugador
                }
                else
                {
                    Reload(); // Si está recargando, manejar el temporizador
                }
                break;
            case State.Patrullando:
                if (playerInAttackRange)
                {
                    state = State.Atacando;
                }
                else if (playerInSightRange)
                {
                    state = State.Persiguiendo;
                }
                // else
                // {
                Patrol();
                // }
                // Lógica de patrullaje
                break;
        }

        // Si el búho es derrotado, entrenar la ANN con los nuevos resultados
        if (Health <= 0)
        {
            defeatedCount++;
            isAwake = false;  // Reinicia la variable para el estado de despierto
            Debug.Log("Buho defeated. Despierto por " + timeAwake + " segundos.");
            List<double> desiredOutput = new List<double> { Mathf.Min(3, defeatedCount) };  // Por ejemplo
            ann.Go(inputs, desiredOutput);  // Entrenar la ANN con los nuevos resultados
            timeAwake = 0f;
            Destroy(gameObject);  // Destruir el búho
        }

        if (state != State.Dormido && !isAwake)
        {
            // Cuando el búho cambia de "Dormido" a otro estado, comienza a contar el tiempo
            isAwake = true;
            timeAwake = 0f;  // Reinicia el contador de tiempo despierto
        }

        if (isAwake)
        {
            // Si el búho está despierto, cuenta el tiempo que lleva en ese estado
            timeAwake += Time.deltaTime;  // Incrementa el tiempo
        }

        if (isTangled && !onGround)
        {
            // Comprobar si el búho ha tocado el suelo
            if (Physics.CheckSphere(transform.position, 0.1f, whatIsGround))
            {
                onGround = true;
                Debug.Log("Buho ha tocado el suelo.");
                ApplyFallDamage(); // Aplicar daño por la caída
            }
        }

        if (onGround)
        {
            // Incrementar el temporizador una vez que el búho ha tocado el suelo
            timeTangled += Time.deltaTime;

            if (timeTangled >= maxTangledTime)
            {
                UnTangle(); // Desenredarse y volver a volar
            }
        }
        
        if(state != State.Dormido)
        {            
            _animations.FlyAnimation(true);
        }
    }

    void OnDrawGizmos()
    {
        // Dibuja un esfera semi-transparente para el rango de vista
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        // Dibuja un esfera semi-transparente para el rango de ataque a distancia
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Dibuja un esfera semi-transparente para el rango de ataque cuerpo a cuerpo
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, maxProximity);

        DrawViewCone(transform.position, transform.forward, sightRange, visionConeAngle); // 45 grados es el ángulo del cono
    }

    void DrawViewCone(Vector3 position, Vector3 forward, float range, float angle)
    {
        // Cuántas líneas usar para formar el cono, más pasos = cono más suave
        float stepAngle = angle / steps;
        Gizmos.color = Color.yellow;

        for (int i = -steps; i <= steps; i++)
        {
            Vector3 direction = Quaternion.Euler(0, stepAngle * i, 0) * forward;
            Gizmos.DrawLine(position, position + direction * range);
        }

        // Dibuja líneas para cerrar los lados del cono
        Gizmos.DrawLine(position, position + Quaternion.Euler(0, -angle, 0) * forward * range);
        Gizmos.DrawLine(position, position + Quaternion.Euler(0, angle, 0) * forward * range);
    }

    private void WakeUpExtraBuho(int count, float maxProximity)
    {
        int awakenedCount = 0;

        foreach (BuhoAI buho in allBuhoEnemies)
        {
            if (awakenedCount >= count) break;

            Debug.Log("Buho: " + buho.name + " - State: " + buho.state + " - Distance: " + Vector3.Distance(transform.position, buho.transform.position));
            if (buho != this && buho.state == State.Dormido && Vector3.Distance(transform.position, buho.transform.position) <= maxProximity)
            {
                buho.state = State.Persiguiendo;  // Wake up the extra enemy
                awakenedCount++;
                Debug.Log("Extra buho awakened.");
            }
        }
    }

    private void FlyTowardsPlayer()
    {
        // Calcula la dirección hacia el jugador y ajusta la altitud
        Vector3 targetPosition = new Vector3(player.position.x, altitude, player.position.z);

        // Mueve el búho hacia el jugador en 3D
        rb.MovePosition(Vector3.MoveTowards(transform.position, targetPosition, flightSpeed * Time.deltaTime));
        transform.LookAt(player.position);
    }

    private void Patrol()
    {
        // Obtener los límites del área de patrullaje (PlayerPlane)
        Vector3 planeSize = playerPlane.GetComponent<Renderer>().bounds.size; // Obtiene el tamaño del PlayerPlane
        Vector3 planeCenter = playerPlane.transform.position; // Centro del PlayerPlane

        // Si no tiene un punto de patrullaje o ya ha llegado al punto, selecciona uno nuevo dentro de los límites del PlayerPlane
        if (Vector3.Distance(transform.position, patrolPoint) < 0.1f || patrolPoint == Vector3.zero)
        {
            // Genera un nuevo punto dentro del área de PlayerPlane
            patrolPoint = new Vector3(
                UnityEngine.Random.Range(planeCenter.x - planeSize.x / 2, planeCenter.x + planeSize.x / 2),
                altitude, // Mantén la altitud constante
                UnityEngine.Random.Range(planeCenter.z - planeSize.z / 2, planeCenter.z + planeSize.z / 2)
            );
        }

        // Mueve el búho hacia el punto de patrullaje en 3D
        Vector3 targetPosition = new Vector3(patrolPoint.x, altitude, patrolPoint.z);
        rb.MovePosition(Vector3.MoveTowards(transform.position, targetPosition, flightSpeed * Time.deltaTime));

        // Orienta el búho hacia el nuevo punto
        if (Vector3.Distance(transform.position, patrolPoint) >= 0.1f)
        {
            transform.LookAt(new Vector3(patrolPoint.x, transform.position.y, patrolPoint.z)); // Mantén la mirada horizontal
        }
    }

    public void TakeDamage(int damage)
    {
        Debug.Log("Buho Take Damage");
    }

    protected override void AttackDefault()
    {
        // Instanciar el proyectil y dispararlo hacia el jugador
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);

        // Aplicar la fuerza hacia la dirección del jugador
        Vector3 directionToPlayer = (player.position - projectileSpawnPoint.position).normalized;
        projectile.GetComponent<Rigidbody>().AddForce(directionToPlayer * 20f, ForceMode.Impulse); // Aumentar el valor según la velocidad deseada

        Debug.Log("Buho ha disparado una flecha!");

        Destroy(projectile, 5f); // Destruir el proyectil después de 5 segundos 

        // Iniciar el ciclo de recarga
        isReloading = true;
        reloadTimer = reloadTime; // Reinicia el temporizador de recarga
    }

    private void Reload()
    {
        reloadTimer -= Time.deltaTime; // Decrementa el temporizador
        if (reloadTimer <= 0)
        {
            isReloading = false; // Finaliza el ciclo de recarga
        }
    }

    public override bool BarringtoniaDamage()
    {
        Debug.Log("Buho Barringtonia Damage");
        Health -= 1;
        return true;
    }

    public override bool GetTangled()
    {
        Debug.Log("Buho Get Tangled");

        if (!isTangled)
        {
            // Marcar el búho como enredado
            isTangled = true;
            rb.useGravity = true; // Activar la gravedad para que el búho caiga
            fallStartHeight = transform.position.y; // Guardar la altura de la caída

            // Desactivar cualquier movimiento o ataque durante el enredo
            flightSpeed = 0f;
        }
        
        return true;
    }

    private void ApplyFallDamage()
    {
        float fallDistance = fallStartHeight - transform.position.y; // Calcular la distancia de la caída
        int damage = Mathf.RoundToInt(fallDistance * 0.02f); // Calcular el daño según la distancia (puedes ajustar el multiplicador)
        Health -= damage;
        Debug.Log("Buho recibió " + damage + " de daño por la caída.");
    }

    private void UnTangle()
    {
        // Restablecer los valores para que el búho vuelva a volar
        isTangled = false;
        onGround = false;
        timeTangled = 0f;
        rb.useGravity = false; // Desactivar la gravedad para que vuelva a volar
        flightSpeed = 15f; // Restaurar la velocidad de vuelo
        Debug.Log("Buho se ha desenredado y vuelve a volar.");
    }

    protected override void StateHealth()
    {
        Debug.Log("Buho State Health");
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : Enemy, IDamageable
{
    [Header("Enemy IA Properties")]
    [SerializeField] private NavMeshAgent agent;

    [SerializeField] private Transform player;
    [SerializeField] private GameObject playerPlane;

    [SerializeField] private LayerMask whatIsGround, whatIsPlayer;

    // Patrolling
    [SerializeField] private Vector3 walkPoint;
    bool walkPointSet;
    [SerializeField] private float walkPointRange;
    [SerializeField] private Transform[] walkPoints;
    private int currentWalkPointIndex = 0;
    [SerializeField] private EnemyHealthProgressBar healthBar;


    // Attacking
    [SerializeField] private float timeBetweenAttacks;
    bool alreadyAttacked;
    [SerializeField] private GameObject projectile;
    [SerializeField] private Transform projectileSpawn;

    public float destroyTime = 3f;

    // States
    [SerializeField] private float sightRange, attackRange, meleeAttackRange, safeDistance;
    [SerializeField] private bool playerInSightRange, playerInAttackRange, playerInMeleeRange;
    [SerializeField] private float visionConeAngle = 45f; // Ángulo de visión del enemigo [0, 180]

    [SerializeField] private int steps = 10; // Número de líneas para dibujar el cono de visión

    public float projectileSpeed = 5f;

    //Amarrado
    [SerializeField] private int totalHitsReceived = 0;
    [SerializeField] private bool isTiedUp = false;
    [SerializeField] private float timeToBreakFree = 6f;
    [SerializeField] private int hitCountSinceTied = 0;
    [SerializeField] private int totalhits = 10;
    [Header("Enemy IA Behaiviour")]
    [SerializeField] private GameObject _spaguettiObject;

    private Coroutine boomerangCoroutine;

    public int Health { get; set; }
    [SerializeField] private SkinnedMeshRenderer _characterRenderer;
    [SerializeField] private Color _damageColor = Color.red;
    private Color _originalColor;
    [SerializeField] private bool hasBeenHit = false;
    [SerializeField] private float attackTimeout = 5f;
    [SerializeField] private float lastAttackTime;
    private bool _stop = false;

    // [SerializeField] private bool isTutorial = false;
    [SerializeField]
    private enum EnemyType
    {
        Normal,
        Tutorial1,
        Tutorial2
    }
    [SerializeField] private EnemyType enemyType;

    //Animation Connection
    private DingoAnimations _animations;

    private void Awake()
    {
        _animations = GetComponent<DingoAnimations>();

        // player = GameObject.Find("PlayerObj").transform;
        agent = GetComponent<NavMeshAgent>();

        if (player == null)
            Debug.LogError("EnemyIA.cs error: Player is Null.");

        if (agent == null)
            Debug.LogError("EnemyIA.cs error: Agent is Null.");

        if (_spaguettiObject == null)
            Debug.LogError("EnemyIA.cs error: Spaguetti Effect is Null.");

        Health = base.healt;
        _originalColor = _characterRenderer.material.color;
        _stop = !gameObject.activeSelf;
    }

    private void OnEnable() 
    {
        StartCoroutine(AppearingEnemy());
    }

    private IEnumerator AppearingEnemy()
    {
        _stop = true;
        print("Apareciendo");
        yield return new WaitForSeconds(2f);
        _stop = false;
        print("ATAQUEEEEE");
    }

    // Update is called once per frame
    public override void Update()
    {
        if(_stop) return;
        
        if (enemyType == EnemyType.Normal || enemyType == EnemyType.Tutorial2)
        {
            playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer) && IsPlayerInVisionCone() && !IsPlayerBehindWall();
            playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer) && IsPlayerInVisionCone() && !IsPlayerBehindWall();
            playerInMeleeRange = Physics.CheckSphere(transform.position, meleeAttackRange, whatIsPlayer) && IsPlayerInVisionCone() && !IsPlayerBehindWall();

            if (hasBeenHit || (playerInSightRange && !playerInAttackRange))
            {
                if (!playerInSightRange && Time.time - lastAttackTime > attackTimeout)
                {
                    // Si el jugador ha salido del rango de vista y ha pasado el tiempo de espera, volver a patrullar
                    hasBeenHit = false;
                }
                else
                {
                    ChasePlayer();
                }
            }
            else if (!playerInSightRange && !playerInAttackRange) Patrolling();

            // Solo atacar si no es de tipo Tutorial2
            if (enemyType != EnemyType.Tutorial2)
            {
                if (playerInSightRange && playerInAttackRange && !playerInMeleeRange) AttackPlayer();
                if (playerInMeleeRange) AttackDefault();
            }

            MaintainSafeDistance();
            _spaguettiObject.SetActive(isTiedUp);
        }

        StateHealth();
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
        Gizmos.DrawWireSphere(transform.position, meleeAttackRange);

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

    private bool IsPlayerInVisionCone()
    {
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float angleBetweenEnemyAndPlayer = Vector3.Angle(transform.forward, dirToPlayer);
        return angleBetweenEnemyAndPlayer < visionConeAngle;
    }

    private bool IsPlayerBehindWall()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, (player.position - transform.position).normalized, out hit, sightRange, whatIsGround))
        {
            if (hit.transform != player)
            {
                return true; // Hay una pared entre el enemigo y el jugador
            }
        }
        return false; // No hay pared entre el enemigo y el jugador
    }


    private void Patrolling()
    {
        // if (!walkPointSet && !isTiedUp) SearchWalkPoint();
        if (!walkPointSet && !isTiedUp) GoToWalkPoint();

        if (walkPointSet && !isTiedUp)
        {
            // print("MAGNITUD: " + agent.velocity.magnitude);
            _animations.WalkingAnimation(agent.velocity.magnitude > 1.0f);
            agent.SetDestination(walkPoint);

            // Cuando el enemigo llega al punto de caminata
            // Debug.Log("Distance to walk point: " + Vector3.Distance(transform.position, walkPoint));
            if (Vector3.Distance(transform.position, walkPoint) <= 2f)
            {
                walkPointSet = false;
                currentWalkPointIndex = (currentWalkPointIndex + 1) % walkPoints.Length;
                // Debug.Log("Next walk point: " + currentWalkPointIndex);
            }
        }
        else
        {
            _animations.WalkingAnimation(false);
        }
        // ! ANIMATION: Set Walk Animation
    }

    private void SearchWalkPoint()
    {
        // Calcula un punto aleatorio en el rango designado
        float randomZ = Random.Range(-playerPlane.transform.localScale.z, playerPlane.transform.localScale.z);
        float randomX = Random.Range(-playerPlane.transform.localScale.x, playerPlane.transform.localScale.x);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);
        NavMeshHit hit;

        // Intenta encontrar el punto más cercano en el NavMesh dentro de un radio dado
        if (NavMesh.SamplePosition(walkPoint, out hit, walkPointRange, NavMesh.AllAreas))
        {
            walkPoint = hit.position;
            walkPointSet = true;
        }
    }

    private void GoToWalkPoint()
    {
        if (walkPoints.Length > 0)
        {
            walkPoint = walkPoints[currentWalkPointIndex].position;
            walkPointSet = true;
        }
    }

    private void ChasePlayer()
    {
        if (!isTiedUp)
        {
            agent.SetDestination(player.position);
            _animations.WalkingAnimation(agent.velocity.magnitude > 1.0f);
            _animations.RuningAnimation(agent.velocity.magnitude > 1.0f);
        }                
    }

    private void AttackPlayer()
    {
        // Asegúrate de que el enemigo no se mueva
        agent.SetDestination(transform.position);

        transform.LookAt(player);

        if (!alreadyAttacked && !isTiedUp)
        {
            boomerangAttack();

            // Marca que el enemigo ya atacó y prepara el siguiente ataque
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);

        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    private void boomerangAttack()
    {
        // StartCoroutine(BoomerangCheck());
        boomerangCoroutine = StartCoroutine(BoomerangCheck());
    }

    private IEnumerator BoomerangCheck()
    {
        // ! ANIMATION: Set Throwing Boomerang Attack Animation
        _animations.ThrowingBoomAnimation();
        GameObject boomerang = Instantiate(projectile, transform.position, Quaternion.identity);
        boomerang.GetComponent<ProyectileCollision>().SetOwner(this);
        Rigidbody rb = boomerang.GetComponent<Rigidbody>();

        // Get the MeshRenderer from the child GameObject
        MeshRenderer meshRenderer = boomerang.GetComponentInChildren<MeshRenderer>();

        if (meshRenderer != null)
        {
            meshRenderer.enabled = true; // Enable the MeshRenderer
        }
        else
        {
            Debug.LogWarning("MeshRenderer not found in the children of the boomerang GameObject.");
        }

        SphereCollider sphereCollider = boomerang.GetComponent<SphereCollider>();
        if (sphereCollider != null)
        {
            sphereCollider.enabled = true;
        }
        else
        {
            Debug.LogWarning("SphereCollider not found on the boomerang GameObject.");
        }

        Destroy(rb.gameObject, destroyTime);

        Vector3 playerPosition = player.position;
        Vector3 enemyPosition = transform.position;
        Vector3 midPoint = (playerPosition + enemyPosition) / 2f; // Midpoint between the enemy and the player
        Vector3 attackDirection = (playerPosition - enemyPosition).normalized; // Attack direction
        Vector3 perpendicularDirection = Vector3.Cross(attackDirection, Vector3.up).normalized; // Perpendicular direction for circular motion

        float orbitRadius = Vector3.Distance(playerPosition, enemyPosition) / 2f; // Orbit radius
        float initialAngle = Mathf.Atan2(perpendicularDirection.x, perpendicularDirection.z); // Initial angle for the orbit

        float x = Mathf.Cos(initialAngle) * orbitRadius;
        float z = Mathf.Sin(initialAngle) * orbitRadius;
        Vector3 initialPosition = midPoint + new Vector3(x, 0, z);
        rb.MovePosition(initialPosition);

        while (rb != null) // You can replace this with an appropriate termination condition
        {
            initialAngle += projectileSpeed * Time.deltaTime; // Increase the angle to move in the orbit
            x = Mathf.Cos(initialAngle) * orbitRadius;
            z = Mathf.Sin(initialAngle) * orbitRadius;

            // Calculate the new position of the projectile based on the orbit
            Vector3 newPosition = midPoint + new Vector3(x, 0, z);
            rb.MovePosition(newPosition);

            yield return null;
        }
    }

    public void StopBoomerangCoroutine()
    {
        Debug.Log("StopCoroutine");
        // StopCoroutine(BoomerangCheck());
        if (boomerangCoroutine != null)
        {
            StopCoroutine(boomerangCoroutine);
            boomerangCoroutine = null;
        }

    }
    protected override void AttackDefault()
    {
        agent.SetDestination(transform.position);

        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            // Ataque cuerpo a cuerpo
            Collider[] hitPlayer = Physics.OverlapSphere(transform.position, meleeAttackRange, whatIsPlayer);
            if (hitPlayer.Length > 0)
            {
                //player.GetComponent<PlayerHealth>().TakeDamage(attackDamage);
                Debug.Log("Player hit");
            }

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    public void TakeDamage(int damage)
    {
        totalHitsReceived += damage;
        Debug.Log("Enemy took damage: " + damage + " Total hits: " + totalHitsReceived);
        StartCoroutine(DamageEffect());
        Debug.LogWarning(totalHitsReceived / totalhits);
        healthBar.SetProgress(1f - ((float)totalHitsReceived / (float)totalhits));

        if (isTiedUp)
        {
            hitCountSinceTied++;
            if (hitCountSinceTied >= 4)
            {
                isTiedUp = false;
                hitCountSinceTied = 0;
            }
        }

        hasBeenHit = true;
        lastAttackTime = Time.time;
        ChasePlayer();
    }

    private IEnumerator DamageEffect()
    {
        _characterRenderer.material.color = _damageColor;
        yield return new WaitForSeconds(0.2f);
        _characterRenderer.material.color = _originalColor;
    }

    public override bool BarringtoniaDamage()
    {
        GameManager.Instance.PlayerDataUpdate(PlayerDataEnum.impact_barringtonia);
        totalHitsReceived += 1;
        StartCoroutine(DamageEffect());
        Debug.LogWarning(totalHitsReceived / totalhits);
        healthBar.SetProgress(1f - ((float)totalHitsReceived / (float)totalhits));
        if (isTiedUp)
        {
            hitCountSinceTied++;
            if (hitCountSinceTied >= 4)
            {
                // isTiedUp = false;
                hitCountSinceTied = 0;
            }
        }

        hasBeenHit = true;
        lastAttackTime = Time.time;
        if (enemyType == EnemyType.Normal) ChasePlayer();
        return true;
    }

    public override bool GetTangled()
    {
        GameManager.Instance.PlayerDataUpdate(PlayerDataEnum.impact_spaggetti);
        isTiedUp = true;
        hitCountSinceTied = 0;

        StartCoroutine(TiedUp());
        return true;
    }

    private IEnumerator TiedUp()
    {
        // while (isTiedUp)
        // {
        //     timeSinceTied += Time.deltaTime;
        //     if (timeSinceTied >= timeToBreakFree)
        //     {
        //         isTiedUp = false;
        //         hitCountSinceTied = 0;
        //     }
        //     yield return null;
        // }

        agent.SetDestination(transform.position);
        // ! ANIMATION: Set Idle Animation (Would be tangled but by the moment, idle)
        
        yield return new WaitForSeconds(timeToBreakFree);

        isTiedUp = false;
    }

    protected override void StateHealth()
    {
        if (totalHitsReceived >= totalhits && !_stop)
        {
            _stop = true;
            Debug.Log("Enemy died");
            if (AudioManager.Instance != null) AudioManager.Instance?.PlayOneShot(FMODEvents.instance.dingoEnemyDie, transform.position);
            if (GameManager.Instance != null) GameManager.Instance.PlayerDataUpdate(PlayerDataEnum.kills);
            // ! ANIMATION: Set Death Animation
            _animations.KO();
            // Will be setted a corroutine for Destroy de object two seconds later
            StartCoroutine(WillBeDestroyed());
        }
    }

    private IEnumerator WillBeDestroyed()
    {
        agent.SetDestination(transform.position);
        yield return new WaitForSeconds(3.5f);
        Destroy(gameObject);
    }

    private void MaintainSafeDistance()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer < safeDistance)
        {
            Debug.Log("MaintainSafeDistance");
            Vector3 directionAwayFromPlayer = (transform.position - player.position).normalized;
            Vector3 newPosition = transform.position + directionAwayFromPlayer * (safeDistance - distanceToPlayer);

            if (!isTiedUp)
            {
                agent.SetDestination(newPosition);
                transform.LookAt(player);
            }

        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class BossAI : Enemy, IDamageable
{
    [SerializeField] private NavMeshAgent agent;

    [SerializeField] private Transform player;
    [SerializeField] private GameObject playerPlane;

    [SerializeField] private LayerMask whatIsGround, whatIsPlayer;

    // Patrolling
    private Vector3 walkPoint;
    bool walkPointSet;
    [SerializeField] private float walkPointRange;

    // Attacking
    [SerializeField] private float timeBetweenAttacks;
    [SerializeField] private float delayBetweenAttacks = 4f;
    bool alreadyAttacked;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform[] projectileSpawnPoints;
    private float meleeAttackTime = 0.5f;
    private Vector3 originalBossPosition;

    // States
    [SerializeField] private float sightRange, attackRange, meleeAttackRange;
    [SerializeField] private bool playerInSightRange, playerInAttackRange, playerInMeleeRange;
    [SerializeField] private float visionConeAngle = 45f; // Ángulo de visión del enemigo [0, 180]

    [SerializeField] private int steps = 10; // Número de líneas para dibujar el cono de visión

    [SerializeField] private float projectileSpeed = 5f;

    // Configuraciones de estados
    [SerializeField] private int totalHitsReceived = 0;
    [SerializeField] private int totalHits = 20;
    [SerializeField] private bool isTiedUp = false;
    [SerializeField] private float timeSinceTied = 0;
    [SerializeField] private float timeToBreakFree = 6f;
    [SerializeField] private int hitCountSinceTied = 0;
    // Estados y configuraciones de fase
    [SerializeField] private enum BossState { Normal, PhaseTwo }
    [SerializeField] private BossState currentState = BossState.Normal;
    [SerializeField] private bool isPhaseTwo = false;
    [SerializeField] private GameObject[] bossPhaseTwoObjs;

    [SerializeField] private GameObject _spaguettiObject;

    // UI
    [SerializeField] private BossHealthBarProgress healthBar;
    
    // Extras
    private bool _playerAllreadySaw = false;
    private bool _finishedBattle = false;

    public int Health { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    //Animation Connection
    private BossDingoAnimations _animations;

    private void Awake()
    
    {
        _animations = GetComponent<BossDingoAnimations>();
        // player = GameObject.Find("PlayerObj").transform;
        agent = GetComponent<NavMeshAgent>();

        if (player == null)
            Debug.LogError("BossIA.cs error: Player is Null.");

        if (agent == null)
            Debug.LogError("BossIA.cs error: Agent is Null.");

        if (_spaguettiObject == null)
            Debug.LogError("EnemyIA.cs error: Spaguetti Effect is Null.");
    }

    // Update is called once per frame
    public override void Update()
    {
        if(_finishedBattle) return;
        
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer) && IsPlayerInVisionCone();
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer) && IsPlayerInVisionCone();
        playerInMeleeRange = Physics.CheckSphere(transform.position, meleeAttackRange, whatIsPlayer) && IsPlayerInVisionCone();

        if (!playerInSightRange && !playerInAttackRange) Patrolling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInSightRange && playerInAttackRange && !playerInMeleeRange && !isTiedUp) AttackPlayer();
        if (playerInMeleeRange) AttackDefault();

        _spaguettiObject.SetActive(isTiedUp);
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
        // Debug.Log("Player in vision cone: " + (angleBetweenEnemyAndPlayer < visionConeAngle));
        return angleBetweenEnemyAndPlayer < visionConeAngle;
    }

    private void Patrolling()
    {
        // Debug.Log("Patrolling");
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet && !isTiedUp) 
        {
            if(isPhaseTwo)
            {
                // ! Set ANIMATION: Running
                _animations.RuningAnimation(agent.velocity.magnitude > 0.1f); 
                _animations.WalkingAnimation(agent.velocity.magnitude > 0.1f);               
            }
            else
            {
                // ! Set ANIMATION: Walking
                _animations.WalkingAnimation(agent.velocity.magnitude > 0.1f);
            }
            agent.SetDestination(walkPoint);
        }

        // Cuando el enemigo llega al punto de caminata
        if (Vector3.Distance(transform.position, walkPoint) <= 6f)
            walkPointSet = false;
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

    private void ChasePlayer()
    {
        if(!_playerAllreadySaw)
        {
            _playerAllreadySaw = true;
            AudioManager.Instance?.PlayOneShot(FMODEvents.instance.bossIntroduction, transform.position);
        }
        
        // Debug.Log("Chase Player");
        if (!isTiedUp)
        {
            agent.SetDestination(player.position);
        }            
    }

    private void AttackPlayer()
    {
        if(!_playerAllreadySaw)
        {
            _playerAllreadySaw = true;
            AudioManager.Instance?.PlayOneShot(FMODEvents.instance.bossIntroduction, transform.position);
        }

        // Asegúrate de que el enemigo no se mueva
        agent.SetDestination(transform.position);

        transform.LookAt(player);

        // Debug.Log("Attack Player");
        if (!alreadyAttacked)
        {
            StartCoroutine(LaunchBoomerang(projectileSpawnPoints[1], 1));
            StartCoroutine(LaunchSecondBoomerang(projectileSpawnPoints[0]));

            // Marca que el enemigo ya atacó y prepara el siguiente ataque
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);

        }
    }

    private IEnumerator LaunchBoomerang(Transform projectileSpawnPoint, int direction)
    {
        // ! Set ANIMATION: Throw Boomerang Option 1
        _animations.ThrowingBoomAnimation();
        GameObject boomerang = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
        Rigidbody rb = boomerang.GetComponent<Rigidbody>();
        rb.useGravity = false;
        yield return StartCoroutine(BoomerangFlight(rb, projectileSpawnPoint, direction));
    }

    private IEnumerator LaunchSecondBoomerang(Transform projectileSpawnPoint)
    {
        // ! Set ANIMATION: Throw Boomerang Option 2 // I feel this will be the best way
        _animations.ThrowingBoomAnimation();
        yield return new WaitForSeconds(delayBetweenAttacks);
        // GameObject boomerang = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
        // Rigidbody rb = boomerang.GetComponent<Rigidbody>();
        // rb.useGravity = false;
        // yield return StartCoroutine(BoomerangFlight(rb, projectileSpawnPoint));
        StartCoroutine(LaunchBoomerang(projectileSpawnPoint, -1));
    }

    private IEnumerator BoomerangFlight(Rigidbody rb, Transform projectileSpawnPoint, int curveDirection)
    {
        float maxDistance = attackRange;
        float speed = projectileSpeed;
        Vector3 targetPosition = player.position;
        Vector3 returnPosition = projectileSpawnPoint.position;
        Vector3 initialDirection = (targetPosition - rb.position).normalized;
        float heightFactor = 25f;

        Vector3 perpendicularDirection = Vector3.Cross(initialDirection, Vector3.up).normalized * curveDirection;

        float distanceTraveled = 0f;
        bool returning = false;

        while (distanceTraveled < maxDistance && !returning)
        {
            float moveStep = speed * Time.deltaTime;
            distanceTraveled += moveStep;

            float normalizedDistance = distanceTraveled / maxDistance;
            float parabola = -4 * heightFactor * normalizedDistance * (normalizedDistance - 1);
            // Vector3 nextPosition = Vector3.Lerp(projectileSpawnPoint.position, targetPosition, normalizedDistance) + Vector3.right * parabola * curveDirection;
            Vector3 nextPosition = Vector3.Lerp(projectileSpawnPoint.position, targetPosition, normalizedDistance) + perpendicularDirection * parabola;
            rb.MovePosition(nextPosition);

            // rb.MovePosition(rb.position + initialDirection * speed * Time.deltaTime);
            // distanceTraveled += speed * Time.deltaTime;
            yield return null;

            if (distanceTraveled >= maxDistance)
            {
                returning = true;
            }
        }

        while (returning)
        {
            Vector3 returnDirection = (returnPosition - rb.position).normalized;
            Vector3 returnNextPosition = rb.position + returnDirection * speed * Time.deltaTime;
            if (Vector3.Distance(rb.position, returnPosition) < maxDistance / 2f)
            {
                returnNextPosition.y = Mathf.Lerp(returnNextPosition.y, returnPosition.y, 0.5f);
            }
            rb.MovePosition(returnNextPosition);

            if (Vector3.Distance(rb.position, returnPosition) < 0.5f)
            {
                Destroy(rb.gameObject);
                returning = false;
            }
            yield return null;
        }

    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    private IEnumerator PerformMeleeAnimation()
    {
        var originalColor = GetComponent<Renderer>().material.color;
        GetComponent<Renderer>().material.color = Color.black;

        originalBossPosition = transform.position;
        // Avanzar hasta cerca del jugador, parando justo antes de alcanzarlo
        var directionToPlayer = (player.position - transform.position).normalized; // dirección hacia el jugador
        var attackDistance = 0.9f; // ajusta esto para cambiar qué tan cerca llega al jugador
        var forwardPosition = transform.position + directionToPlayer * (Vector3.Distance(transform.position, player.position) * attackDistance);

        float elapsedTime = 0;

        // Avance hacia el jugador
        while (elapsedTime < meleeAttackTime)
        {
            transform.position = Vector3.Lerp(originalBossPosition, forwardPosition, (elapsedTime / meleeAttackTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Pausa en el punto de impacto
        yield return new WaitForSeconds(0.1f);

        // Retroceso a la posición original
        elapsedTime = 0;
        while (elapsedTime < meleeAttackTime)
        {
            transform.position = Vector3.Lerp(forwardPosition, originalBossPosition, (elapsedTime / meleeAttackTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        GetComponent<Renderer>().material.color = originalColor;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))  // Asegúrate de que el jugador tenga asignado el tag "Player"
        {
            Debug.Log("Player hit by melee attack");
            // Asumiendo que tienes un sistema de salud en el jugador
            // other.gameObject.GetComponent<PlayerHealth>().TakeDamage(attackDamage);

            // Efecto visual en el jugador golpeado, por ejemplo, cambio de color
            other.GetComponent<Renderer>().material.color = Color.red;

            // Detener el movimiento de embate si es necesario
            StopCoroutine(PerformMeleeAnimation());
            transform.position = originalBossPosition;  // Asumiendo que guardas la posición original al iniciar el ataque
            return;
        }

        Bullet bullet = other.gameObject.GetComponent<Bullet>();
        bullet?.RandomizeDirection();
    }

    public void TakeDamage(int damage)
    {
        totalHitsReceived += damage;
        Debug.Log("Boss took damage: " + damage + " Total hits: " + totalHitsReceived);
        healthBar.SetProgress(1f - ((float)totalHitsReceived / (float)totalHits));

        if (isTiedUp)
        {
            // ! Set ANIMATION: Idle
            hitCountSinceTied++;            
            if (hitCountSinceTied >= 4)
            {
                isTiedUp = false;
                hitCountSinceTied = 0;
            }
        }
    }

    private IEnumerator TiedUp()
    {
        while (isTiedUp)
        {
            timeSinceTied += Time.deltaTime;
            if (timeSinceTied >= timeToBreakFree)
            {
                isTiedUp = false;
                hitCountSinceTied = 0;
            }
            yield return null;
        }

        agent.SetDestination(transform.position);

        yield return new WaitForSeconds(timeToBreakFree);

        isTiedUp = false;
    }

    protected override void AttackDefault()
    {
        agent.SetDestination(transform.position);

        transform.LookAt(player);
        // Debug.Log("Melee Attack");

        if (!alreadyAttacked)
        {
            // Ataque visual: cambio de color y animación de embate
            StartCoroutine(PerformMeleeAnimation());

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    public override bool BarringtoniaDamage()
    {
        if (isTiedUp && !isPhaseTwo)
        {
            totalHitsReceived += 1;
            GameManager.Instance.PlayerDataUpdate(PlayerDataEnum.impact_barringtonia);
            Debug.Log("Boss took damage: 1 Total hits: " + totalHitsReceived);
            healthBar.SetProgress(1f - ((float)totalHitsReceived / (float)totalHits));
            hitCountSinceTied++;
            AudioManager.Instance?.PlayOneShot(FMODEvents.instance.bossHurtSound, transform.position);
            if (hitCountSinceTied >= 4)
            {
                isTiedUp = false;
                hitCountSinceTied = 0;
            }
            return true;
        }

        return false;
    }

    public override bool GetTangled()
    {
        if(isPhaseTwo) return false;
        
        isTiedUp = true;
        timeSinceTied = 0;
        hitCountSinceTied = 0;
        GameManager.Instance.PlayerDataUpdate(PlayerDataEnum.impact_spaggetti);

        StartCoroutine(TiedUp());
        return true;
    }

    protected override void StateHealth()
    {
        if (totalHitsReceived == 12 && !isPhaseTwo)
        {
            currentState = BossState.PhaseTwo;
            projectileSpeed *= 2f;
            timeBetweenAttacks /= 2f;
            delayBetweenAttacks /= 2f;
            meleeAttackTime = 0.2f;
            isPhaseTwo = true;
            AudioManager.Instance?.PlayOneShot(FMODEvents.instance.bossIntroduction, transform.position);
            foreach (GameObject obj in bossPhaseTwoObjs)
            {
                obj.SetActive(true);
            }
        }
        // if (totalHitsReceived >= totalHits)
        // {
        //     Debug.Log("Boss is defeated");
        //     Destroy(gameObject);
        // }

        // si se destruyen 3 objetos de la fase 2, el jefe es derrotado
        int destroyedCount = bossPhaseTwoObjs.Count(obj => obj == null);

        if (destroyedCount >= 4 && !_finishedBattle)
        {
            _finishedBattle = true;
            AudioManager.Instance?.InitializeMusic(FMODEvents.instance.musicVictory);            
            _animations.KO();
            totalHitsReceived = totalHits;
            healthBar?.SetProgress(1f - ((float)totalHitsReceived / (float)totalHits));
            GameManager.Instance?.ProcessResultsAndSaveGameSession(LevelResultEnum.win);
            GameManager.Instance?.StageClear();
            AudioManager.Instance?.PlayOneShot(FMODEvents.instance.bossKO, transform.position);            
        }
    }
}

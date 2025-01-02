using System.Collections;
using UnityEngine;
using Cinemachine;

public enum Camerastyle
{
    Exploration,
    Combat
}

public class ThirdPersonMovement : MonoBehaviour, IDamageable
{
    [SerializeField] private CharacterController controller;    
    private float speed;
    private int _life = 5;

    [Header("Reference")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform orientation;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform _camera;
    [SerializeField] private GameObject explorationCam;
    [SerializeField] private GameObject combatCam;
    [SerializeField] private float turnSmoothTime = 0.1f; 
    [SerializeField] private Camerastyle currentStyle;    
    private Quaternion lastPlayerRotation;
    private CinemachineFreeLook _cinemachineExploration;
    private CinemachineFreeLook _cinemachineCombat;
    // [SerializeField] private Transform combatLookAt;

    [Header("Zoom")]
    [SerializeField] private CinemachineFreeLook _cameraZoom;
    [SerializeField] private float normalFOV = 40f;
    [SerializeField] private float zoomedFOV = 20f;
    [SerializeField] private float zoomSpeed = 10f;
    // bool isZoomed = false; 

    [Header("Movement")]
    [SerializeField] private float walkingSpeed = 8f;
    [SerializeField] private float sprintSpeed = 20f;
    [SerializeField] private float defenseSpeed = 2f;
    private bool _isRunning = false;
    private float _horizontal = 0f;
    private float _vertical = 0f;
 
    float turnSmoothVelocity;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float gravity = -9.81f;
    Vector3 velocity;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;
    private bool isGrounded;

    [Header("Attack")]
    [SerializeField] private Transform attackSphere;
    [SerializeField] private LayerMask _enemyLayers;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private GameObject miraGO;
    private Mira mira;

    [Header("Invulnerable Effect")]
    [SerializeField] private float _effectDuration = 3f;
    [SerializeField] private SkinnedMeshRenderer _characterRenderer;
    [SerializeField] private MeshRenderer _armCharacterRenderer;
    [SerializeField] private Color _invulnerableColor = Color.gray;
    private Color _originalColorCharacter;
    private Color _originalColorArm;
    private bool _invulnerable = false;
    public bool Invulnerable { get { return _invulnerable;} }
    
    public int Health { get; set; }
    
    // ? ===>  Private Attributes
    private bool isInDefense = false;
    // For The Trigger RT from the controller
    private float shootAxisPrevious = 0f; 
    private float shootThreshold = 0.5f;
    // private Array<PlayerDataEnum> _frecuencyBullets = {};
    
    private Vector3 lastCameraForward;
    private Quaternion lastCameraRotation;

    // Animation Connection
    private GausAnimations _animations;
    
    public bool IsRunning { get { return _isRunning; }}
    public bool IsGrounded { get { return isGrounded; }}
    public bool IsInDefense { get { return isInDefense; }}

    private void Start() 
    {
        _animations = GetComponent<GausAnimations>();

        _cinemachineExploration = explorationCam.GetComponent<CinemachineFreeLook>();
        _cinemachineCombat = combatCam.GetComponent<CinemachineFreeLook>();
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        _originalColorCharacter = _characterRenderer.material.color;
        _originalColorArm = _armCharacterRenderer.material.color;
        Health = _life;
        lastCameraForward = explorationCam.transform.forward;
        lastCameraRotation = explorationCam.transform.rotation;

        mira = miraGO.GetComponent<Mira>();
    }

    private void OnDrawGizmosSelected()
    {
        if (attackSphere == null)
            return;

        Gizmos.DrawWireSphere(attackSphere.position, attackRange);
    }

    // Update is called once per frame
    private void Update()
    {               
        if(!GameManager.Instance.Playing) return;
        
        if(GameManager.Instance.Paused) return;
        
        // Ground Check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if(isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // A small downward force to ensure the character stays grounded
        }
        
        HandleInputs();
        Movement();        
        SprintLogic();
        DefenseLogic();
        // ControlGunLookAt();
    }

    private void HandleInputs()
    {        
        //Jump -space
        if(Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);            
            GameManager.Instance.PlayerDataUpdate(PlayerDataEnum.jumps);
            AudioManager.Instance?.PlayOneShot(FMODEvents.instance.gaussJump, transform.position);
            _animations.Jumping();
        }
        velocity.y += gravity * Time.deltaTime;        
        controller.Move(velocity * Time.deltaTime);

        //Crouch -ctrl
        if(Input.GetButtonDown("Atack"))
        {
            StartCoroutine(Attack());
        }

        // Hide Cursor When Click is presed
        if((Cursor.visible || Cursor.lockState == CursorLockMode.None) && GameManager.Instance.Playing)
        {
            if(Input.GetButtonDown("Fire1") || Input.GetButtonDown("Fire2"))
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
                    
        // Controllers for change the type of bullet
        ChangeToLastMunitionUsed();
        ChangeBulletWithWheel();
        ReloadMunition();
    }

    private void SprintLogic()
    {
        if(isInDefense) return;

        if(Input.GetButtonDown("Sprint")) _isRunning = !_isRunning;

        if(_horizontal < 0.5f && _horizontal > -0.5f && _vertical < 0.5f && _vertical > -0.5f)
        {
            _isRunning = false;
        }
        
        speed = _isRunning ? sprintSpeed : walkingSpeed;  
    }

    private void DefenseLogic()
    {
        isInDefense = Input.GetButton("Defense") || (Input.GetButton("ControlLeft") && Input.GetButton("Fire1"));
        
        if(!isInDefense) return;
        
        speed = defenseSpeed;
        _isRunning = false;
    }

    private void Movement()
    {
        _horizontal = Input.GetAxisRaw("Horizontal");
        _vertical = Input.GetAxisRaw("Vertical");

        // Camera Style Controll
        if (currentStyle == Camerastyle.Exploration)
        {            
            Vector3 direction = new Vector3(_horizontal, 0f, _vertical).normalized;

            if (direction.magnitude >= 0.1f)
            {
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + _camera.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                controller.Move(moveDir.normalized * speed * Time.deltaTime);
            }

            if (mira.isActive) mira.Hide();
            
        }

        else if (currentStyle == Camerastyle.Combat)
        {
            
            // // Combat mode movement: Directly behind the player, player a bit to the left of screen
            Vector3 direction = transform.forward * _vertical + transform.right * _horizontal;

            if (direction.magnitude >= 0.1f)
            {
                Vector3 moveDir = direction.normalized;
                controller.Move(moveDir * speed * Time.deltaTime);
            }

            // Optionally, adjust the player's rotation to align with the camera's forward direction
            // This is if you want the player to always face the same direction as the camera in combat mode
            Vector3 forward = _camera.forward;
            forward.y = 0; // Keep the player rotation _horizontal
            transform.rotation = Quaternion.LookRotation(forward);
            // HandleCombatCameraZoom();

            if (!mira.isActive) mira.Show();
        }

        _animations.RuningAnimation(_isRunning);
        _animations.WalkingAnimation(_horizontal != 0 || _vertical != 0);
        // _animations.Jumping();
         _animations.JumpingInStop(!isGrounded);
        // _animations.IsFaling(isGrounded); // TODO: Animation: Este debería de ser
    }
    
    private void ReloadMunition()
    {
        if(Input.GetButtonDown("Reload"))
        {

        }
    }

    private void ChangeBulletWithWheel()
    {
        if(Input.GetButton("MunitionWheel"))
        {
            
        }
    }

    private void ChangeToLastMunitionUsed()
    {
        if(Input.GetButtonDown("LastBulletUsed"))
        {

        }
    }

    public void SwitchCameraStyle(Camerastyle newStyle)
    {
        // Store the current camera's forward direction before switching
        if (currentStyle == Camerastyle.Exploration)
        {
            lastCameraForward = explorationCam.transform.forward;
            lastCameraRotation = explorationCam.transform.rotation;
        }
        else if (currentStyle == Camerastyle.Combat)
        {
            lastCameraForward = combatCam.transform.forward;
            lastCameraRotation = combatCam.transform.rotation;
        }

        if (newStyle == Camerastyle.Exploration)
        {
            explorationCam.SetActive(true);
            combatCam.SetActive(false);
            // Apply the stored camera rotation to the new active camera
            explorationCam.transform.rotation = lastCameraRotation;
        }
        else if (newStyle == Camerastyle.Combat)
        {
            float xValueToSet = _cinemachineExploration.m_XAxis.Value;            
                        
            explorationCam.SetActive(false);
            combatCam.SetActive(true);
            // Apply the stored camera rotation to the new active camera

            if(newStyle != currentStyle) _cinemachineCombat.m_XAxis.Value = xValueToSet;         
            combatCam.transform.rotation = lastCameraRotation;
        }

        currentStyle = newStyle;
    }

    private IEnumerator Attack()
    {        
        Collider[] hitEnemies = Physics.OverlapSphere(attackSphere.position, attackRange, _enemyLayers);
        GameManager.Instance.PlayerDataUpdate(PlayerDataEnum.frequency_melee_attack);

        foreach (Collider enemyColl2D in hitEnemies)
        {
            IDamageable enemyImpact = enemyColl2D.GetComponent<IDamageable>();
            if(enemyImpact != null)
            {
                GameManager.Instance.PlayerDataUpdate(PlayerDataEnum.impact_melee_attack);
                enemyImpact.TakeDamage(1);
            }
        }

        AudioManager.Instance?.PlayOneShot(FMODEvents.instance.gaussClaws, transform.position);
        yield return new WaitForSeconds( 1.0f );        
    }

    public void TakeDamage(int pDamage)
    {
        int damage = Health - pDamage;

        if (damage > 0 && !_invulnerable)
        {
            Health = damage;
            StartCoroutine(InvulnerabilityEffect());
            GameManager.Instance.PlayerDataUpdate(PlayerDataEnum.damage_received);
        }

        if(damage <= 0)
        {
            Health = 0;
            GameManager.Instance.ProcessResultsAndSaveGameSession(LevelResultEnum.lose);
            GameManager.Instance.GameOver();            
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            _animations.KO();
        }
    }

    private IEnumerator InvulnerabilityEffect()
    {
        _invulnerable = true;
        _characterRenderer.material.color = _invulnerableColor;
        _armCharacterRenderer.material.color = _invulnerableColor;
        yield return new WaitForSeconds(_effectDuration);
        _characterRenderer.material.color = _originalColorCharacter;
        _armCharacterRenderer.material.color = _originalColorArm;
        _invulnerable = false;
    }
}

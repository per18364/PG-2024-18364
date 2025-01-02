using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;

[Serializable]
public struct Munitions
{
    public GameObject bulletPrefab;
    [Range(0, 100)] public int maximunAmmo;
    [Range(0, 100)] public int ammo;
    public Texture2D textureForFoodBox;
    [Range(0.1f, 1.0f)] public float shootRate;
}

[RequireComponent(typeof(ThirdPersonMovement))]
public class FoodBox : MonoBehaviour
{
    // [SerializeField] private MeshRenderer _texture;
    [SerializeField] private List<Munitions> _bulletPrefabs;
    [SerializeField] float bulletVelocity = 30;
    [SerializeField] float bulletPrefabLifetime = 3f;
    [SerializeField] private Transform bulletSpawn;
    [SerializeField] private Renderer objRenderer;
    [SerializeField] private float maxRange = 100f;
    [SerializeField] private bool infiniteMunition = false;
    
    private Texture _originalTextureFoodBox;
    private MunitionDisplay _ammunitionDisplay;

    private MunitionType _bulletSelected = MunitionType.BARRINGTONIA;
    // For Change Bullets from the controller
    private float crossAxisPrevious = 0f; // Track the previous frame's axis value
    private float crossThreshold = 0.1f; // Define the threshold for the axis    
    private float _canShoot = -1f;
    // [SerializeField] private Mira bullseye;

    private PlayerDataEnum[] _bulletsTypeRegister = new PlayerDataEnum[5];

    public MunitionType BulletSelected() => _bulletSelected;

    private Munitions _MuntionSelected { get { return _bulletPrefabs[(int)_bulletSelected]; }}

    private ThirdPersonMovement _player;
    private GausAnimations _animations;

    private Camera activeCamera;
    private Vector3 targetPoint;

    // Unity Events
    [Space]
    [Header("Bulls Eye Events")]

    [SerializeField] private UnityEvent bullsEyeDetected;
    [SerializeField] private UnityEvent bullsEyeFree;
    [SerializeField] private UnityEvent<int> bullsEyeUpdateColor;
        
    // Start is called before the first frame update
    private void Start()
    {        
        CinemachineBrain brainCamera = Camera.main.GetComponent<CinemachineBrain>();

         if (brainCamera != null)
        {
            brainCamera.m_CameraActivatedEvent.AddListener(OnCameraChanged);
        }
        else
        {
            Debug.LogError("CinemachineBrain not found on Main Camera!");
        }

        _player = GetComponent<ThirdPersonMovement>();
        _animations = GetComponent<GausAnimations>();

        if(_player == null) Debug.LogError("FoodBox.cs error: ThirdPersonMovement not found.");
        if(_animations == null) Debug.LogError("FoodBox.cs error: Gaus Animations not found.");
        
        // Start Bullets Type Register
        _bulletsTypeRegister[0] = PlayerDataEnum.frequency_barringtonia;
        _bulletsTypeRegister[1] = PlayerDataEnum.frequency_spaggetti;
        _bulletsTypeRegister[2] = PlayerDataEnum.frequency_jelly;
        _bulletsTypeRegister[3] = PlayerDataEnum.frequency_hot_tea;
        _bulletsTypeRegister[4] = PlayerDataEnum.frequency_cake;

        _ammunitionDisplay = FindObjectOfType<MunitionDisplay>();

        if(_bulletPrefabs.Count > 0)
        {
            _originalTextureFoodBox = objRenderer.material.mainTexture;
            ChangeTextureAndMaterialFoodBox();
        }
    }

    private void Update() 
    {
        ChangeBulletType();
        ChangeBulletWitKeys();
        ControlAim();

        // For Testing purposes
        if(Input.GetKeyDown(KeyCode.N))
        {
            AddMoreMunition(MunitionType.BARRINGTONIA, 5);
        }

        if(Input.GetKeyDown(KeyCode.M))
        {
            AddMoreMunition(MunitionType.SPAGGETTI, 5);
        }
    }

    private void ControlAim()
    {        
        float aimAxis = Input.GetAxis("Fire2");
        bool aimButton = Input.GetButton("Fire2");

        if(aimButton || aimAxis > 0.2f) /// Activar Mira
        {
            _animations?.ShootAnimation(true);
            _player?.SwitchCameraStyle(Camerastyle.Combat);
            ControlShootInAimMode();            
            return;
        }

        _animations?.ShootAnimation(false);
        _player?.SwitchCameraStyle(Camerastyle.Exploration);
    }

    private void ControlShootInAimMode() 
    {        
        if(_player == null) return;
        
        if(_player.IsInDefense) return;
                        
        float shootAxis = Input.GetAxis("Fire1");
        bool shotButton = Input.GetButton("Fire1");
                
        bool shootPressed = (shootAxis > 0.1f || shotButton) && Time.time > _canShoot;
    
        if(shootPressed && AmmoIsEnough())
        {
            // Play sound of munition empty
            return;
        }

        // TODO: Set here a Debug.DrawLine about the current raycast and how its pointing.
        Ray ray = activeCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                
        if (Physics.Raycast(ray, out RaycastHit hit, maxRange))
        {
            Shotable shotableTarget = hit.collider.GetComponent<Shotable>();
            
            if (shotableTarget != null)
            {
                targetPoint = hit.point;
                Debug.DrawLine(ray.origin, hit.point, Color.cyan, 2f);
                bullsEyeDetected.Invoke();
            }
            else
            {
                // targetPoint = hit.collider.transform.position;
                targetPoint = ray.origin + ray.direction * maxRange;
                bullsEyeFree.Invoke();
            }
        }
        else
        {
            targetPoint = ray.origin + ray.direction * maxRange;
            // Debug.DrawLine(ray.origin, targetPoint, Color.blue, 2f);
            bullsEyeFree.Invoke();
        }
                
        // Instantiate Bullet
        if (shootPressed)
        {       
            _animations?.ShootAnimation(true);                
            _canShoot = Time.time + _MuntionSelected.shootRate;
            Shoot();
        }
    }

    private void Shoot()
    {        
        Vector3 bulletDirection = (targetPoint - bulletSpawn.position).normalized;

        GameObject bullet = Instantiate(_bulletPrefabs[(int)_bulletSelected].bulletPrefab, 
                                        bulletSpawn.position, Quaternion.identity);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = bulletDirection * bulletVelocity;
            Debug.DrawLine(bulletSpawn.position, bulletSpawn.position + bulletDirection * maxRange, Color.yellow, 2f);
        }

        StartCoroutine(DestroyBulletAfterTime(bullet, bulletPrefabLifetime));
        Munitions munition = _bulletPrefabs[(int)_bulletSelected];

        if(!infiniteMunition) munition.ammo--;

        if(munition.ammo <= 0) 
        {
            munition.ammo = 0;
            objRenderer.material.mainTexture = _originalTextureFoodBox;
        }
        
        _bulletPrefabs[(int)_bulletSelected] = munition;
        _ammunitionDisplay.AmmoText(munition.ammo, munition.maximunAmmo);

        if (munition.ammo == 0)
        {
            objRenderer.material.mainTexture = _originalTextureFoodBox;
        }
    }


    void OnDestroy()
    {
        CinemachineBrain brainCamera = Camera.main.GetComponent<CinemachineBrain>();
        if (brainCamera != null)
        {
            brainCamera.m_CameraActivatedEvent.RemoveListener(OnCameraChanged);
        }
    }
    
    private void ChangeBulletType()
    {
        float crossAxis = Input.GetAxis("HorizontalCross");
                
        if (Input.GetAxis("Mouse ScrollWheel") > 0f || crossAxis >= crossThreshold && crossAxisPrevious < crossThreshold)
        {
            _bulletSelected++;
            if ((int)_bulletSelected >= _bulletPrefabs.Count)
                _bulletSelected = 0;

            _ammunitionDisplay.ChangeAmmo((int)_bulletSelected);
            ChangeTextureAndMaterialFoodBox();            
            bullsEyeUpdateColor.Invoke((int)_bulletSelected);
        }

        else if (Input.GetAxis("Mouse ScrollWheel") < 0f || crossAxis <= -crossThreshold && crossAxisPrevious > -crossThreshold)
        {
            _bulletSelected--;
            if ((int)_bulletSelected < 0)
                _bulletSelected = (MunitionType)(_bulletPrefabs.Count - 1);

            _ammunitionDisplay.ChangeAmmo((int)_bulletSelected);
            ChangeTextureAndMaterialFoodBox();        
            bullsEyeUpdateColor.Invoke((int)_bulletSelected);
        }
        
        crossAxisPrevious = crossAxis;
    }

    private void ChangeBulletWitKeys()
    {        
        if (Input.GetButtonDown("Bullet1"))
        {
            _bulletSelected = MunitionType.BARRINGTONIA;
            _ammunitionDisplay.ChangeAmmo((int)_bulletSelected);
            bullsEyeUpdateColor.Invoke((int)_bulletSelected);
            ChangeTextureAndMaterialFoodBox();
        }
        else if (Input.GetButtonDown("Bullet2") && _bulletPrefabs.Count >= 2)
        {
            _bulletSelected = MunitionType.SPAGGETTI;
            _ammunitionDisplay.ChangeAmmo((int)_bulletSelected);
            bullsEyeUpdateColor.Invoke((int)_bulletSelected);
            ChangeTextureAndMaterialFoodBox();
        }
        else if (Input.GetButtonDown("Bullet3") && _bulletPrefabs.Count >= 3)
        {
            _bulletSelected = MunitionType.JELLY;
            _ammunitionDisplay.ChangeAmmo((int)_bulletSelected);
            bullsEyeUpdateColor.Invoke((int)_bulletSelected);
            ChangeTextureAndMaterialFoodBox();
        }
        else if (Input.GetButtonDown("Bullet4") && _bulletPrefabs.Count >= 4)
        {
            _bulletSelected = MunitionType.HOT_TEA;
            _ammunitionDisplay.ChangeAmmo((int)_bulletSelected);
            bullsEyeUpdateColor.Invoke((int)_bulletSelected);
            ChangeTextureAndMaterialFoodBox();
        }
        else if (Input.GetButtonDown("Bullet5") && _bulletPrefabs.Count >= 5)
        {
            _bulletSelected = MunitionType.CAKE;
            _ammunitionDisplay.ChangeAmmo((int)_bulletSelected);
            bullsEyeUpdateColor.Invoke((int)_bulletSelected);
            ChangeTextureAndMaterialFoodBox();
        }                
    }

    private void ChangeTextureAndMaterialFoodBox()
    {        
        _ammunitionDisplay.AmmoText(_MuntionSelected.ammo, _MuntionSelected.maximunAmmo);
        objRenderer.material.mainTexture = _MuntionSelected.ammo > 0 ? _MuntionSelected.textureForFoodBox : _originalTextureFoodBox;
    }

    private void OnCameraChanged(ICinemachineCamera incomingCamera, ICinemachineCamera outgoingCamera)
    {
        Debug.Log($"Camera changed: Incoming = {incomingCamera?.Name}, Outgoing = {outgoingCamera?.Name}");

        // Update active camera if necessary
        activeCamera = CinemachineCore.Instance.GetActiveBrain(0)?.OutputCamera;

        if (activeCamera != null)
        {
            Debug.Log($"Active camera updated: {activeCamera.name}");
        }
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(bullet);
    }

    public void AddMoreMunition(MunitionType munitionSelected, int amount)
    {
        if((int)munitionSelected > _bulletPrefabs.Count)
        {
            print("Munition Selected Not Avaliable");
            return;
        }
        
        Munitions munition = _bulletPrefabs[(int)munitionSelected];
        munition.ammo += amount;
        
        if(munition.ammo > munition.maximunAmmo) munition.ammo = munition.maximunAmmo;

        _bulletPrefabs[(int)munitionSelected] = munition;
        
        _ammunitionDisplay.AmmoText(_MuntionSelected.ammo, _MuntionSelected.maximunAmmo);
        AudioManager.Instance?.PlayOneShot(FMODEvents.instance.brokeMunitionBox, transform.position);
        if(_MuntionSelected.ammo > 0) objRenderer.material.mainTexture = _MuntionSelected.textureForFoodBox;
    }

    public bool AmmoIsEnough() => _bulletPrefabs[(int)_bulletSelected].ammo <= 0;

    public void ReturnToNormalMunition() => infiniteMunition = false;
}

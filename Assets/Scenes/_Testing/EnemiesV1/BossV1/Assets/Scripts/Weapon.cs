using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{

    public Camera playerCamera;

    //shooting
    public bool isFiring, readyToFire;
    bool allowreset = true;
    public float shootingDelay = 0.5f;

    //burst
    public int burstCount = 3;
    public int burstBulletsLeft;

    //spread
    public float spreadIntensity;

    //bullet
    public GameObject[] bulletPrefab;
    // public GameObject[] bulletPrefab;
    public Transform bulletSpawn;
    public float bulletVelocity = 30;
    public float bulletLife = 2f;

    public enum WeaponType
    {
        Normal,
        Espaghetti
    }

    public enum FireMode
    {
        Single,
        Burst,
        Auto
    }

    public FireMode fireMode;
    public WeaponType weaponType;

    private void Awake()
    {
        readyToFire = true;
        burstBulletsLeft = burstCount;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (fireMode == FireMode.Auto)
        {
            isFiring = Input.GetKey(KeyCode.Mouse0);
        }
        else if (fireMode == FireMode.Single || fireMode == FireMode.Burst)
        {
            isFiring = Input.GetKeyDown(KeyCode.Mouse0);
        }

        if (isFiring && readyToFire)
        {
            burstBulletsLeft = burstCount;
            Fire();
        }
    }

    private void Fire()
    {
        readyToFire = false;

        Vector3 shootingDirection = CalculateDirectionAndSpread().normalized;

        GameObject bullet = null;

        if (weaponType == WeaponType.Normal)
        {
            bullet = Instantiate(bulletPrefab[0], bulletSpawn.position, Quaternion.identity);
        }
        else if (weaponType == WeaponType.Espaghetti)
        {
            bullet = Instantiate(bulletPrefab[1], bulletSpawn.position, Quaternion.identity);
        }

        bullet.transform.forward = shootingDirection;
        // Rigidbody rb = bullet.GetComponent<Rigidbody>();
        // rb.velocity = bulletSpawn.forward * bulletVelocity;
        bullet.GetComponent<Rigidbody>().AddForce(shootingDirection * bulletVelocity, ForceMode.Impulse);
        StartCoroutine(DestroyBullet(bullet, bulletLife));

        if (allowreset)
        {
            Invoke("ResetFire", shootingDelay);
            allowreset = false;
        }

        if (fireMode == FireMode.Burst && burstBulletsLeft > 1)
        {
            burstBulletsLeft--;
            Invoke("Fire", shootingDelay);
        }
    }

    private void ResetFire()
    {
        readyToFire = true;
        allowreset = true;
    }

    private Vector3 CalculateDirectionAndSpread()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(1000);
        }

        Vector3 direction = targetPoint - bulletSpawn.position;

        float xSpread = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);
        float ySpread = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);

        return direction + new Vector3(xSpread, ySpread, 0);
    }

    private IEnumerator DestroyBullet(GameObject bullet, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(bullet);
    }
}

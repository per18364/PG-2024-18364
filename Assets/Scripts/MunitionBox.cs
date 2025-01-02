using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MunitionBox : MonoBehaviour
{
    [Header("Munition Props")]
    [SerializeField] private MunitionType bulletMunition;
    [SerializeField] private int amountMunition = 30;

    [Header("Materials for the Icons to Select")]
    [SerializeField] private MeshRenderer[] _foodMeshRederers;
    [SerializeField] private Material[] _foodMaterials;

    [Header("Rotation Vectors")]
    [SerializeField] private Vector3 _rotationVectors;
    [SerializeField] private float _speedRotation = 5;

    [Header("GameObjects for customize the Munition Box")]
    [SerializeField] private GameObject[] _barringotiansVisuals;
    [SerializeField] private GameObject _spaguettiBall;
    
    [Header("For Particle System")]
    [SerializeField] private Material[] _materialParticles;
    private ParticleSystem _particleSystem;
    private ParticleSystemRenderer _particleSystemRenderer;

    private void Start() 
    {
        _particleSystem = GetComponentInChildren<ParticleSystem>();

        if(_particleSystem == null) Debug.LogError("MunitionBox.cs: Particle System is null.");
        else _particleSystemRenderer = _particleSystem.GetComponent<ParticleSystemRenderer>();
        
        if(bulletMunition == MunitionType.BARRINGTONIA)
        {
            // Active elements
            foreach (GameObject barringtonias in _barringotiansVisuals) barringtonias.SetActive(true);
            
            // Deactive elements
            _spaguettiBall.SetActive(false);

            _particleSystemRenderer.material = _materialParticles[0];
            SetAllIcons();
        }

        if(bulletMunition == MunitionType.SPAGGETTI)
        {
            // Active elements
            _spaguettiBall.SetActive(true);

            // Deactive elements
            foreach (GameObject barringtonias in _barringotiansVisuals) barringtonias.SetActive(false);

            _particleSystemRenderer.material = _materialParticles[1];
            SetAllIcons();
        }
    }

    private void Update() 
    {
        transform.Rotate(
            Mathf.Sin(_rotationVectors.x) * _speedRotation,  
            Mathf.Sin(_rotationVectors.y) * _speedRotation, 
            Mathf.Sin(_rotationVectors.z) * _speedRotation,
            Space.Self
        );
    }

    private void SetAllIcons()
    {
        foreach (MeshRenderer foodMeshRenderer in _foodMeshRederers)
        {
            Material[] mats = foodMeshRenderer.materials;
            mats[0] = _foodMaterials[(int)bulletMunition];
            foodMeshRenderer.materials = mats;
        }
    }

    private void OnTriggerEnter(Collider other) 
    {
        if(other.gameObject.CompareTag("Player"))
        {
            // ThirdPersonMovement gaus = other.gameObject.GetComponent<ThirdPersonMovement>();
            FoodBox foodBox = other.gameObject.GetComponent<FoodBox>();
            
            if(foodBox == null)
            {
                return;            
            }

            foodBox.AddMoreMunition(bulletMunition, amountMunition);
            Destroy(gameObject);
        }    
    }
}

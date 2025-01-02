using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;

public class Bullet : MonoBehaviour
{   
    [Header("Bullet Special Effects Properties")]
    [SerializeField] private GameObject _munitionObject;
    [SerializeField] private float _speedRotation;
    [SerializeField] private Vector3 _rotationVectors;
    private Rigidbody _rb;

    
    private void Start() 
    {
        if(!_munitionObject) Debug.LogError("Bullet.cs: Munition Object is null.");

        _rb = GetComponent<Rigidbody>();
        if(_rb == null) Debug.LogError("Bullet.cs: Rigidbody is null.");
    }
    
    // Update is called once per frame
    void Update()
    {
        if(!_munitionObject) return;
        
        _munitionObject.transform.Rotate(
            Mathf.Sin(_rotationVectors.x) * _speedRotation,  
            Mathf.Sin(_rotationVectors.y) * _speedRotation, 
            Mathf.Sin(_rotationVectors.z) * _speedRotation,
            Space.Self
        );
    }

    public void RandomizeDirection(float speedMultiplier = 1f)
    {
        if (_rb == null) return;
                
        Vector3 randomDirection = Random.insideUnitSphere.normalized;        
        _rb.velocity = randomDirection * _rb.velocity.magnitude * speedMultiplier;
    }

    // private void OnCollisionEnter(Collision collision) 
    // {
    //     // if (collision.gameObject.CompareTag("Target"))
    //     // {
    //     //     print("hit " + collision.gameObject.name + "!");
    //     // }
    //     Destroy(gameObject);
    // }
}

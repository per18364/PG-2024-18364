using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WheelWoodInteraction : Shotable
{
    [SerializeField] private Animator _wheelAnimation;
    [SerializeField] private UnityEvent _extraConsequences;
    private bool _turned = false;
    
    public bool GetIsTurned() => _turned;
    
    private void Start() 
    {
        this.gameObject.SetActive(false);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.GetComponent<SpaghettiScript>() && !_turned)
        {
            StartCoroutine(TurnWheelAction());            
        }
    }
        
    private IEnumerator TurnWheelAction()
    {        
        _turned = true;
        _wheelAnimation.SetTrigger("Turn");
        _extraConsequences.Invoke();
        yield return new WaitForSeconds(1.5f);        
    }
}

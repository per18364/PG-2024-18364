using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuhoAnimations : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    public void FlyAnimation(bool flyAnimation)
    {
        _animator.SetBool("Fly", flyAnimation);
    }
    
}

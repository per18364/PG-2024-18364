using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DingoAnimations : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    public void WalkingAnimation(bool walkingAnimation)
    {
        _animator.SetBool("Walking", walkingAnimation);
    }

    public void RuningAnimation(bool runningAnimation)
    {
        _animator.SetBool("Running", runningAnimation);
    }

    public void KO()
    {
        _animator.SetTrigger("Ko");
    }

    public void ThrowingBoomAnimation()
    {
        _animator.SetTrigger("Throwing_Boom");
    }
}


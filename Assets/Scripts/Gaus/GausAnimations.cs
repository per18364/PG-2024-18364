using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GausAnimations : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    public void WalkingAnimation(bool walkingAnimation)
    {
        _animator.SetBool("Walking", walkingAnimation);
    }

    public void RuningAnimation(bool runningAnimation)
    {
        _animator.SetBool("Ruuning", runningAnimation);
    }

    public void Jumping()
    {
        _animator.SetTrigger("Jumping");
    }

    public void JumpingInStop(bool jumpingOnAirAnimation)
    {
        _animator.SetBool("JumpingOnAir", jumpingOnAirAnimation);
    }

    public void ShootAnimation(bool isShooting)
    {
        _animator.SetBool("Shooting", isShooting);
    }

    public void KO()
    {
        _animator.SetTrigger("Ko");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    [SerializeField] private Animator _doorsAnimator;

    public void OpenDoor()
    {
        _doorsAnimator.SetTrigger("Open");
    }
}

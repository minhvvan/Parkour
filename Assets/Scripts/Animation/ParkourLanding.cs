using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkourLanding : StateMachineBehaviour
{
    private ParkourCharacterController _controller;
    
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _controller = animator.gameObject.GetComponent<ParkourCharacterController>();
        if (!_controller) return;
    }
}

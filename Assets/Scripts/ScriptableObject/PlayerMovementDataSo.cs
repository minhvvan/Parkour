using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMovementData", menuName = "SO/PlayerMovementData")]
public class PlayerMovementDataSo : ScriptableObject
{
    public float moveSpeed;
    public float sprintSpeed;
    public float jumpForce;
    public float gravity;
    public float jumpCoolDown = .5f;


    public LayerMask groundLayer;
    
    public float groundedOffset = -0.14f;
    public float groundedRadius = 0.28f;

    public float rotationSmoothTime;
    public float speedChangeRate;
}

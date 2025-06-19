using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMovementData", menuName = "SO/PlayerMovementData")]
public class PlayerMovementDataSo : ScriptableObject
{
    // Default
    public float gravity;
    
    // Move
    public float moveSpeed;
    public float sprintSpeed;
    public float decelerateRate = 2f;
    public float rotationSmoothTime;
    public float speedChangeRate;
    
    // Jump
    public float jumpForce;
    public float jumpCoolDown = .5f;
    public LayerMask groundLayer;
    public float groundedOffset = -0.14f;
    public float groundedRadius = 0.28f;
    public float fallThreshold;

    // Slope
    public float slopeLimit;
    public float slopeThreshold;
    public float slopeResistanceMultiplier;
}

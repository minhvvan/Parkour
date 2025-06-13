using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMovementData", menuName = "SO/PlayerMovementData")]
public class PlayerMovementDataSo : ScriptableObject
{
    public float moveSpeed;
    public float jumpForce;
    public float gravity;

    public float rotationSmoothTime;
    public float speedChangeRate;
}

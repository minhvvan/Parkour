using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class FootTracker : MonoBehaviour
{
    [SerializeField] private PlayerMovementDataSo playerMovementData;
    
    [SerializeField] private Transform leftFoot;
    [SerializeField] private Transform rightFoot;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float rayDistance = 0.5f;
    [SerializeField] private float footStepOffset;
    [SerializeField] private float footUpOffset;

    public Vector3 groundNormal { get; private set; }
    public float groundAngle { get; private set; }
    public bool isOnSlope { get; private set; }
    public Vector3 leftFootPosition { get; private set; }
    public Vector3 rightFootPosition { get; private set; }
    public bool isUp { get; private set; }

    private Vector3 _leftGroundPos;
    private Vector3 _rightGroundPos;
    
    private void Update()
    {
        leftFootPosition = leftFoot.transform.position;
        rightFootPosition = rightFoot.transform.position;

        UpdateFootGroundInfo();
    }

    private void UpdateFootGroundInfo()
    {
        // 삼점 탐지
        var currentHit = GetGroundHit(transform.position + transform.up * footUpOffset);
        var forwardHit = GetGroundHit(transform.position + transform.forward * footStepOffset + transform.up * footUpOffset);
        var rightHitPoint = currentHit.point + transform.right * footStepOffset;

        groundNormal = Vector3.Cross(forwardHit.point - currentHit.point, rightHitPoint - currentHit.point);
        groundAngle = Vector3.Angle(groundNormal, Vector3.up);
        isOnSlope = groundAngle > playerMovementData.slopeThreshold;
        isUp = forwardHit.point.y > currentHit.point.y;
    }
    
    private RaycastHit GetGroundHit(Vector3 startPos)
    {
        Physics.SphereCast(startPos, .1f, Vector3.down, out var hit, rayDistance, groundLayer);
        return hit;
    }
    
    private void OnDrawGizmosSelected()
    {
        // 지면 법선 벡터 표시 (파란색)
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, groundNormal * 10f);
   
        // 경사 상태에 따른 색상 표시
        if (isOnSlope)
        {
            if (groundAngle > playerMovementData.slopeLimit)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, 0.5f);
            }
            else
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, 0.5f);
            }
        }
        else
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
   
        // 삼점 위치 표시 (작은 구)
        Gizmos.color = Color.cyan;
        var currentHit = GetGroundHit(transform.position + transform.up * footUpOffset);
        var forwardHit = GetGroundHit(transform.position + transform.forward * footStepOffset + transform.up * footUpOffset);
        var rightHitPoint = currentHit.point + transform.right * footStepOffset;
   
        Gizmos.DrawWireSphere(currentHit.point, 0.1f);
        Gizmos.DrawWireSphere(forwardHit.point, 0.1f);
        Gizmos.DrawWireSphere(rightHitPoint, 0.1f);
    }
}

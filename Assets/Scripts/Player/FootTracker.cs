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
    public bool isCliff { get; private set; }

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
        var rightHit = GetGroundHit(transform.position + transform.right * footStepOffset + transform.up * footUpOffset);

        // 높이 차이로 각도 계산
        float heightDiff = currentHit.point.y - forwardHit.point.y;
        float horizontalDist = footStepOffset;
        float slopeAngle = Mathf.Atan2(heightDiff, horizontalDist) * Mathf.Rad2Deg;
    
        Debug.Log(slopeAngle);
        
        if (slopeAngle > 80f)  // 수직 낙하
        {
            isCliff = true;
            isOnSlope = false;
        }
        else if (slopeAngle > 60f)  // 가파른 경사 (슬라이딩)
        {
            isCliff = false;
            isOnSlope = true;
        }
        else
        {
            // 기존 3점 법선 계산으로 일반 경사 체크
            groundNormal = Vector3.Cross(forwardHit.point - currentHit.point, rightHit.point - currentHit.point);
            groundAngle = Vector3.Angle(groundNormal, Vector3.up);
            isOnSlope = groundAngle > playerMovementData.slopeThreshold;
        }
    }
    
    private RaycastHit GetGroundHit(Vector3 startPos)
    {
        Physics.Raycast(startPos, Vector3.down, out var hit, rayDistance, groundLayer);
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
        var rightHit = GetGroundHit(transform.position + transform.right * footStepOffset + transform.up * footUpOffset);
   
        Gizmos.DrawWireSphere(currentHit.point, 0.1f);
        Gizmos.DrawWireSphere(forwardHit.point, 0.1f);
        Gizmos.DrawWireSphere(rightHit.point, 0.1f);
    }
}

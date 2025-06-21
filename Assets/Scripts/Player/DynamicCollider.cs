using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DynamicCollider : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private GameObject rootBone;
    [SerializeField] private float heightPadding;
    [SerializeField] private float radiusPadding;
    
    
    private List<Transform> _allBones;
    private Bounds _bounds;

    private void Start()
    {
        CollectAllBones();
    }

    private void Update()
    {
        UpdateBoneBound();
    }

    private void CollectAllBones()
    {
        if (rootBone == null) return;
        
        _allBones = new List<Transform>();
        
        for (int i = 0; i < rootBone.transform.childCount; i++)
        {
            CollectBonesRecursively(rootBone.transform.GetChild(i));
        }
    }

    private void CollectBonesRecursively(Transform bone)
    {
        _allBones.Add(bone);
        
        for (int i = 0; i < bone.childCount; i++)
        {
            CollectBonesRecursively(bone.GetChild(i));
        }
    }
    
    private void UpdateBoneBound()
    {
        if (_allBones == null || _allBones.Count == 0) return;
        
        // 모든 본의 현재 위치로 바운드 계산
        _bounds = CalculateBoundsFromAllBones();
        
        // CharacterController 업데이트
        characterController.height = _bounds.size.y + heightPadding;
        characterController.radius = Mathf.Min(_bounds.size.x, _bounds.size.z) * 0.5f + radiusPadding;
        
        // 중심점을 로컬 좌표로 변환
        Vector3 localCenter = transform.InverseTransformPoint(_bounds.center);
        characterController.center = new Vector3(0, localCenter.y, 0);
    }

    private Bounds CalculateBoundsFromAllBones()
    {
        // 모든 본의 위치를 한번에 추출
        var positions = _allBones.Select(bone => bone.position);
    
        // Min/Max 계산
        Vector3 min = new Vector3(
            positions.Min(p => p.x),
            positions.Min(p => p.y),
            positions.Min(p => p.z)
        );
    
        Vector3 max = new Vector3(
            positions.Max(p => p.x),
            positions.Max(p => p.y),
            positions.Max(p => p.z)
        );
    
        Vector3 center = (min + max) * 0.5f;
        Vector3 size = max - min;
    
        return new Bounds(center, size);
    }
}

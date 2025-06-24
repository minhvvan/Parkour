using System.Collections;
using System.Collections.Generic;
using Interface;
using UnityEngine;

public class ParkourSystem : MonoBehaviour
{
    [SerializeField] LayerMask parkourLayer;
    [SerializeField] float detectDistance;
    
    public IParkourObject DetectParkourObjects()
    {
        IParkourObject result = null;

        //전방 ray
        if (Physics.CapsuleCast(transform.position + Vector3.up, transform.position, .1f, transform.forward, out var hit, detectDistance, parkourLayer))
        {
            if (hit.collider.TryGetComponent<IParkourObject>(out var parkourObject))
            {
                result = parkourObject;
            }
        }
        
        return result;
    }
}

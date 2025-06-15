using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class FootTracker : MonoBehaviour
{
    [SerializeField] private GameObject leftFoot;
    [SerializeField] private GameObject rightFoot;

    public Vector3 leftFootPosition;
    public Vector3 rightFootPosition;

    private void Update()
    {
        leftFootPosition = leftFoot.transform.position;
        rightFootPosition = rightFoot.transform.position;
    }
}

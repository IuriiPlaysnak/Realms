using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointGravity : MonoBehaviour
{
    Rigidbody rbody;
    private void Awake()
    {
        rbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate ()
    {
        foreach (PointGravityMass m in PointGravityMass.Masses)
        {
            Vector3 offset = m.transform.position - transform.position;
            float range = offset.magnitude;
            rbody.AddForce((offset * m.Mass * rbody.mass) / (range * range));// *range));
        }		
	}
}

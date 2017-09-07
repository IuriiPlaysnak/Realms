using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointGravityMass : MonoBehaviour
{
    public float Mass;

    public static List<PointGravityMass> Masses;

    private void Awake()
    {
        if (Masses == null)
            Masses = new List<PointGravityMass>();

        Masses.Add(this);
    }
}

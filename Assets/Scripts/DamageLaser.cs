using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageLaser : MonoBehaviour
{
    public float Size = 1;
    public float Falloff = 1;
    public float Strength = 1;

    private void Update()
    {
        Ray ray = new Ray(transform.position, -transform.up);
        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            if(hit.collider.TryGetComponent(out DamageBuffer buffer))
            {
                buffer.RegisterHit(hit.point, Size, Falloff, Strength);
            }
        }
    }
}

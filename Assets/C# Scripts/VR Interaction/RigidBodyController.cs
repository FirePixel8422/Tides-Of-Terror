using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class RigidBodyController : MonoBehaviour
{
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Automatically calculate and set the center of mass
        rb.centerOfMass = CalculateCenterOfMass();
    }

    private Vector3 CalculateCenterOfMass()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();

        for (int i = 0; i < colliders.Length; i++)
        {
            for (int i2 = 0; i2 < colliders.Length; i2++)
            {
                Physics.IgnoreCollision(colliders[i], colliders[i2]);
            }
        }

        if (colliders.Length == 0)
        {
            Debug.LogWarning("No colliders found. Using default center of mass.");
            return Vector3.zero; // Default center of mass
        }

        Vector3 totalWeightedPosition = Vector3.zero;
        float totalMass = 0f;

        foreach (Collider coll in colliders)
        {
            if (coll.isTrigger)
            {
                continue;
            }

            float colliderMass = coll.bounds.size.x * coll.bounds.size.y * coll.bounds.size.z; // Approximate mass
            totalWeightedPosition += coll.bounds.center * colliderMass;
            totalMass += colliderMass;
        }

        if (totalMass > 0)
        {
            return transform.InverseTransformPoint(totalWeightedPosition / totalMass); // Convert to local space
        }

        return Vector3.zero; // Fallback
    }

    private void OnDrawGizmos()
    {
        if (rb != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.TransformPoint(rb.centerOfMass), 0.1f); // Visualize center of mass
        }
    }
}

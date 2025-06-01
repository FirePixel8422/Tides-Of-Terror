using NUnit.Framework.Constraints;
using UnityEngine;


public class CannonTrail : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;

    [SerializeField] private Transform startPoint;

    [SerializeField] private AnimationCurve curve;
    [SerializeField] private int positionCount;

    [SerializeField] private float distanceMultiplier = 1f;
    [SerializeField] private float heightMultiplier = 1f;



    [ContextMenu("Calculate Trail")]
    private void OnValidate()
    {
        if (lineRenderer == null || positionCount <= 0)
        {
            return;
        }

        Vector3[] positions = new Vector3[positionCount];

        for (int i = 0; i < positionCount; i++)
        {
            float percent = (float)i / positionCount;

            positions[i] = startPoint.position + (startPoint.forward * percent * distanceMultiplier + Vector3.up * curve.Evaluate(percent) * heightMultiplier);
        }

        lineRenderer.positionCount = positionCount;
        lineRenderer.SetPositions(positions);
    }
}

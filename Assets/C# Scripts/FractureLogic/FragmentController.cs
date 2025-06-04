using UnityEngine;


public class FragmentController : MonoBehaviour
{
    [SerializeField] private float explosionForce;
    [SerializeField] private float explosionRadius;
    [SerializeField] private float upwardsModifier;

    [SerializeField] private float decayDelay;
    [SerializeField] private float decaySpeed;

    [SerializeField] private VibrationParamaters vibrationWhenBroken;



#if UNITY_EDITOR

    [ContextMenu("Shatter This Object")]
    private void ShatterEditor()
    {
        if (Application.isPlaying)
        {
            Shatter(transform.position);
        }
    }

#endif


    public void Shatter(Vector3 shatterCenterPoint)
    {
        FragmentScalar shatterObj = GetComponentInChildren<FragmentScalar>(true);

        print(shatterObj == null);

        Rigidbody[] shatterPieces = shatterObj.GetComponentsInChildren<Rigidbody>(true);

        shatterObj.transform.parent = null;
        shatterObj.gameObject.SetActive(true);
        shatterObj.StartCoroutine(shatterObj.SchrinkFragments(shatterPieces, decayDelay, decaySpeed));

        int shatterPieceCount = shatterPieces.Length;
        for (int i = 0; i < shatterPieceCount; i++)
        {
            shatterPieces[i].AddExplosionForce(Random.Range(0, explosionForce), shatterCenterPoint, explosionRadius, upwardsModifier, ForceMode.VelocityChange);
        }

        Hand.Left.SendVibration(vibrationWhenBroken);
        Hand.Right.SendVibration(vibrationWhenBroken);
    }
}

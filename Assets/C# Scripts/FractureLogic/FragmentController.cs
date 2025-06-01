using UnityEngine;


public class FragmentController : MonoBehaviour
{
    [SerializeField] private float explosionForce;
    [SerializeField] private float explosionRadius;
    [SerializeField] private float upwardsModifier;

    [SerializeField] private float decayDelay;
    [SerializeField] private float decaySpeed;

    [SerializeField] private VibrationParamaters vibrationWhenBroken;

    [SerializeField] private AudioClip[] audioClips;
    [SerializeField] private float minVolume, maxVolume;



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

        Rigidbody[] shatterPieces = shatterObj.GetComponentsInChildren<Rigidbody>(true);

        shatterObj.transform.parent = null;
        shatterObj.gameObject.SetActive(true);
        shatterObj.StartCoroutine(shatterObj.SchrinkFragments(shatterPieces, decayDelay, decaySpeed));

        int shatterPieceCount = shatterPieces.Length;
        for (int i = 0; i < shatterPieceCount; i++)
        {
            shatterPieces[i].AddExplosionForce(Random.Range(0, explosionForce), shatterCenterPoint, explosionRadius, upwardsModifier, ForceMode.VelocityChange);
        }

        if (audioClips.Length != 0)
        {
            AudioSource source = new GameObject("audio").AddComponent<AudioSource>();

            source.pitch = Random.Range(0.95f, 1.05f);

            source.volume = Random.Range(minVolume, maxVolume);

            source.clip = audioClips[Random.Range(0, audioClips.Length)];

            source.Play();

            Destroy(source, source.clip.length + 0.25f);
        }

        Destroy(gameObject);

        Hand.Left.SendVibration(vibrationWhenBroken);
        Hand.Right.SendVibration(vibrationWhenBroken);
    }
}

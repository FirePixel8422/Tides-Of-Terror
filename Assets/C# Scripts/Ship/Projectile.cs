using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float damage;

    [SerializeField] private AudioClip[] impactAudioClips;
    [SerializeField] private float minVolume, maxVolume;

    private AudioSource audioSource;
    private Renderer meshRenderer;



    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.TryGetComponent(out MonsterCore monster))
        {
            monster.Hit(damage);
        }

        if (TryGetComponent(out FragmentController fragmentController))
        {
            fragmentController.Shatter(transform.position + transform.forward);
        }

        if (impactAudioClips.Length != 0)
        {
            meshRenderer.enabled = false;

            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.volume = Random.Range(minVolume, maxVolume);

            audioSource.clip = impactAudioClips[Random.Range(0, impactAudioClips.Length)];

            audioSource.Play();

            Destroy(gameObject, audioSource.clip.length + 0.25f);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

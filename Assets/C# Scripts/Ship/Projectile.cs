using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float damage;

    [SerializeField] private ProjectileHitType hitType = ProjectileHitType.Fracture;
    [SerializeField] private bool onlyBreakOnMonster;

    [SerializeField] private AudioClip[] impactAudioClips;
    [SerializeField] private float minVolume, maxVolume;

    private AudioSource audioSource;
    private Renderer meshRenderer;
    private Collider coll;



    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        meshRenderer = GetComponent<Renderer>();
        coll = GetComponent<Collider>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.TryGetComponent(out MonsterCore monster))
        {
            monster.Hit(damage);
        }
        else if (onlyBreakOnMonster == false)
        {
            return;
        }
        coll.enabled = false;


        switch (hitType)
        {
            case ProjectileHitType.None:

                meshRenderer.enabled = false;

                break;

            case ProjectileHitType.Fracture:

                if (TryGetComponent(out FragmentController fragmentController))
                {
                    fragmentController.Shatter(transform.position + transform.forward);
                }

                meshRenderer.enabled = false;

                break;

            case ProjectileHitType.Stick:

                transform.SetParent(collision.transform, true);

                break;
        }


        if (impactAudioClips.Length != 0)
        {
            meshRenderer.enabled = false;

            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.volume = Random.Range(minVolume, maxVolume);

            audioSource.clip = impactAudioClips[Random.Range(0, impactAudioClips.Length)];

            audioSource.Play();

            DestroyObj(audioSource.clip.length + 0.25f);
        }
        else
        {
            DestroyObj();
        }
    }

    private void DestroyObj(float time = 0)
    {
        if (hitType != ProjectileHitType.Stick)
        {
            Destroy(gameObject, time);
        }
        else
        {
            foreach (Component comp in GetComponents<Component>())
            {
                if (comp is Transform || comp is MeshRenderer || comp is MeshFilter) continue;

                Destroy(comp, time);
            }
        }
    }
}
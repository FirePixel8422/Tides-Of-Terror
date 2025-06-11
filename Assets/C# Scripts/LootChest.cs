using UnityEngine;


public class LootChest : Interactable
{
    [SerializeField] private Interactable thropy;

    private ParticleSystem coinParticles;
    private Animator anim;

    private bool opened;


    protected override void Start()
    {
        base.Start();

        thropy.interactable = false;

        coinParticles = GetComponentInChildren<ParticleSystem>();
        anim = GetComponent<Animator>();
    }

    public override void Pickup(InteractionController handInteractor)
    {
        base.Pickup(handInteractor);

        if (opened == false)
        {
            OpenChest();
            opened = true;
        }
    }


    [ContextMenu("Open Chest")]
    public void OpenChest()
    {
        anim.SetTrigger("Open");
    }

    public void PlayCoinParticles()
    {
        coinParticles.Play();

        Invoke(nameof(DestroyChest), coinParticles.main.duration);
    }

    public void DestroyChest()
    {
        thropy.transform.SetParent(BoatEngine.Instance.transform, true);
        thropy.interactable = true;

        Destroy(gameObject);
    }
}

using UnityEngine;



public class InteractableButton : Interactable
{
    private Animator buttonAnimator;


    protected override void Start()
    {
        base.Start();

        buttonAnimator = GetComponent<Animator>();
    }


    public override void Pickup(InteractionController hand)
    {
        buttonAnimator.SetTrigger("Press");
    }
}

using UnityEngine;



public class InteractableButton : Interactable
{
    private Animator buttonAnimator;


    protected override void Start()
    {
        buttonAnimator = GetComponent<Animator>();
    }


    public override void Pickup(InteractionController handInteractor)
    {
        buttonAnimator.SetTrigger("Press");
    }
}

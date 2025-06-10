using UnityEngine.UI;


public class VRButton : Interactable
{
    private Button button;


    protected override void Start()
    {
        base.Start();

        button = GetComponent<Button>();
    }


    public override void Pickup(InteractionController handInteractor)
    {
        base.Pickup(handInteractor);

        button?.onClick.Invoke();
    }
}

using System.Collections;
using UnityEngine;

public class InfiniPickupable : Interactable
{
    public Pickupable pickupablePrefab;


    public override void Pickup(InteractionController hand)
    {
        Pickupable foodObj = Instantiate(pickupablePrefab, hand.transform.position, hand.transform.rotation);

        StartCoroutine(Delay(hand, foodObj));
    }


    private IEnumerator Delay(InteractionController hand, Interactable pickupableObj)
    {
        yield return new WaitForEndOfFrame();

        hand.Pickup(pickupableObj);
    }
}

using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;




[BurstCompile]
public class Interactable : MonoBehaviour
{
    //[HideInInspector]
    public InteractionController connectedHand;

    public bool interactable = true;
    public bool isThrowable = true;
    public bool heldByPlayer;

    public float objectSize;

    [SerializeField] protected UnityEvent OnInteract;


    protected virtual void Start()
    {

    }


    #region Select And Deselect

    [BurstCompile]
    public virtual void OnSelect()
    {

    }

    [BurstCompile]
    public virtual void OnDeSelect()
    {

    }

    #endregion




    #region Pickup, Throw And Drop

    [BurstCompile]
    public virtual void Pickup(InteractionController handInteractor)
    {
        if (connectedHand != null)
        {
            connectedHand.isHoldingObject = false;
        }

        connectedHand = handInteractor;
        heldByPlayer = true;

        OnInteract?.Invoke();
    }




    [BurstCompile]
    public virtual void Throw(HandType handType, float3 velocity, float3 angularVelocity)
    {
        connectedHand = null;
        heldByPlayer = false;
    }




    [BurstCompile]
    public virtual void Drop(HandType handType)
    {
        connectedHand = null;
        heldByPlayer = false;
    }

    #endregion




    private void OnDestroy()
    {
        interactable = false;

        if (connectedHand != null)
        {
            connectedHand.isHoldingObject = false;
        }
    }


    private void OnValidate()
    {
        if (!Application.isPlaying && transform.TryFindObjectOfType(out Hand hand) && hand.interactionController.Settings != null)
        {
            gameObject.layer = Mathf.RoundToInt(Mathf.Log(hand.interactionController.Settings.interactablesLayer.value, 2));
        }
    }


    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, objectSize);
    }
}

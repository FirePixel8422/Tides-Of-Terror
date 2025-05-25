using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;




public class Interactable : MonoBehaviour
{
    public bool interactable = true;
    public bool isThrowable = true;
    public bool heldByPlayer;

    public float objectSize;

    [SerializeField] protected UnityEvent OnInteract;


    protected virtual void Start() { }

    public virtual void OnSelect() { }
    public virtual void OnDeSelect() { }



    public virtual void Pickup(InteractionController handInteractor)
    {
        heldByPlayer = true;

        OnInteract?.Invoke();
    }

    public virtual void Drop(HandType handType)
    {
        heldByPlayer = false;
    }

    public virtual void Throw(HandType handType, float3 velocity, float3 angularVelocity)
    {
        heldByPlayer = false;
    }


    protected virtual void OnDestroy()
    {
        interactable = false;
    }



#if UNITY_EDITOR

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

#endif
}

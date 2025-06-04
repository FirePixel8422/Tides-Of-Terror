using System;
using Unity.Burst;
using UnityEngine;
using UnityEngine.InputSystem;


[BurstCompile]
public class InteractionController : MonoBehaviour
{
    [HideInInspector]
    public Hand hand;

    public HandInteractionSettingsSO Settings;


    [SerializeField]
    private Transform rayTransform;

    [SerializeField]
    private Transform overlapSphereTransform;

    public Transform HeldItemHolder;


    private Interactable heldObject;
    public bool objectHeld;

    private Interactable toPickupObject;
    public bool objectSelected;


    private Collider[] hitObjectsInSphere;
    private RaycastHit rayHit;



    [BurstCompile]
    public void OnClick(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (objectHeld == false && objectSelected)
            {
                Pickup(toPickupObject);
            }
        }

        if (ctx.canceled && objectHeld)
        {
            Drop();
        }
    }


    [BurstCompile]
    private void Start()
    {
        hand = GetComponent<Hand>();

        hitObjectsInSphere = new Collider[Settings.maxExpectedObjectInSphere];


        if (Settings.shouldThrowVelAddMovementVel)
        {
            bodyMovementTransform = transform.root;
        }

        savedLocalVelocity = new Vector3[frameAmount];
        savedAngularVelocity = new Vector3[frameAmount];

        UpdateScheduler.RegisterUpdate(OnUpdate);
    }



    [BurstCompile]
    public void OnUpdate()
    {
        //if you are holding nothing, scan for objects by using a raycast and a sphere around you hand
        if (objectHeld == false)
        {
            UpdateToPickObject();
        }

        //if you are holding something and it is throwable (Or "pickupsUseOldHandVel" is true), start doing velocity calculations
        if (Settings.pickupsUseOldHandVel || (heldObject != null && heldObject))
        {
            CalculateHandVelocity();
        }
    }




    #region UpdateToPickupObject

    [BurstCompile]
    private void UpdateToPickObject()
    {
        if (Settings.interactionPriorityMode == InteractionPriorityMode.SphereTrigger)
        {
            //if "GrabState.OnSphereTrigger" is true (OverlapSphere is enabled)
            if (Settings.grabState.HasFlag(GrabState.OnSphereTrigger) && CreateOverlapSphere(overlapSphereTransform.position, Settings.overlapSphereSize, Settings.interactablesLayer))
            {
                return;
            }

            //if overlapSphere missed and "GrabState.OnRaycast" is true (rayCasts are enabled) and there are no objects near your hand, check if there is one in front of your hand
            if (Settings.grabState.HasFlag(GrabState.OnRaycast) && ShootRayCast())
            {
                return;
            }
        }
        else
        {
            //if "GrabState.OnRaycast" is true (rayCasts are enabled) and there are no objects near your hand, check if there is one in front of your hand
            if (Settings.grabState.HasFlag(GrabState.OnRaycast) && ShootRayCast())
            {
                return;
            }

            //if raycast missed and "GrabState.OnSphereTrigger" is true (OverlapSphere is enabled)
            if (Settings.grabState.HasFlag(GrabState.OnSphereTrigger) && CreateOverlapSphere(overlapSphereTransform.position, Settings.overlapSphereSize, Settings.interactablesLayer))
            {
                return;
            }
        }

        //deselect potential previous selected object if no object is in range anymore
        DeSelectObject();
    }




    /// <returns>OverlapSphere Succes State</returns>
    [BurstCompile]
    private bool CreateOverlapSphere(Vector3 overlapSphereTransformPos, float overlapSphereSize, int interactablesLayer)
    {
        //get all objects near your hand
        int objectsInSphereCount = Physics.OverlapSphereNonAlloc(overlapSphereTransformPos, overlapSphereSize, hitObjectsInSphere, interactablesLayer);


        //resize array if there are too little spots in the Collider Array "hitObjectsInSphere"
        if (objectsInSphereCount > hitObjectsInSphere.Length)
        {
            Debug.LogWarning("Too Little Interaction Slots, Sphere check was resized");

            hitObjectsInSphere = Physics.OverlapSphere(overlapSphereTransformPos, overlapSphereSize, interactablesLayer);
        }


        //if there is atleast 1 object in the sphere
        if (objectsInSphereCount > 0)
        {
            float closestObjectDistance = 10000;
            Interactable new_ToPickupObject = null;


            //calculate closest object
            for (int i = 0; i < objectsInSphereCount; i++)
            {
                if (hitObjectsInSphere[i].TryGetComponent(out Interactable targetObject))
                {
                    float distanceToTargetObject = Vector3.Distance(overlapSphereTransformPos, targetObject.transform.position);

                    if (distanceToTargetObject - targetObject.objectSize < closestObjectDistance)
                    {
                        new_ToPickupObject = targetObject;
                        closestObjectDistance = distanceToTargetObject;
                    }
                }
            }

            //if you are holdijg nothing, or the new_ToPickupObject isnt already selected, select the object and deselect potential previous selected object
            if (objectSelected == false || new_ToPickupObject != toPickupObject)
            {
                SelectNewObject(new_ToPickupObject);
            }

            //new object found
            return true;
        }

        //no new object found
        return false;
    }


    /// <returns>RayCast Succes State</returns>
    [BurstCompile]
    private bool ShootRayCast()
    {
        Ray ray = new Ray(rayTransform.position, rayTransform.forward);

        if (Physics.Raycast(ray, out rayHit, Settings.interactRayCastRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide)
                && rayHit.transform.TryGetComponent(out Interactable new_ToPickupObject))
        {
            //if you are holdijg nothing, or the new_ToPickupObject isnt already selected, select the object and deselect potential previous selected object
            if (objectSelected == false || new_ToPickupObject != toPickupObject)
            {
                SelectNewObject(new_ToPickupObject);
            }

            //new object found
            return true;
        }

        //no new object found
        return false;
    }

    #endregion




    #region Select/Deselect Object

    [BurstCompile]
    private void SelectNewObject(Interactable new_ToPickupObject)
    {
        if (objectSelected)
        {
            toPickupObject.OnDeSelect();
        }

        toPickupObject = new_ToPickupObject;

        objectSelected = true;

        hand.SendVibration(Settings.selectPickupVibrationParams);
    }

    [BurstCompile]
    private void DeSelectObject()
    {
        if (objectSelected)
        {
            toPickupObject.OnDeSelect();
        }

        objectSelected = false;
    }

    #endregion




    #region Drop and Pickup

    [BurstCompile]
    public void Pickup(Interactable toPickupObject)
    {
        //if the object that is trying to be picked up by this hand, is held by the other hand and canSwapItemFromHands is false, return
        if (toPickupObject.interactable == false || (toPickupObject.heldByPlayer && Settings.canSwapItemFromHands == false))
        {
            return;
        }

        toPickupObject.Pickup(this);

        heldObject = toPickupObject;
        objectHeld = true;

        hand.SendVibration(Settings.pickupVibrationParams);
    }


    [BurstCompile]
    private void Drop()
    {
        //drop item if it is throwable
        if (heldObject.isThrowable)
        {
            Vector3 velocity = Vector3.zero;
            for (int i = 0; i < frameAmount; i++)
            {
                velocity += savedLocalVelocity[i] / frameAmount;
            }

            Vector3 angularVelocity = Vector3.zero;
            for (int i = 0; i < frameAmount; i++)
            {
                angularVelocity += savedAngularVelocity[i] / frameAmount;
            }

            heldObject.Throw(hand.handType, velocity, 0, angularVelocity);
        }
        else
        {
            heldObject.Drop(hand.handType);
        }

        heldObject = null;
        objectHeld = false;
    }

    #endregion




    #region CalculateHandVelocity

    private Transform bodyMovementTransform;
    private Vector3 prevbodyTransformPos;

    private Vector3 prevTransformPos;
    private Vector3[] savedLocalVelocity;

    private Quaternion prevRotation;
    private Vector3[] savedAngularVelocity;

    [Range(1, 32)]
    public int frameAmount;
    private int frameIndex;



    [BurstCompile]
    private void CalculateHandVelocity()
    {
        //Calculate velocity based on hand movement
        savedLocalVelocity[frameIndex] = bodyMovementTransform.rotation * (transform.localPosition - prevTransformPos) * Settings.throwVelocityMultiplier / Time.deltaTime;

        prevTransformPos = transform.localPosition;


        //Add velocity based on player body
        if (Settings.shouldThrowVelAddMovementVel)
        {
            savedLocalVelocity[frameIndex] += (bodyMovementTransform.localPosition - prevbodyTransformPos) / Time.deltaTime;

            prevbodyTransformPos = bodyMovementTransform.localPosition;
        }


        //Calculate Angular velocity based on hand rotation
        Quaternion deltaRotation = transform.rotation * Quaternion.Inverse(prevRotation);
        deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);

        if (angle > 180f) angle -= 360f;
        savedAngularVelocity[frameIndex] = axis * (angle * Mathf.Deg2Rad / Time.deltaTime);

        prevRotation = transform.rotation;




        frameIndex += 1;
        if (frameIndex == frameAmount)
        {
            frameIndex = 0;
        }
    }

    #endregion


    private void OnDestroy()
    {
        UpdateScheduler.UnregisterUpdate(OnUpdate);
    }

    public void ForceDrop() { }
}
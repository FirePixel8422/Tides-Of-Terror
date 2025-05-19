using System;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;


[BurstCompile]
public class InteractionController : MonoBehaviour
{
    [HideInInspector]
    public Hand hand;

    [SerializeField] private HandInteractionSettingsSO settings;
    public HandInteractionSettingsSO Settings => settings;


    [SerializeField] private Transform boatTransform;
    [SerializeField] private Transform rayTransform;
    [SerializeField] private Transform overlapSphereTransform;


    [SerializeField] private Transform heldItemHolder;
    public Transform HeldItemHolder => heldItemHolder;


    private Interactable heldObject;
    public bool isHoldingObject;

    private Interactable toPickupObject;
    private bool objectSelected;

    private VrButton toClickButton;
    private bool uiSelected;


    private Collider[] hitObjectsInSphere;
    private RaycastHit rayHit;



    [BurstCompile]
    public void OnClick(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (uiSelected)
            {
                toClickButton.Click();

                print("ui clicked called");

                uiSelected = false;
            }

            if (isHoldingObject == false && objectSelected)
            {
                Pickup(toPickupObject);
            }
        }

        if (ctx.canceled && isHoldingObject)
        {
            Drop();
        }
    }


    private void Start()
    {
        hand = GetComponent<Hand>();

        hitObjectsInSphere = new Collider[settings.maxExpectedObjectInSphere];


        if (settings.shouldThrowVelAddMovementVel)
        {
            bodyMovementTransform = transform.root;
        }

        savedLocalVelocity = new float3[maxFrames];
        savedAngularVelocity = new float3[maxFrames];
    }


    private void OnEnable() => UpdateScheduler.Register(OnUpdate);
    private void OnDisable() => UpdateScheduler.Unregister(OnUpdate);



    private void OnUpdate()
    {
        //if you are holding nothing, scan for objects by using a raycast and a sphere around you hand
        if (isHoldingObject == false)
        {
            UpdateToPickObject();
        }
    }




    #region UpdateToPickupObject

    private void UpdateToPickObject()
    {
        if (settings.interactionPriorityMode == InteractionPriorityMode.SphereTrigger)
        {
            //if "GrabState.OnSphereTrigger" is true (OverlapSphere is enabled)
            if (settings.grabState.HasFlag(GrabState.OnSphereTrigger) && CreateOverlapSphere(overlapSphereTransform.position, settings.overlapSphereSize, settings.interactablesLayer))
            {
                return;
            }

            //if overlapSphere missed and "GrabState.OnRaycast" is true (rayCasts are enabled) and there are no objects near your hand, check if there is one in front of your hand
            if (settings.grabState.HasFlag(GrabState.OnRaycast) && ShootRayCast())
            {
                return;
            }
        }
        else
        {
            //if "GrabState.OnRaycast" is true (rayCasts are enabled) and there are no objects near your hand, check if there is one in front of your hand
            if (settings.grabState.HasFlag(GrabState.OnRaycast) && ShootRayCast())
            {
                return;
            }

            //if raycast missed and "GrabState.OnSphereTrigger" is true (OverlapSphere is enabled)
            if (settings.grabState.HasFlag(GrabState.OnSphereTrigger) && CreateOverlapSphere(overlapSphereTransform.position, settings.overlapSphereSize, settings.interactablesLayer))
            {
                return;
            }
        }

        //deselect potential previous selected object if no object is in range anymore
        DeSelectObject();
    }




    /// <returns>OverlapSphere Succes State</returns>
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
    private bool ShootRayCast()
    {
        Ray ray = new Ray(rayTransform.position, rayTransform.forward);

        if (Physics.Raycast(ray, out rayHit, settings.interactRayCastRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide)
                && rayHit.transform.TryGetComponent(out Interactable new_ToPickupObject))
        {
            //if you are holdijg nothing, or the new_ToPickupObject isnt already selected, select the object and deselect potential previous selected object
            if (objectSelected == false || new_ToPickupObject != toPickupObject)
            {
                SelectNewObject(new_ToPickupObject);
            }

            uiSelected = false;

            //new object found
            return true;
        }
        else if (Physics.Raycast(ray, out RaycastHit hit, settings.interactRayCastRange, settings.interactablesLayer))
        {
            //print(hit.transform.name);
            if (hit.transform.gameObject.TryGetComponent(out VrButton newToClickButton))
            {
                //return if already selected
                if (toClickButton == newToClickButton)
                {
                    return false;
                }

                toClickButton = newToClickButton;

                uiSelected = true;

                hand.SendVibration(settings.selectPickupVibrationParams);
                return true;
            }
        }
        else
        {
            uiSelected = false;
        }

        //no new object found
        return false;
    }

    #endregion


    #region Select/Deselect Object

    private void SelectNewObject(Interactable new_ToPickupObject)
    {
        if (objectSelected)
        {
            toPickupObject.OnDeSelect();
        }

        toPickupObject = new_ToPickupObject;

        objectSelected = true;

        hand.SendVibration(settings.selectPickupVibrationParams);
    }

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

    private void Pickup(Interactable toPickupObject)
    {
        //if the object that is trying to be picked up by this hand, is held by the other hand and canSwapItemFromHands is false, return
        if (toPickupObject.interactable == false || (toPickupObject.heldByPlayer && settings.canSwapItemFromHands == false))
        {
            return;
        }

        toPickupObject.Pickup(this);

        heldObject = toPickupObject;
        isHoldingObject = true;

        hand.SendVibration(settings.pickupVibrationParams);
    }


    private void Drop()
    {
        //drop item if it is throwable
        if (heldObject.isThrowable)
        {
            float3 velocity = float3.zero;
            for (int i = 0; i < maxFrames; i++)
            {
                velocity += savedLocalVelocity[i] / maxFrames;
            }

            float3 angularVelocity = float3.zero;
            for (int i = 0; i < maxFrames; i++)
            {
                angularVelocity += savedAngularVelocity[i] / maxFrames;
            }

            heldObject.Throw(hand.handType, velocity, angularVelocity);
        }
        else
        {
            heldObject.Drop(hand.handType);
        }

        heldObject = null;
        isHoldingObject = false;
    }

    public void ForceDrop()
    {
        if (isHoldingObject)
        {
            heldObject.Drop(hand.handType);

            heldObject = null;
            isHoldingObject = false;
        }
    }

    #endregion




    #region CalculateHandVelocity

    private Transform bodyMovementTransform;
    private float3 prevbodyTransformPos;

    private float3 prevTransformPos;
    private float3[] savedLocalVelocity;

    private quaternion prevRotation;
    private float3[] savedAngularVelocity;

    [Range(1, 32)]
    public int maxFrames;
    private int frameIndex;



    private void FixedUpdate()
    {
        //if you are holding something and it is throwable (Or "pickupsUseOldHandVel" is true), start doing velocity calculations
        if (settings.pickupsUseOldHandVel || (heldObject != null && heldObject))
        {
            CalculateHandVelocity();
        }
    }

    private void CalculateHandVelocity()
    {
        //store and remove boatTransform (parent) rotation influence
        Quaternion boatRot = boatTransform.rotation;
        boatTransform.transform.rotation = Quaternion.identity;

        //Calculate velocity based on hand movement
        savedLocalVelocity[frameIndex] = bodyMovementTransform.rotation * ((float3)transform.localPosition - prevTransformPos) * settings.throwVelocityMultiplier / Time.fixedDeltaTime;

        prevTransformPos = transform.localPosition;

        //Add velocity based on player body
        if (settings.shouldThrowVelAddMovementVel)
        {
            savedLocalVelocity[frameIndex] += ((float3)bodyMovementTransform.localPosition - prevbodyTransformPos) / Time.fixedDeltaTime;

            prevbodyTransformPos = bodyMovementTransform.localPosition;
        }

        //Calculate Angular velocity based on hand rotation
        Quaternion deltaRotation = transform.rotation * Quaternion.Inverse(prevRotation);
        deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);

        if (angle > 180f) angle -= 360f;
        savedAngularVelocity[frameIndex] = axis * (angle * Mathf.Deg2Rad / Time.fixedDeltaTime);

        prevRotation = transform.rotation;

        frameIndex = (frameIndex + 1) % maxFrames;

        //restore boat rotation
        boatTransform.rotation = boatRot;
    }

    #endregion




    public void DEBUG_ForcePickup(Interactable toPickupObject)
    {
        toPickupObject.Pickup(this);

        heldObject = toPickupObject;
        isHoldingObject = true;
    }
}
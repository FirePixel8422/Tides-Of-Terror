using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class InteractionController : MonoBehaviour
{
    [HideInInspector]
    public Hand hand;

    [SerializeField] private HandInteractionSettingsSO settings;
    public HandInteractionSettingsSO Settings => settings;

    [SerializeField] private Transform rayTransform;
    [SerializeField] private Transform overlapSphereTransform;


    [SerializeField] private Transform heldItemHolder;
    public Transform HeldItemHolder => heldItemHolder;


    private Interactable heldObject;
    public bool objectHeld;

    private Interactable toPickupObject;
    public bool objectSelected;


    private Collider[] hitObjectsInSphere;
    private RaycastHit rayHit;


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

    private void Start()
    {
        hand = GetComponent<Hand>();

        hitObjectsInSphere = new Collider[settings.maxExpectedObjectInSphere];


        if (settings.shouldThrowVelAddMovementVel)
        {
            bodyMovementTransform = transform.root;
        }

        savedLocalThrowVelocity = new Vector3[frameAmount];
        savedLocalBodyVelocity = new Vector3[frameAmount];
        savedAngularVelocity = new Vector3[frameAmount];
    }


    private void OnEnable() => UpdateScheduler.RegisterUpdate(OnUpdate);
    private void OnDisable() => UpdateScheduler.UnregisterUpdate(OnUpdate);

    public void OnUpdate()
    {
        //if you are holding nothing, scan for objects by using a raycast and a sphere around you hand
        if (objectHeld == false)
        {
            UpdateToPickObject();
        }

        //if you are holding something and it is throwable (Or "pickupsUseOldHandVel" is true), start doing velocity calculations
        if (settings.pickupsUseOldHandVel || (heldObject != null && heldObject))
        {
            CalculateHandVelocity();
        }


#if UNITY_EDITOR
        DEBUG_Update();
#endif
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

            //new object found
            return true;
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

    public void Pickup(Interactable toPickupObject)
    {
        //if the object that is trying to be picked up by this hand, is held by the other hand and canSwapItemFromHands is false, return
        if (toPickupObject.interactable == false || (toPickupObject.heldByPlayer && settings.canSwapItemFromHands == false))
        {
            return;
        }

        toPickupObject.Pickup(this);

        heldObject = toPickupObject;
        objectHeld = true;

        hand.SendVibration(settings.pickupVibrationParams);
    }


    private void Drop()
    {
        //drop item if it is throwable
        if (heldObject.isThrowable)
        {
            Vector3 throwVelocity = Vector3.zero;
            Vector3 bodyVelocity = Vector3.zero;
            for (int i = 0; i < frameAmount; i++)
            {
                throwVelocity += savedLocalThrowVelocity[i] / frameAmount;
                bodyVelocity += savedLocalBodyVelocity[i] / frameAmount;
            }

            Vector3 angularVelocity = Vector3.zero;
            for (int i = 0; i < frameAmount; i++)
            {
                angularVelocity += savedAngularVelocity[i] / frameAmount;
            }

            heldObject.Throw(hand.handType, throwVelocity * settings.throwVelocityMultiplier, bodyVelocity, angularVelocity);
        }
        else
        {
            heldObject.Drop(hand.handType);
        }

        heldObject = null;
        objectHeld = false;
    }

    public void ForceDrop()
    {
        if (objectHeld)
        {
            heldObject.Drop(hand.handType);

            heldObject = null;
            objectHeld = false;
        }
    }

    #endregion


    #region CalculateHandVelocity

    private Transform bodyMovementTransform;
    private Vector3 prevbodyTransformPos;

    private Vector3 prevTransformPos;
    private Vector3[] savedLocalThrowVelocity;
    private Vector3[] savedLocalBodyVelocity;

    private Quaternion prevRotation;
    private Vector3[] savedAngularVelocity;

    [Range(1, 32)]
    public int frameAmount;
    private int frameIndex;


    private void CalculateHandVelocity()
    {
        //Calculate velocity based on hand movement
        savedLocalThrowVelocity[frameIndex] = bodyMovementTransform.rotation * (transform.localPosition - prevTransformPos) / Time.deltaTime;

        prevTransformPos = transform.localPosition;


        //Add velocity based on player body
        if (settings.shouldThrowVelAddMovementVel)
        {
            savedLocalBodyVelocity[frameIndex] += (bodyMovementTransform.localPosition - prevbodyTransformPos) / Time.deltaTime;

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


    

#if UNITY_EDITOR

    [SerializeField] private bool DEBUG_ControlMode;
    [SerializeField] private float DEBUG_MoveSpeed = 1f;
    [SerializeField] private float DEBUG_RotSpeed = 1f;
    private void DEBUG_Update()
    {
        if (DEBUG_ControlMode == false) return;

        Vector3 move = Vector3.zero;
        float rot = 0;

        if (Input.GetKey(KeyCode.W)) move += transform.forward;
        if (Input.GetKey(KeyCode.S)) move -= transform.forward;
        if (Input.GetKey(KeyCode.A)) move -= transform.right;
        if (Input.GetKey(KeyCode.D)) move += transform.right;
        if (Input.GetKey(KeyCode.LeftShift)) move -= transform.up;
        if (Input.GetKey(KeyCode.Space)) move += transform.up;

        if (Input.GetKey(KeyCode.Q)) rot -= 1;
        if (Input.GetKey(KeyCode.E)) rot += 1;

        transform.position += move * DEBUG_MoveSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up, rot * DEBUG_RotSpeed * Time.deltaTime);

        if (Input.GetMouseButtonDown(0)) DEBUG_Pickup();
        if (Input.GetMouseButtonUp(0) && objectHeld) Drop();
    }


    [ContextMenu("Pickup (Debug, Forced)")]
    private void DEBUG_ForcePickup()
    {
        if (objectSelected == false) return;

        toPickupObject.Pickup(this);

        heldObject = toPickupObject;
        objectHeld = true;
    }



    [ContextMenu("Pickup")]
    private void DEBUG_Pickup()
    {
        if (objectSelected == false) return;

        //if the object that is trying to be picked up by this hand, is held by the other hand and canSwapItemFromHands is false, return
        if (toPickupObject.interactable == false || (toPickupObject.heldByPlayer && settings.canSwapItemFromHands == false))
        {
            return;
        }

        toPickupObject.Pickup(this);

        heldObject = toPickupObject;
        objectHeld = true;

        hand.SendVibration(settings.pickupVibrationParams);
    }

#endif
}
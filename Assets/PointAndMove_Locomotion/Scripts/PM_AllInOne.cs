using System;
using System.Collections        ;
using System.Collections.Generic;
using UnityEngine               ;
using VRTK                      ;

namespace PointAndMove
{
    [AddComponentMenu("PointAndMove_Locomotion/Scripts/PM_AllInOne")]
    public class PM_AllInOne : MonoBehaviour
    {
        //Point and Move Functions
        void applyTranslation()
        {
            Vector3 newPos = new Vector3(direction.x * velocity + playArea.position.x, direction.y * velocity + playArea.position.y, direction.z * velocity + playArea.position.z);

            playArea.position = newPos;
        }

        void calcVelocity()
        {
            if (behavior == moveType.sticky)
            {
                velocity = Vector3.Distance(currPosition, prevPosition) * sensitivity;
            }
            else
            {
                velocity = velocity + Vector3.Distance(currPosition, prevPosition) * sensitivity/10 ;

                if (velocity > 0)
                {
                    velocity = velocity - degredation / 2500;
                }
                if (velocity < 0)
                {
                    velocity = 0;
                }
            }
        }

        void setDirection()
        {
            Vector3 calculatedDirection = (trackedController != null ? trackedController.gameObject.transform.rotation : Quaternion.identity) * Vector3.forward;

            direction = calculatedDirection;
        }

        //Behavior mechanics.
        void floaty()
        {
            if (trackedController.buttonTwoPressed)
            {
                setDirection();

                velocity = 0;
            }
            else
            {
                if (trackedController.triggerClicked)
                {
                    calcVelocity();
                }
            }
        }

        void semiPerpetual()
        {
            if (trackedController.buttonTwoPressed)
            {
                setDirection();

                velocity = 0;
            }
            else
            {
                if (trackedController.triggerClicked && triggerPrevState == false)
                {
                    velocity     = 0;

                    calcVelocity();
                }
                else if (trackedController.triggerClicked)
                {
                    calcVelocity();
                }
                else
                {
                    velocity = 0;
                }
            }
        }

        void sticky()
        {
            if (trackedController.buttonTwoPressed)
            {
                setDirection();

                velocity = 0;
            }
            else
            {
                if (trackedController.triggerClicked)
                {
                    calcVelocity();
                }
                else
                {
                    velocity = 0;
                }
            }
        }


        //Setup
        void setController()
        {
            leftController  = VRTK_DeviceFinder.GetControllerLeftHand ();
            rightController = VRTK_DeviceFinder.GetControllerRightHand();

            if (controllerSel == controller.left) { Debug.Log("Left Hand Selected" ); trackedController = leftController .GetComponent<VRTK_ControllerEvents>(); }
            else                                  { Debug.Log("Right Hand Selected"); trackedController = rightController.GetComponent<VRTK_ControllerEvents>(); }
        }

        void awake()   //Not sure if using this yet...
        { Debug.Log("awake func of PM_AllInOne called."); }

        void onEnable()   //Setting up locoomotion during start function.
        {
            Debug.Log("onEnable func of PM_AllInOne called.");

            Debug.Log("Current behavior: "+  behavior);

            playArea = VRTK_DeviceFinder.PlayAreaTransform();

            if (playArea != null) { Debug.Log("Play area referenced: " + playArea.gameObject); }

            setController();
        }


        // Use this for initialization
        void Start()
        {
            direction = Vector3.zero;

            acceleration = 0;
            velocity     = 0;

            onEnable();
        }


        // Update is called once per frame
        void Update()
        {
            try
            {
                if (trackedController.enabled && playArea.gameObject != null)
                {
                    currPosition = trackedController.gameObject.transform.position - playArea.position;

                    switch (behavior)
                    {
                        case moveType.floaty:
                            floaty();
                            break;
                        case moveType.semiPerpetual:
                            semiPerpetual();
                            break;
                        case moveType.sticky:
                            sticky();
                            break;
                    }

                    applyTranslation();

                    prevPosition = currPosition;

                    triggerPrevState = trackedController.triggerClicked;
                }
            }
            catch (NullReferenceException)
            {
                Debug.Log("Null Refrence exception for a component occured. Attempting to set...");

                playArea = VRTK_DeviceFinder.PlayAreaTransform();

                setController();

                if (playArea != null)
                    Debug.Log("Sucess for play area. Current Play Area: " + playArea.gameObject);
                else
                    Debug.Log("The play area is throwing a null refrence, could not get the refrence.");

                if (trackedController != null)
                    Debug.Log("Sucess for controller. Current Controller: " + trackedController.gameObject);
                else
                    Debug.Log("The tracked controller is throwing a null refrence, could not get the refrence.");
            }
        }


        public enum moveType { sticky, semiPerpetual, floaty }
        public enum controller { left, right }

        //Locomotion Related
        //Configurable by user.
        [Header("Configuration")]

        [Tooltip("Specifies the controller to be used for locomotion.")]
        public controller controllerSel = controller.left;

        [Tooltip("Determines the behavior of the movement.")]
        public moveType behavior = moveType.sticky;

        [Tooltip("Specifies the sensifivity factor for acceleration.")]
        public float sensitivity = 1.0f;

        [Tooltip("Specifies the degredation factor for acceleration.")]
        public float degredation = 1.0f;

        [Tooltip("Button useed to confrim direction to travel.")]
        public VRTK_ControllerEvents.ButtonAlias pointConfirm = VRTK_ControllerEvents.ButtonAlias.ButtonOnePress;

        [Tooltip("Button used to kill velocity.")]
        public VRTK_ControllerEvents.ButtonAlias stop = VRTK_ControllerEvents.ButtonAlias.GripClick;

        [Tooltip("Binding for activating travel behavior.")]
        public VRTK_ControllerEvents.ButtonAlias activateTranslation = VRTK_ControllerEvents.ButtonAlias.TriggerClick;

        //Private & Protected
        private bool triggerPrevState;

        private float acceleration;
        private float velocity    ;

        private Vector3 currPosition;
        private Vector3 prevPosition;

        private Transform playArea    ;

        private Vector3 direction;

        //Controller Related
        protected GameObject leftController ;
        protected GameObject rightController;

        protected VRTK_ControllerEvents trackedController;
    }
}
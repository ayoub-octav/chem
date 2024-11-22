using UnityEngine;
using UnityEngine.XR.Hands;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;

namespace ChemistryVR.Controller
{
    public class SwipeGesture : MonoBehaviour
    {
        public float timeThreshold = 0.2f;
        public float swipeThreshold = 0.2f;
        public float distanceThreshold = 3f;
        public float coolDown = .5f;
        public Transform hand;
        public int velocitySmoothingFrames = 5; // Number of frames to smooth the velocity
        public MyHands targetHand;
        public enum MyHands
        {
            Left,
            right
        }
        private bool isGestureActive;
        private XRHandSubsystem subSystem;
        private static readonly List<XRHandSubsystem> subsystemsReuse = new List<XRHandSubsystem>();

        private float timeCounter;
        private float previousVelocity;
        private bool checkSwipe = true;
        private Queue<float> velocityQueue = new Queue<float>();
        private Transform target;
        private XRHand xRHand;

        [HideInInspector] public UnityEvent onSwipeLeft;
        [HideInInspector] public UnityEvent onSwipeRight;

        private void Start()
        {
            target = GameObject.Find("Book").transform;
            ResetSwipe();
        }

        private void Update()
        {
            if (!checkSwipe)
            {
                return;
            }
            if (isGestureActive)
            {
                if (subSystem == null || !subSystem.running)
                {
                    SubsystemManager.GetSubsystems(subsystemsReuse);
                    for (var i = 0; i < subsystemsReuse.Count; ++i)
                    {
                        var handSubsystem = subsystemsReuse[i];
                        if (handSubsystem.running)
                        {
                            subSystem = handSubsystem;
                            break;
                        }
                    }
                }

                if (DistanceChecker())
                {
                    if (subSystem != null && subSystem.running)
                    {
                       
                        switch (targetHand)
                        {
                            case MyHands.Left:
                                xRHand = subSystem.leftHand;
                                break;
                            case MyHands.right:
                                xRHand = subSystem.rightHand;
                                break;
                        }
                        XRHandJoint joint = xRHand.GetJoint(XRHandJointID.MiddleTip);
                        if (joint.TryGetLinearVelocity(out var velocity))
                        {
                            float smoothedVelocity = SmoothVelocity(velocity.x);
                            SwipeChecker(smoothedVelocity);
                        }
                    }
                }
            }
        }

        private bool DistanceChecker()
        {
            float distance = Vector3.Distance(hand.position, target.position);
            return distance <= distanceThreshold;
        }

        private float SmoothVelocity(float newVelocity)
        {
            velocityQueue.Enqueue(newVelocity);
            if (velocityQueue.Count > velocitySmoothingFrames)
            {
                velocityQueue.Dequeue();
            }

            float averageVelocity = 0f;
            foreach (var vel in velocityQueue)
            {
                averageVelocity += vel;
            }

            return averageVelocity / velocityQueue.Count;
        }

        private void SwipeChecker(float velocity)
        {
            if (timeCounter > 0)
            {
                if (Mathf.Abs(velocity) >= swipeThreshold)
                {
                    if (previousVelocity == 0)
                    {
                        previousVelocity = velocity;
                    }

                    timeCounter -= Time.deltaTime;

                    if (velocity * previousVelocity < 0)
                    {
                        ResetSwipe();
                    }
                }
                else
                {
                    ResetSwipe();
                }
            }
            else
            {
                if (previousVelocity > 0)
                {
                    SwipeRight();
                }
                else if (previousVelocity < 0)
                {
                    SwipeLeft();
                }
            }
        }

        private void ResetSwipe()
        {
            timeCounter = timeThreshold;
            previousVelocity = 0;
            //i reset the velocity to o 
            velocityQueue.Clear(); 
        }

        private void SwipeRight()
        {
            StartCoroutine(CoolDown());
            onSwipeRight?.Invoke();
        }

        private void SwipeLeft()
        {
            StartCoroutine(CoolDown());
            onSwipeLeft?.Invoke();
        }

        private IEnumerator CoolDown()
        {
            ResetSwipe();
            checkSwipe = false;
            yield return new WaitForSeconds(coolDown);
            checkSwipe = true;
            ResetSwipe();
        }

        public void SetGestureState(bool state)
        {
            isGestureActive = state;
        }
    }
}

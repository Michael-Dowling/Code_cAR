//-----------------------------------------------------------------------
// <copyright file="HelloARController.cs" company="Google">
//
// Copyright 2017 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace GoogleARCore.Examples.HelloAR
{
    using System.Collections.Generic;
    using GoogleARCore;
    using GoogleARCore.Examples.Common;
    using UnityEngine;

#if UNITY_EDITOR
    // Set up touch input propagation while using Instant Preview in the editor.
    using Input = InstantPreviewInput;
#endif

    /// <summary>
    /// Controls the HelloAR example.
    /// </summary>
    public class Controller : MonoBehaviour
    {
        /// <summary>
        /// The first-person camera being used to render the passthrough camera image (i.e. AR background).
        /// </summary>
        public Camera FirstPersonCamera;

        /// <summary>
        /// A prefab for tracking and visualizing detected planes.
        /// </summary>
        public GameObject DetectedPlanePrefab;

        /// <summary>
        /// A model to place when a raycast from a user touch hits a plane.
        /// </summary>
        public GameObject AndyPlanePrefab;

        /// <summary>
        /// A model to place when a raycast from a user touch hits a feature point.
        /// </summary>
        public GameObject AndyPointPrefab;

        /// <summary>
        /// A game object parenting UI for displaying the "searching for planes" snackbar.
        /// </summary>
        public GameObject SearchingForPlaneUI;

        public GameObject Wall;

        public GameObject Star;

        public GameObject PlacingTrack;

        public static bool renderCar = true;

    /// <summary>
    /// The rotation in degrees need to apply to model when the Andy model is placed.
    /// </summary>
        private const float k_ModelRotation = 180.0f;

        /// <summary>
        /// A list to hold all planes ARCore is tracking in the current frame. This object is used across
        /// the application to avoid per-frame allocations.
        /// </summary>
        private List<DetectedPlane> m_AllPlanes = new List<DetectedPlane>();

        /// <summary>
        /// True if the app is in the process of quitting due to an ARCore connection error, otherwise false.
        /// </summary>
        private bool m_IsQuitting = false;

        private GameObject car;

        private float time = 0.0f;
        private bool move;
        private bool placed = false;
        private Vector3 initPos;
        private Vector3 dest;
        private int orientation;
        private Vector3 start;
        private UnityEngine.Quaternion startRotation;
        private TrackableHit initHit;
        private bool setCourse;

        void Start()
        {
            if (!DataScript.initialized)
            {
                initData();
            }
            
            DataScript.moveCtr = 0;
            move = false;
            DataScript.moves = "";
        }


        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            
            _UpdateApplicationLifecycle();

            // Hide snackbar when currently tracking at least one plane.
            Session.GetTrackables<DetectedPlane>(m_AllPlanes);

            bool showSearchingUI = true;
            for (int i = 0; i < m_AllPlanes.Count; i++)
            {
                if (m_AllPlanes[i].TrackingState == TrackingState.Tracking)
                {
                    showSearchingUI = false;

                    break;
                }
            }

            SearchingForPlaneUI.SetActive(showSearchingUI);

            if (DataScript.resetCar)
            {
                car = null;
                DataScript.resetCar = false;
                GameObject prefab = AndyPlanePrefab;
                car = Instantiate(prefab, start, startRotation);
                car.transform.position = start;
                car.transform.rotation = startRotation;


                // Compensate for the hitPose rotation facing away from the raycast (i.e. camera).
                car.transform.Rotate(0, k_ModelRotation, 0, Space.Self);

                var anchor = initHit.Trackable.CreateAnchor(initHit.Pose);
                // Make Andy model a child of the anchor.
                car.transform.parent = anchor.transform;

                car.tag = "Player";

                var rb = car.AddComponent<Rigidbody>();
                rb.position = start;


                BoxCollider bc = car.AddComponent<BoxCollider>();
                bc.size = new Vector3(0.1f, 0.1f, 0.1f);
                bc.isTrigger = true;
                rb.useGravity = false;
                move = false;

                
                orientation = 0;
            }

            if (car != null && DataScript.isRunning)
            {
                MoveCreation.showControls = false;
                if (!move)
                {
                    if (DataScript.moveCtr >= DataScript.moves.Length)
                    {
                        DataScript.isRunning = false;
                        Destroy(car);
                        CrashedMenuController.codeFinished = true;
                    }
                    else
                    {
                        char c = DataScript.moves[DataScript.moveCtr];
                        switch (c)
                        {
                            case 'F':
                                initForward();
                                break;
                            case 'R':
                                rotate(true);
                                break;
                            case 'L':
                                rotate(false);
                                break;
                        }
                        DataScript.moveCtr++;
                    }
                }
                else
                {
                    forward();
                }
            }
            else if (car)
            {
                MoveCreation.showControls = true;
            }

            PlacingTrack.SetActive(!showSearchingUI && !MoveCreation.showControls && !setCourse);

            // If the player has not touched the screen, we are done with this update.
            Touch touch;
            if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began || setCourse)
            {
                return;
            }

            // Raycast against the location the player touched to search for planes.
            TrackableHit hit;
            TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
                TrackableHitFlags.FeaturePointWithSurfaceNormal;


            if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
            {
                // Use hit pose and camera pose to check if hittest is from the
                // back of the plane, if it is, no need to create the anchor.
                if ((hit.Trackable is DetectedPlane) &&
                    Vector3.Dot(FirstPersonCamera.transform.position - hit.Pose.position,
                        hit.Pose.rotation * Vector3.up) < 0)
                {
                    Debug.Log("Hit at back of the current DetectedPlane");
                }
                else
                {
                    if(renderCar)
                    {
                        // Choose the Andy model for the Trackable that got hit.
                        GameObject prefab;
                        if (hit.Trackable is FeaturePoint)
                        {
                            prefab = AndyPointPrefab;
                        }
                        else
                        {
                            prefab = AndyPlanePrefab;
                        }

                        // Instantiate Andy model at the hit pose.

                        car = Instantiate(prefab, hit.Pose.position, hit.Pose.rotation);
                        start = hit.Pose.position;
                        startRotation = hit.Pose.rotation;

                        // Compensate for the hitPose rotation facing away from the raycast (i.e. camera).
                        car.transform.Rotate(0, k_ModelRotation, 0, Space.Self);

                        // Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
                        // world evolves.
                        var anchor = hit.Trackable.CreateAnchor(hit.Pose);

                        initHit = hit;
                        // Make Andy model a child of the anchor.
                        car.transform.parent = anchor.transform;

                        var rb = car.AddComponent<Rigidbody>();
                        BoxCollider bc = car.AddComponent<BoxCollider>();
                        bc.size = new Vector3(0.1f, 0.1f, 0.1f);
                        bc.isTrigger = true;
                        rb.useGravity = false;

                        orientation = 0;

                        PlacingTrack.SetActive(false);

                        //GameObject w = Wall;
                        //var obj = Instantiate(w, hit.Pose.position + new Vector3(0.0f, 0.0f, -.12f), hit.Pose.rotation);

                        int[,] path = DataScript.levels[DataScript.level];

                        for(int i = 0; i<path.GetLength(0); i++)
                        {
                            for (int j = 0; j < path.GetLength(1); j++)
                            {
                                if(path[i,j] == 1)
                                {
                                    var w = Wall;
                                    var obj = Instantiate(w, hit.Pose.position + new Vector3(0.3f - 0.1f * j, 0.0f,0.1f + -0.1f * i), hit.Pose.rotation);
                                }else if(path[i,j] == 2)
                                {
                                    var s = Star;
                                    var obj = Instantiate(s,hit.Pose.position + new Vector3(0.3f - 0.1f * j, 0.0f, 0.1f + -0.1f * i), hit.Pose.rotation);
                                    var rbStar = obj.AddComponent<Rigidbody>();
                                    rbStar.useGravity = false;
                                    rbStar.angularVelocity = new Vector3(0.0f, 0.5f, 0.0f);
                                    rbStar.angularDrag = 0;
                                    var boxCol = obj.AddComponent<BoxCollider>();
                                    boxCol.size = new Vector3(0.05f, 0.05f, 0.05f);
                                    boxCol.isTrigger = true;
                                }
                            }
                        }
                        setCourse = true;
                  

              
                    }
                }
            }
        }

        private void initForward()
        {
            move = true;
            initPos = car.transform.position;
            switch (orientation)
            {
                case 0:
                    dest = initPos + new Vector3(0.0f, 0.0f, -0.1f);
                    break;
                case 1:
                    dest = initPos + new Vector3(0.1f, 0.0f, 0.0f);
                    break;
                case 2:
                    dest = initPos + new Vector3(0.0f, 0.0f, 0.1f);
                    break;
                case 3:
                    dest = initPos + new Vector3(-0.1f, 0.0f, 0.0f);
                    break;
            }
        }


        private void forward()
        {
            if(car == null)
            {
                move = false;
                time = 0;
                return;
            }
            time += Time.deltaTime;
            var frac = (time / 2.0f);
            
            var rb = car.GetComponent<Rigidbody>();
            rb.position = Vector3.Lerp(initPos, dest, frac);
            if (frac >= 1.0f)
            {
                time = 0;
                move = false;
            }
        }

        private void rotate(bool right)
        {
            if(car == null)
            {
                move = false;
                time = 0;
            }
            var rb = car.GetComponent<Rigidbody>();
            if (!right)
            {
                rb.transform.Rotate(0, -90, 0);
                orientation = (orientation + 1) % 4;
            }
            else
            {
                rb.transform.Rotate(0, 90, 0);
                orientation--;
                if (orientation < 0)
                    orientation = 3;
            }
        }




        private void initData()
        {
            DataScript.initialized = true;
            //TODO: GET RID OF THIS!!!!!!
            //DataScript.moves = "FFFFFF";
            DataScript.level = 0;
            DataScript.levels = new List<int[,]>();
            DataScript.levels.Add(new int[,] { { 1, 1, 1, 1, 1, 1, 1 },
                                               { 1, 1, 1, 0, 1, 1, 1 },
                                               { 1, 1, 1, 0, 1, 1, 1 },
                                               { 1, 1, 1, 0, 1, 1, 1 },
                                               { 1, 1, 1, 0, 1, 1, 1 },
                                               { 1, 1, 1, 0, 1, 1, 1 },
                                               { 1, 1, 1, 2, 1, 1, 1 } });
            DataScript.levels.Add(new int[,] { { 1, 1, 1, 1, 1, 1, 1 },
                                               { 1, 0, 0, 0, 1, 1, 1 },
                                               { 1, 0, 1, 1, 1, 1, 1 },
                                               { 1, 0, 0, 0, 1, 1, 1 },
                                               { 1, 1, 1, 0, 1, 1, 1 },
                                               { 1, 1, 1, 0, 1, 1, 1 },
                                               { 1, 1, 1, 2, 1, 1, 1 } });
            DataScript.levels.Add(new int[,] { { 1, 1, 1, 1, 1, 1, 1 },
                                               { 1, 0, 0, 0, 1, 1, 1 },
                                               { 1, 0, 1, 1, 1, 1, 1 },
                                               { 1, 0, 0, 0, 0, 0, 1 },
                                               { 1, 1, 1, 1, 1, 0, 1 },
                                               { 1, 0, 0, 0, 0, 0, 1 },
                                               { 1, 0, 1, 1, 1, 1, 1 },
                                               { 1, 0, 0, 2, 1, 1, 1 }});
            DataScript.levels.Add(new int[,] { { 1, 1, 1, 1, 1, 1, 1 },
                                               { 1, 0, 0, 0, 1, 1, 1 },
                                               { 1, 0, 1, 1, 1, 1, 1 },
                                               { 1, 0, 0, 1, 1, 1, 1 },
                                               { 1, 1, 0, 0, 1, 1, 1 },
                                               { 1, 1, 1, 0, 0, 1, 1 },
                                               { 1, 0, 1, 1, 0, 0, 1 },
                                               { 1, 1, 1, 1, 1, 0, 1 },
                                               { 1, 2, 0, 0, 0, 0, 1 }});
            DataScript.totalLevels = 4;

        }

        /// <summary>
        /// Check and update the application lifecycle.
        /// </summary>
        private void _UpdateApplicationLifecycle()
        {
            // Exit the app when the 'back' button is pressed.
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }

            // Only allow the screen to sleep when not tracking.
            if (Session.Status != SessionStatus.Tracking)
            {
                const int lostTrackingSleepTimeout = 15;
                Screen.sleepTimeout = lostTrackingSleepTimeout;
            }
            else
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }

            if (m_IsQuitting)
            {
                return;
            }

            // Quit if ARCore was unable to connect and give Unity some time for the toast to appear.
            if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
            {
                _ShowAndroidToastMessage("Camera permission is needed to run this application.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }
            else if (Session.Status.IsError())
            {
                _ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }
        }

        /// <summary>
        /// Actually quit the application.
        /// </summary>
        private void _DoQuit()
        {
            Application.Quit();
        }

        /// <summary>
        /// Show an Android toast message.
        /// </summary>
        /// <param name="message">Message string to show in the toast.</param>
        private void _ShowAndroidToastMessage(string message)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if (unityActivity != null)
            {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
                        message, 0);
                    toastObject.Call("show");
                }));
            }
        }
    }
}

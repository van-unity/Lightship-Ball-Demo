// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System.Collections.Generic;
// using Auki.ConjureKit;
// using Auki.ConjureKit.Manna;
// using Auki.Util;
using Niantic.ARDK.AR;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.AR.HitTest;
using Niantic.ARDK.Extensions;
using Niantic.ARDK.External;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Input.Legacy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Basektball.Scripts {
    //! A helper class that demonstrates hit tests based on user input
    /// <summary>
    /// A sample class that can be added to a scene and takes user input in the form of a screen touch.
    ///   A hit test is run from that location. If a plane is found, spawn a game object at the
    ///   hit location.
    /// </summary>
    public class HitTester : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI sessionState;
        [SerializeField] private TextMeshProUGUI sessionID;
        public RawImage _rawImage;
        // private IConjureKit _conjureKit;
        // private Manna _manna;


        /// The camera used to render the scene. Used to get the center of the screen.
        public Camera Camera;

        /// The types of hit test results to filter against when performing a hit test.
        [EnumFlag] public ARHitTestResultType HitTestType = ARHitTestResultType.ExistingPlane;

        /// The object we will place when we get a valid hit test result!
        public GameObject PlacementObjectPf;

        /// A list of placed game objects to be destroyed in the OnDestroy method.
        private List<GameObject> _placedObjects = new List<GameObject>();

        /// Internal reference to the session, used to get the current frame to hit test against.
        private IARSession _session;

        
        
        
        private void Start() {
            ARSessionFactory.SessionInitialized += OnAnyARSessionDidInitialize;
            StartConjureKit();
        }

        private void StartConjureKit() {
            // _conjureKit = new ConjureKit(
            //     Camera.transform,
            //     "d69fb2b9-3e83-47c8-95a2-26a12796e2e1",
            //     "6764b692-e8d0-4a07-baff-c0804d4b4ece96254774-50a1-4818-b96b-0992cacb8a2");
            //
            // _manna = new Manna(_conjureKit);
            //
            // _conjureKit.OnStateChanged += state => { sessionState.text = state.ToString(); };
            //
            // _conjureKit.OnJoined += session => { sessionID.text = session.Id.ToString(); };
            //
            // _conjureKit.OnLeft += () => { sessionID.text = ""; };
            //
            // _manna.OnLighthouseTracked += OnLighthouseTracked;
            //
            //
            // _conjureKit.Connect();
        }

        private GameObject _basket;

        // private void OnLighthouseTracked(Lighthouse arg1, Pose arg2, bool arg3) {
        //     if (_basket == null) {
        //         _basket = Instantiate(PlacementObjectPf, arg2.position, arg2.rotation);
        //         _placedObjects.Add(_basket);
        //     }
        //     else {
        //         _basket.transform.position = arg2.position;
        //         _basket.transform.rotation = arg2.rotation;
        //     }
        // }


        private void OnAnyARSessionDidInitialize(AnyARSessionInitializedArgs args) {
            _session = args.Session;
            _session.Deinitialized += OnSessionDeinitialized;
        }

        private void OnSessionDeinitialized(ARSessionDeinitializedArgs args) {
            ClearObjects();
        }

        private void OnDestroy() {
            ARSessionFactory.SessionInitialized -= OnAnyARSessionDidInitialize;

            _session = null;

            ClearObjects();
        }

        private void ClearObjects() {
            foreach (var placedObject in _placedObjects) {
                Destroy(placedObject);
            }

            _placedObjects.Clear();
        }

        private void Update() {
            if (_session == null) {
                return;
            }

            FeedMannaWithVideoFrames();

            // if (PlatformAgnosticInput.touchCount <= 0) {
            //     return;
            // }
            //
            // var touch = PlatformAgnosticInput.GetTouch(0);
            // if (touch.phase == TouchPhase.Began) {
            //     TouchBegan(touch);
            // }
        }

        private void FeedMannaWithVideoFrames() {
            // var texture = FindObjectOfType<ARRenderingManager>().GPUTexture;
            // _rawImage.texture = texture;
            // _manna.ProcessVideoFrameTexture(
            //     texture,
            //     Camera.projectionMatrix,
            //     Camera.worldToCameraMatrix
            // );
        }

        private void TouchBegan(Touch touch) {
            var currentFrame = _session.CurrentFrame;
            if (currentFrame == null) {
                return;
            }

            if (touch.IsTouchOverUIObject())
                return;

            var results = currentFrame.HitTest
            (
                Camera.pixelWidth,
                Camera.pixelHeight,
                touch.position,
                HitTestType
            );

            int count = results.Count;
            Debug.Log("Hit test results: " + count);

            if (count <= 0)
                return;

            // Get the closest result
            var result = results[0];

            var hitPosition = result.WorldTransform.ToPosition();

            _placedObjects.Add(Instantiate(PlacementObjectPf, hitPosition, Quaternion.identity));

            var anchor = result.Anchor;
            Debug.LogFormat
            (
                "Spawning cube at {0} (anchor: {1})",
                hitPosition.ToString("F4"),
                anchor == null
                    ? "none"
                    : anchor.AnchorType + " " + anchor.Identifier
            );
        }
    }
}
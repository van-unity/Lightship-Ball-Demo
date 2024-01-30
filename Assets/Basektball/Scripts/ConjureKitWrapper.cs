using System;
using System.Collections.Generic;
using Auki.ConjureKit;
using Auki.ConjureKit.Manna;
using Auki.ConjureKit.Vikja;
using Auki.Util;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;


namespace Basektball.Scripts {
    public class ConjureKitWrapper : MonoBehaviour {
        private const string APP_KEY = "34cd9dd5-b3d9-469a-8492-5d0b3da15fcd";
        private const string APP_SECRET = "1a476734-6810-4213-aea1-0d9232a6b61c17565376-f48e-4267-a0e9-c9e10a6fb495";

        [SerializeField] private RawImage rawImage;
        [SerializeField] private TextMeshProUGUI statsText;
        
        [SerializeField] private Camera arCamera;
        // [SerializeField] private ARHitTester arHitTester;
        [SerializeField] private GameObject basketballGamePrefab;
        [SerializeField] private TextMeshProUGUI _sessionText;
        [SerializeField] private RawImage _qrImage;
        private ConjureKit _conjureKit;
        private Manna _manna;
        private Vikja _vikja;

        void Start() {
            _conjureKit = new ConjureKit(arCamera.transform, APP_KEY, APP_SECRET);
            _manna = new Manna(_conjureKit);
            _manna.SetLighthouseVisible(true);
            _conjureKit.OnJoined += OnJoinedSession;

            _manna.GetOrCreateFrameFeederComponent().AttachMannaInstance(_manna);

            // arHitTester.OnObjectPlaced += OnBasketballGameObjectPlaced;
            // arHitTester.enabled = false;

            _vikja = new Vikja(_conjureKit);
            _vikja.OnEntityAction += action => {
                switch (action.Name) {
                    case BALL_THROW_VIKJA_ACTION:
                        PlayThrowAnimation(action);
                        break;
                    default:
                        Debug.Log($"Unknown Vikja action {action.Name}");
                        break;
                }
            };

            AukiDebug.logLevel = AukiDebug.LogLevel.TRACE;
            
            _conjureKit.Connect();
        }

        private void Update() {
            // FeedMannaWithVideoFrames();
        }

        // private void FeedMannaWithVideoFrames() {
        //     if(_conjureKit.GetState() != State.Calibrated)
        //         return;
        //     
        //     var texture = FindObjectOfType<ARRenderingManager>().GPUTexture;
        //     var videoTexture = new RenderTexture(texture.width, texture.height, 0, GraphicsFormat.R8G8B8A8_UNorm);
        //     _qrImage.texture = videoTexture;
        //     _manna.ProcessVideoFrameTexture(
        //         videoTexture,
        //         arCamera.projectionMatrix,
        //         arCamera.worldToCameraMatrix
        //     );
        //
        //     videoTexture = texture;
        //     rawImage.texture = videoTexture;
        //     statsText.text = $"Stats:\n" +
        //                      $"Width: {videoTexture.width}\n" +
        //                      $"Height: {videoTexture.height}\n" +
        //                      $"depth: {videoTexture.depth}\n" +
        //                      $"Format: {videoTexture.format}\n" +
        //                      $"EnableRandomWrite: {videoTexture.enableRandomWrite}\n" +
        //                      $"sRGB: {videoTexture.sRGB}\n" +
        //                      $"UseDynamicScale: {videoTexture.useDynamicScale}\n";
        // }

        private void OnBasketballGameObjectPlaced(GameObject basketballGameObject) {
            _conjureKit.GetSession().AddEntity(
                new Pose(basketballGameObject.transform.position, basketballGameObject.transform.rotation),
                entity => Debug.Log("Entity placed"),
                Debug.LogError);
        }

        private void OnJoinedSession(Session session) {
            _sessionText.text = session.Id;
            foreach (var entity in session.GetEntities()) {
                if (entity.Flag != EntityFlag.EntityFlagParticipantEntity) {
                    PlaceExistingBasketballGameEntity(entity);
                    return;
                }
            }

            // arHitTester.enabled = true;
        }

        private void PlaceExistingBasketballGameEntity(Entity entity) {
            var pose = _conjureKit.GetSession().GetEntityPose(entity);
            Instantiate(basketballGamePrefab, pose.position, pose.rotation);

            GameObject.FindGameObjectWithTag("hoop-hit").SetActive(false);
        }

        public void Throw(Pose pose, Vector3 direction) {
            _conjureKit.GetSession()?.AddEntity(pose, false, entity => {
                var action = new BallThrowAction {
                    Direction = direction,
                    Position = pose.position
                };

                _vikja.RequestAction(entity.Id, BALL_THROW_VIKJA_ACTION, action.ToByteArray(),
                    PlayThrowAnimation,
                    s => AukiDebug.LogInfo($"Received request action error: {s}"));
            }, s => AukiDebug.LogDebug($"Received entity add error error: {s}"));
        }

        private void PlayThrowAnimation(EntityAction entityAction) {
            var throwAction = BallThrowAction.FromByteArray(entityAction.Data);

            FindObjectOfType<GameController>().ApplyForce(throwAction.Direction, throwAction.Position);
        }

        private const string BALL_THROW_VIKJA_ACTION = "BALL_THROW_VIKJA_ACTION";

        public struct BallThrowAction {
            public Vector3 Direction;
            public Vector3 Position;

            public byte[] ToByteArray() {
                // 3 floats per each vector
                int byteSize = sizeof(float) * 6;
                List<byte> bytes = new List<byte>(byteSize);
                bytes.AddRange(BitConverter.GetBytes(Direction.x));
                bytes.AddRange(BitConverter.GetBytes(Direction.y));
                bytes.AddRange(BitConverter.GetBytes(Direction.z));

                bytes.AddRange(BitConverter.GetBytes(Position.x));
                bytes.AddRange(BitConverter.GetBytes(Position.y));
                bytes.AddRange(BitConverter.GetBytes(Position.z));

                return bytes.ToArray();
            }

            public static BallThrowAction FromByteArray(byte[] bytes) {
                var action = new BallThrowAction {
                    Direction = new Vector3(
                        BitConverter.ToSingle(bytes, sizeof(float) * 0),
                        BitConverter.ToSingle(bytes, sizeof(float) * 1),
                        BitConverter.ToSingle(bytes, sizeof(float) * 2)),

                    Position = new Vector3(
                        BitConverter.ToSingle(bytes, sizeof(float) * 3),
                        BitConverter.ToSingle(bytes, sizeof(float) * 4),
                        BitConverter.ToSingle(bytes, sizeof(float) * 5))
                };

                return action;
            }
        }
    }
}
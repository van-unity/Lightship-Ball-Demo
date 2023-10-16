using System;
using Auki.ConjureKit;
using Auki.ConjureKit.Manna;
using Niantic.ARDK.Extensions;
using Niantic.ARDKExamples.Helpers;
using UnityEngine;

public class ConjureKitWrapper : MonoBehaviour {
    private const string APP_KEY = "34cd9dd5-b3d9-469a-8492-5d0b3da15fcd";
    private const string APP_SECRET = "1a476734-6810-4213-aea1-0d9232a6b61c17565376-f48e-4267-a0e9-c9e10a6fb495";
    
    [SerializeField] private Camera arCamera;
    [SerializeField] private ARHitTester arHitTester;
    [SerializeField] private GameObject basketballGamePrefab;
    
    private ConjureKit _conjureKit;
    private Manna _manna;
    
    void Start() {
        _conjureKit = new ConjureKit(arCamera.transform, APP_KEY, APP_SECRET);
        _manna = new Manna(_conjureKit);

        _conjureKit.OnJoined += OnJoinedSession;

        arHitTester.OnObjectPlaced += OnBasketballGameObjectPlaced;
        arHitTester.enabled = false;
        
        _conjureKit.Connect();
    }

    private void Update() {
        FeedMannaWithVideoFrames();
    }

    private void FeedMannaWithVideoFrames() {
        var texture = FindObjectOfType<ARRenderingManager>().GPUTexture;
        _manna.ProcessVideoFrameTexture(
            texture,
            arCamera.projectionMatrix,
            arCamera.worldToCameraMatrix
        );
    }

    private void OnBasketballGameObjectPlaced(GameObject basketballGameObject) {
        _conjureKit.GetSession().AddEntity(
            new Pose(basketballGameObject.transform.position, basketballGameObject.transform.rotation), 
            entity => Debug.Log("Entity placed"),
            Debug.LogError);
    }

    private void OnJoinedSession(Session session) {
        foreach (var entity in session.GetEntities()) {
            if (entity.Flag != EntityFlag.EntityFlagParticipantEntity) {
                PlaceExistingBasketballGameEntity(entity);
                return;
            }
        }
        
        arHitTester.enabled = false;
    }

    private void PlaceExistingBasketballGameEntity(Entity entity) {
        var pose = _conjureKit.GetSession().GetEntityPose(entity);
        Instantiate(basketballGamePrefab, pose.position, pose.rotation);
    }
}

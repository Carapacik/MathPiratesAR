using MathPirates.Infrastructure;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;

namespace MathPirates.AR
{
    public sealed class SurfacePlacement : MonoBehaviour
    {
        [SerializeField] private GameObject rig;
        [SerializeField] private ARSession session;
        [SerializeField] private ARRaycastManager raycasts;
        [SerializeField] private ARPlaneManager planes;
        [SerializeField] private ARAnchorManager anchors;
        [SerializeField] private Camera arCamera;
        private readonly List<ARRaycastHit> hits = new();
        private bool requested;
        private bool initializing;
        private bool initialized;
        private bool placingAnchor;
        private int generation;
        private ARAnchor anchor;
        private Transform content;
        private Pose pose;
        private string placementError;

        public Camera Camera => arCamera;
        public bool IsPlaced { get; private set; }
        public bool CanPlace { get; private set; }
        public bool Tracking => requested && ARSession.state == ARSessionState.SessionTracking && (!IsPlaced || anchor != null && anchor.trackingState == TrackingState.Tracking);
        public string Status { get; private set; } = "";

        public void Configure(GameObject xrRig, ARSession arSession, ARRaycastManager rays, ARPlaneManager planeManager, ARAnchorManager anchorManager, Camera camera)
        {
            rig = xrRig; session = arSession; raycasts = rays; planes = planeManager; anchors = anchorManager; arCamera = camera;
        }

        public void Begin(Transform board)
        {
            content = board;
            requested = true;
            IsPlaced = false;
            CanPlace = false;
            placementError = null;
            ARPresentation.ConfigureCamera(arCamera);
            content.gameObject.SetActive(false);
            Status = Locale.Text("ar-connecting");
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!initializing) StartCoroutine(Initialize());
#else
            Status = Locale.Text("ar-platform");
#endif
        }

        private IEnumerator Initialize()
        {
            initializing = true;
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera))
            {
                UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Camera);
                float deadline = Time.realtimeSinceStartup + 30;
                while (requested && !UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera) && Time.realtimeSinceStartup < deadline)
                    yield return null;
                if (!requested || !UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera))
                {
                    Status = Locale.Text("ar-permission");
                    initializing = false;
                    yield break;
                }
            }
            var manager = XRGeneralSettings.Instance?.Manager;
            if (manager == null)
            {
                Status = Locale.Text("ar-config");
                initializing = false;
                yield break;
            }
            if (!initialized)
            {
                yield return manager.InitializeLoader();
                initialized = manager.activeLoader != null;
            }
            if (!requested) { ShutdownLoader(); initializing = false; yield break; }
            if (!initialized)
            {
                Status = Locale.Text("ar-start");
                initializing = false;
                yield break;
            }
            manager.StartSubsystems();
            rig.SetActive(true);
            session.gameObject.SetActive(true);
            yield return ARSession.CheckAvailability();
            if (requested && ARSession.state == ARSessionState.NeedsInstall)
                yield return ARSession.Install();
            initializing = false;
            if (!requested) { End(); yield break; }
            Status = ARSession.state == ARSessionState.Unsupported || ARSession.state == ARSessionState.NeedsInstall
                ? Locale.Text("ar-unavailable")
                : Locale.Text("ar-scan");
        }

        private void Update()
        {
            if (!requested || initializing || !initialized) return;
            if (IsPlaced)
            {
                // Keep the last anchored pose visible while tracking recovers. Input remains blocked.
                content.gameObject.SetActive(anchor != null);
                Status = Tracking ? Locale.Text("ar-placed") : Locale.Text("ar-lost");
                return;
            }
            CanPlace = !placingAnchor && Tracking && raycasts.Raycast(new Vector2(Screen.width * .5f, Screen.height * .5f), hits, TrackableType.PlaneWithinPolygon);
            if (CanPlace)
            {
                pose = hits[0].pose;
                ARPresentation.PlaceBoard(content, null, pose, arCamera.transform.position);
                Status = placementError ?? Locale.Text("ar-found");
            }
            else
            {
                if (!placingAnchor) content.gameObject.SetActive(false);
                if (Tracking) Status = placingAnchor ? Locale.Text("ar-anchoring") : Locale.Text("ar-search");
                else Status = Locale.Text("ar-lost");
            }
        }

        public async void Place()
        {
            if (!CanPlace || placingAnchor) return;
            placingAnchor = true;
            CanPlace = false;
            placementError = null;
            Status = Locale.Text("ar-anchoring");
            int requestGeneration = generation;
            try
            {
                var result = await anchors.TryAddAnchorAsync(pose);
                if (!requested || requestGeneration != generation)
                {
                    if (result.status.IsSuccess()) anchors.TryRemoveAnchor(result.value);
                    return;
                }
                if (!result.status.IsSuccess()) { placementError = Status = Locale.Text("ar-anchor-failed"); return; }
                anchor = result.value;
                ARPresentation.PlaceBoard(content, anchor.transform, new Pose(anchor.transform.position, anchor.transform.rotation), arCamera.transform.position);
                IsPlaced = true;
                Status = Locale.Text("ar-placed");
                Debug.Log($"AR board placed at {content.position}, scale {content.lossyScale}, anchor tracking {anchor.trackingState}.");
                planes.enabled = false;
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);
                if (requested && requestGeneration == generation) placementError = Status = Locale.Text("ar-place-failed");
            }
            finally { placingAnchor = false; }
        }

        public void Reposition()
        {
            generation++;
            placementError = null;
            if (content != null) { content.SetParent(null, true); content.gameObject.SetActive(false); }
            if (anchor != null) anchors.TryRemoveAnchor(anchor);
            anchor = null;
            IsPlaced = false;
            CanPlace = false;
            planes.enabled = true;
        }

        public void End()
        {
            requested = false;
            Reposition();
            rig.SetActive(false);
            session.gameObject.SetActive(false);
            if (!initializing) ShutdownLoader();
        }

        private void ShutdownLoader()
        {
            if (!initialized) return;
            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
            initialized = false;
        }

        private void OnDestroy() { if (initialized) ShutdownLoader(); }
    }
}


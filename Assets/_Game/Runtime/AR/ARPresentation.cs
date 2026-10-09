using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace MathPirates.AR
{
    public static class ARPresentation
    {
        public const float BoardScale = .14f;

        public static void ConfigureCamera(Camera camera)
        {
            ConfigureTracking(camera);
            camera.nearClipPlane = .01f;
            camera.farClipPlane = 30;
            var data = camera.GetUniversalAdditionalCameraData();
            data.renderType = CameraRenderType.Base;
            data.SetRenderer(0);
            data.renderPostProcessing = false;
            data.allowXRRendering = true;
            // Draw the live camera first, then opaque models and transparent water.
            camera.GetComponent<ARCameraManager>().requestedBackgroundRenderingMode = CameraBackgroundRenderingMode.BeforeOpaques;
        }

        private static void ConfigureTracking(Camera camera)
        {
            if (!camera.TryGetComponent<TrackedPoseDriver>(out var driver))
                driver = camera.gameObject.AddComponent<TrackedPoseDriver>();
            if (HasHandheldBinding(driver.positionInput.action, "devicePosition") &&
                HasHandheldBinding(driver.rotationInput.action, "deviceRotation") &&
                (driver.trackingStateInput.action == null || driver.trackingStateInput.action.bindings.Count == 0)) return;

            var position = new InputAction("Position", InputActionType.Value,
                "<HandheldARInputDevice>/devicePosition", expectedControlType: "Vector3");
            var rotation = new InputAction("Rotation", InputActionType.Value,
                "<HandheldARInputDevice>/deviceRotation", expectedControlType: "Quaternion");
            position.AddBinding("<XRHMD>/centerEyePosition");
            rotation.AddBinding("<XRHMD>/centerEyeRotation");
            var previousPosition = driver.positionInput.action;
            var previousRotation = driver.rotationInput.action;
            var previousTracking = driver.trackingStateInput.action;
            driver.positionInput = new InputActionProperty(position);
            driver.rotationInput = new InputActionProperty(rotation);
            // Handheld AR exposes pose controls but no trackingState control. Let the driver
            // infer pose availability; SurfacePlacement gates interaction on AR session tracking.
            driver.trackingStateInput = default;
            previousPosition?.Dispose();
            previousRotation?.Dispose();
            previousTracking?.Dispose();
        }

        private static bool HasHandheldBinding(InputAction action, string control)
        {
            if (action == null || action.type != InputActionType.Value) return false;
            foreach (var binding in action.bindings)
                if (binding.path == "<HandheldARInputDevice>/" + control) return true;
            return false;
        }

        public static void PlaceBoard(Transform board, Transform parent, Pose surface, Vector3 cameraPosition)
        {
            board.SetParent(parent, false);
            Vector3 awayFromPlayer = surface.position - cameraPosition;
            awayFromPlayer.y = 0;
            if (awayFromPlayer.sqrMagnitude < .0001f) awayFromPlayer = Vector3.forward;
            board.SetPositionAndRotation(surface.position + Vector3.up * .02f,
                Quaternion.LookRotation(awayFromPlayer, Vector3.up));
            board.localScale = Vector3.one * BoardScale;
            board.gameObject.SetActive(true);
        }
    }
}

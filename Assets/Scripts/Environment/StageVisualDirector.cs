using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AreaX.Environment
{
    public class StageVisualDirector : MonoBehaviour
    {
        [SerializeField] private Color _spaceColor = new Color(0.002f, 0.004f, 0.012f, 1f);
        [SerializeField] private Color _fogColor = new Color(0.01f, 0.025f, 0.045f, 1f);
        [SerializeField] private float _fogDensity = 0.012f;
        [SerializeField] private float _cameraFarClip = 650f;

        public static StageVisualDirector CreateDefault()
        {
            GameObject directorObject = new GameObject("StageVisualDirector");
            return directorObject.AddComponent<StageVisualDirector>();
        }

        private void Awake()
        {
            ApplyCameraLook();
            ApplyRenderSettings();
            HidePrototypeGeometry();
            EnsurePostProcessing();
        }

        private void ApplyCameraLook()
        {
            Camera camera = Camera.main;
            if (camera == null) return;

            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = _spaceColor;
            camera.farClipPlane = _cameraFarClip;

            UniversalAdditionalCameraData urpCamera = camera.GetComponent<UniversalAdditionalCameraData>();
            if (urpCamera != null)
            {
                urpCamera.renderPostProcessing = true;
            }
        }

        private void ApplyRenderSettings()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = _fogColor;
            RenderSettings.fogDensity = _fogDensity;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.015f, 0.02f, 0.035f, 1f);
        }

        private void HidePrototypeGeometry()
        {
            GameObject plane = GameObject.Find("Plane");
            if (plane != null)
            {
                plane.SetActive(false);
            }
        }

        private void EnsurePostProcessing()
        {
            Volume volume = FindFirstObjectByType<Volume>();
            if (volume == null)
            {
                GameObject volumeObject = new GameObject("Runtime Global Volume");
                volume = volumeObject.AddComponent<Volume>();
                volume.isGlobal = true;
                volume.priority = 10f;
            }

            if (volume.profile == null)
            {
                volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();
            }

            if (!volume.profile.TryGet(out Bloom bloom))
            {
                bloom = volume.profile.Add<Bloom>(true);
            }

            bloom.active = true;
            bloom.threshold.Override(0.25f);
            bloom.intensity.Override(1.8f);
            bloom.scatter.Override(0.65f);

            if (!volume.profile.TryGet(out Vignette vignette))
            {
                vignette = volume.profile.Add<Vignette>(true);
            }

            vignette.active = true;
            vignette.intensity.Override(0.32f);
            vignette.smoothness.Override(0.55f);
            vignette.color.Override(Color.black);
        }
    }
}

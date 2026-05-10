using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using AreaX.Player;
using AreaX.Targets;

namespace AreaX.UI
{
    public class CrosshairUI : MonoBehaviour
    {
        [SerializeField] private Image _crosshairImage;
        [SerializeField] private LockOnSystem _lockOnSystem;
        [SerializeField] private GameObject _lockMarkerPrefab; // Prefab for visual lock
        [SerializeField] private Transform _markerContainer;

        private List<GameObject> _activeMarkers = new List<GameObject>();
        private Camera _mainCamera;

        private void Start()
        {
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            UpdateLockMarkers();
        }

        private void UpdateLockMarkers()
        {
            if (_lockOnSystem == null || _markerContainer == null || _lockMarkerPrefab == null) return;

            var lockedTargets = _lockOnSystem.LockedTargets;

            // Adjust active markers count
            while (_activeMarkers.Count < lockedTargets.Count)
            {
                GameObject marker = Instantiate(_lockMarkerPrefab, _markerContainer);
                _activeMarkers.Add(marker);
            }
            while (_activeMarkers.Count > lockedTargets.Count)
            {
                GameObject marker = _activeMarkers[_activeMarkers.Count - 1];
                _activeMarkers.RemoveAt(_activeMarkers.Count - 1);
                Destroy(marker);
            }

            // Position markers
            for (int i = 0; i < lockedTargets.Count; i++)
            {
                Target t = lockedTargets[i];
                GameObject m = _activeMarkers[i];

                if (t != null)
                {
                    Vector3 screenPos = _mainCamera.WorldToScreenPoint(t.transform.position);
                    // Check if in front of camera
                    if (screenPos.z > 0)
                    {
                        m.transform.position = screenPos;
                        m.gameObject.SetActive(true);
                    }
                    else
                    {
                        m.gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}

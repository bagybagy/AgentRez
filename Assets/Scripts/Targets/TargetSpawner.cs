using UnityEngine;
using System.Collections.Generic;

namespace AreaX.Targets
{
    public class TargetSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _targetPrefab;
        [SerializeField] private int _count = 20;
        [SerializeField] private Vector3 _spawnAreaSize = new Vector3(50, 20, 100);
        [SerializeField] private float _startZ = 20f;

        private List<GameObject> _spawnedTargets = new List<GameObject>();

        private void Start()
        {
            SpawnTargets();
        }

        [ContextMenu("Spawn Targets")]
        public void SpawnTargets()
        {
            // Clear existing if calling from Editor (Validation)
            foreach(var t in _spawnedTargets)
            {
                if(t != null) Destroy(t);
            }
            _spawnedTargets.Clear();

            if (_targetPrefab == null)
            {
                Debug.LogWarning("Target Prefab is missing.");
                return;
            }

            for (int i = 0; i < _count; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-_spawnAreaSize.x / 2, _spawnAreaSize.x / 2),
                    Random.Range(-_spawnAreaSize.y / 2, _spawnAreaSize.y / 2),
                    _startZ + Random.Range(0, _spawnAreaSize.z)
                );

                GameObject instance = Instantiate(_targetPrefab, pos, Quaternion.identity, transform);
                _spawnedTargets.Add(instance);
            }
        }
    }
}

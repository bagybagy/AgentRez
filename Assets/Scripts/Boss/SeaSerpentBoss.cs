using System.Collections.Generic;
using AreaX.Targets;
using UnityEngine;

namespace AreaX.Boss
{
    public class SeaSerpentBoss : MonoBehaviour
    {
        private const string SegmentName = "SerpentSegment";
        private const string LockPointName = "SerpentLockPoint";

        [Header("Body")]
        [SerializeField, Min(4)] private int _segmentCount = 18;
        [SerializeField, Min(1)] private int _lockPointsPerSegment = 2;
        [SerializeField] private float _segmentSpacing = 3.2f;
        [SerializeField] private float _segmentRadius = 1.2f;
        [SerializeField] private float _bodyWaveAmplitude = 5f;
        [SerializeField] private float _bodyWaveFrequency = 0.55f;
        [SerializeField] private float _bodyWaveSpeed = 0.8f;

        [Header("Stage Placement")]
        [SerializeField] private Vector3 _origin = new Vector3(0f, 1.5f, 38f);
        [SerializeField] private float _lockPointRadius = 0.32f;

        [Header("Materials")]
        [SerializeField] private Color _bodyColor = new Color(0.08f, 0.24f, 0.42f, 1f);
        [SerializeField] private Color _lockPointColor = new Color(0.2f, 0.95f, 1f, 1f);
        [SerializeField] private Color _lockedColor = new Color(1f, 0.2f, 0.12f, 1f);

        private readonly List<Transform> _segments = new List<Transform>();
        private readonly List<Target> _lockPoints = new List<Target>();
        private Material _bodyMaterial;
        private Material _lockPointMaterial;
        private int _remainingLockPoints;

        public IReadOnlyList<Target> LockPoints => _lockPoints;
        public bool IsDefeated => _remainingLockPoints <= 0 && _lockPoints.Count > 0;

        public static SeaSerpentBoss CreateDefault()
        {
            GameObject bossObject = new GameObject("SeaSerpentBoss");
            SeaSerpentBoss boss = bossObject.AddComponent<SeaSerpentBoss>();
            boss.Build();
            return boss;
        }

        private void Start()
        {
            if (_segments.Count == 0)
            {
                Build();
            }
        }

        private void Update()
        {
            AnimateBody();
        }

        [ContextMenu("Rebuild Boss")]
        public void Build()
        {
            ClearChildren();
            CreateMaterials();

            _segments.Clear();
            _lockPoints.Clear();

            for (int i = 0; i < _segmentCount; i++)
            {
                Transform segment = CreateSegment(i);
                _segments.Add(segment);
                CreateLockPoints(segment, i);
            }

            _remainingLockPoints = _lockPoints.Count;
            AnimateBody();
        }

        private Transform CreateSegment(int index)
        {
            GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            segment.name = $"{SegmentName}_{index:00}";
            segment.transform.SetParent(transform, false);
            segment.transform.localScale = Vector3.one * _segmentRadius;

            Renderer renderer = segment.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = _bodyMaterial;
            }

            Collider collider = segment.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            return segment.transform;
        }

        private void CreateLockPoints(Transform segment, int segmentIndex)
        {
            for (int i = 0; i < _lockPointsPerSegment; i++)
            {
                float side = i % 2 == 0 ? -1f : 1f;
                Vector3 localOffset = new Vector3(side * _segmentRadius * 0.82f, 0.15f, 0f);

                GameObject lockPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                lockPoint.name = $"{LockPointName}_{segmentIndex:00}_{i:00}";
                lockPoint.transform.SetParent(segment, false);
                lockPoint.transform.localPosition = localOffset;
                lockPoint.transform.localScale = Vector3.one * _lockPointRadius;

                Renderer renderer = lockPoint.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = _lockPointMaterial;
                }

                Target target = lockPoint.AddComponent<Target>();
                target.Locked += HandleLockPointLocked;
                target.Hit += HandleLockPointHit;
                _lockPoints.Add(target);
            }
        }

        private void AnimateBody()
        {
            if (_segments.Count == 0) return;

            float time = Time.time * _bodyWaveSpeed;
            for (int i = 0; i < _segments.Count; i++)
            {
                float t = _segments.Count <= 1 ? 0f : i / (float)(_segments.Count - 1);
                float z = (t - 0.5f) * _segmentSpacing * (_segments.Count - 1);
                float wave = Mathf.Sin(time + i * _bodyWaveFrequency);
                float verticalWave = Mathf.Cos(time * 0.7f + i * _bodyWaveFrequency) * 1.4f;

                Vector3 position = _origin + new Vector3(
                    wave * _bodyWaveAmplitude,
                    verticalWave,
                    z
                );

                Transform segment = _segments[i];
                segment.position = position;

                if (i > 0)
                {
                    Vector3 previous = _segments[i - 1].position;
                    Vector3 direction = position - previous;
                    if (direction.sqrMagnitude > 0.01f)
                    {
                        segment.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
                    }
                }
            }
        }

        private void HandleLockPointLocked(Target target)
        {
            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = _lockedColor;
            }
        }

        private void HandleLockPointHit(Target target)
        {
            _remainingLockPoints = Mathf.Max(0, _remainingLockPoints - 1);
        }

        private void CreateMaterials()
        {
            _bodyMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            _bodyMaterial.color = _bodyColor;

            _lockPointMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            _lockPointMaterial.color = _lockPointColor;
            _lockPointMaterial.EnableKeyword("_EMISSION");
            _lockPointMaterial.SetColor("_EmissionColor", _lockPointColor * 1.8f);
        }

        private void ClearChildren()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }
    }
}

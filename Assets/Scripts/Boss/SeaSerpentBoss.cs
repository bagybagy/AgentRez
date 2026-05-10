using System.Collections.Generic;
using AreaX.Targets;
using AreaX.Managers;
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
        [SerializeField, Min(1)] private int _phaseCount = 3;
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
        [SerializeField] private Color _inactiveColor = new Color(0.02f, 0.08f, 0.12f, 1f);
        [SerializeField] private Color _eyeColor = new Color(1f, 0.08f, 0.02f, 1f);

        [Header("Reaction VFX")]
        [SerializeField] private int _hitBurstCount = 28;
        [SerializeField] private int _phasePulseCount = 120;
        [SerializeField] private float _particleLife = 0.8f;

        private readonly List<Transform> _segments = new List<Transform>();
        private readonly List<Transform> _links = new List<Transform>();
        private readonly List<Target> _lockPoints = new List<Target>();
        private readonly Dictionary<Target, int> _lockPointPhases = new Dictionary<Target, int>();
        private Material _bodyMaterial;
        private Material _lockPointMaterial;
        private Material _inactiveLockPointMaterial;
        private Material _eyeMaterial;
        private int _remainingLockPoints;
        private int _currentPhase;

        public IReadOnlyList<Target> LockPoints => _lockPoints;
        public bool IsDefeated => _remainingLockPoints <= 0 && _lockPoints.Count > 0;
        public int CurrentPhase => _currentPhase + 1;
        public int PhaseCount => _phaseCount;
        public int RemainingLockPoints => _remainingLockPoints;

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
            _links.Clear();
            _lockPoints.Clear();
            _lockPointPhases.Clear();

            for (int i = 0; i < _segmentCount; i++)
            {
                Transform segment = CreateSegment(i);
                _segments.Add(segment);
                CreateLockPoints(segment, i);
            }

            for (int i = 0; i < _segments.Count - 1; i++)
            {
                _links.Add(CreateLink(i));
            }

            _remainingLockPoints = _lockPoints.Count;
            _currentPhase = 0;
            ActivatePhase(_currentPhase);
            AnimateBody();
        }

        private Transform CreateSegment(int index)
        {
            GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bool isHead = index == _segmentCount - 1;
            segment.name = isHead ? "SerpentHead" : $"{SegmentName}_{index:00}";
            segment.transform.SetParent(transform, false);
            segment.transform.localScale = Vector3.one * _segmentRadius * (isHead ? 1.55f : 1f);

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

            if (isHead)
            {
                CreateEyes(segment.transform);
            }

            return segment.transform;
        }

        private void CreateEyes(Transform head)
        {
            CreateEye(head, new Vector3(-0.34f, 0.28f, 0.78f));
            CreateEye(head, new Vector3(0.34f, 0.28f, 0.78f));
        }

        private void CreateEye(Transform head, Vector3 localPosition)
        {
            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = "SerpentEye";
            eye.transform.SetParent(head, false);
            eye.transform.localPosition = localPosition;
            eye.transform.localScale = Vector3.one * 0.16f;

            Renderer renderer = eye.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = _eyeMaterial;
            }

            Collider collider = eye.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
        }

        private Transform CreateLink(int index)
        {
            GameObject link = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            link.name = $"SerpentBodyLink_{index:00}";
            link.transform.SetParent(transform, false);

            Renderer renderer = link.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = _bodyMaterial;
            }

            Collider collider = link.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            return link.transform;
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
                _lockPointPhases[target] = GetPhaseForSegment(segmentIndex);
            }
        }

        private int GetPhaseForSegment(int segmentIndex)
        {
            float normalized = _segmentCount <= 1 ? 0f : segmentIndex / (float)(_segmentCount - 1);
            return Mathf.Clamp(Mathf.FloorToInt(normalized * _phaseCount), 0, _phaseCount - 1);
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

            UpdateBodyLinks();
        }

        private void UpdateBodyLinks()
        {
            for (int i = 0; i < _links.Count; i++)
            {
                Transform link = _links[i];
                Vector3 start = _segments[i].position;
                Vector3 end = _segments[i + 1].position;
                Vector3 delta = end - start;
                float length = delta.magnitude;

                link.position = (start + end) * 0.5f;
                if (length > 0.01f)
                {
                    link.rotation = Quaternion.FromToRotation(Vector3.up, delta.normalized);
                }

                link.localScale = new Vector3(_segmentRadius * 0.68f, length * 0.5f, _segmentRadius * 0.68f);
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
            SpawnParticleBurst(target.transform.position, _lockPointColor, _hitBurstCount, 0.08f, 5f);

            if (!HasActiveLockPointInPhase(_currentPhase))
            {
                AdvancePhase();
            }
        }

        private void ActivatePhase(int phase)
        {
            for (int i = 0; i < _lockPoints.Count; i++)
            {
                Target lockPoint = _lockPoints[i];
                bool isActivePhase = _lockPointPhases.TryGetValue(lockPoint, out int pointPhase) && pointPhase == phase;
                bool lockable = isActivePhase && lockPoint.State != TargetState.Processed;
                lockPoint.SetLockable(lockable);

                Renderer renderer = lockPoint.GetComponent<Renderer>();
                if (renderer != null && lockPoint.State != TargetState.Processed)
                {
                    renderer.enabled = true;
                    renderer.sharedMaterial = lockable ? _lockPointMaterial : _inactiveLockPointMaterial;
                }
            }
        }

        private bool HasActiveLockPointInPhase(int phase)
        {
            for (int i = 0; i < _lockPoints.Count; i++)
            {
                Target lockPoint = _lockPoints[i];
                if (_lockPointPhases.TryGetValue(lockPoint, out int pointPhase) &&
                    pointPhase == phase &&
                    lockPoint.State != TargetState.Processed)
                {
                    return true;
                }
            }

            return false;
        }

        private void AdvancePhase()
        {
            _currentPhase++;
            if (_currentPhase >= _phaseCount)
            {
                return;
            }

            ActivatePhase(_currentPhase);
            SpawnParticleBurst(transform.position + _origin, _lockedColor, _phasePulseCount, 0.16f, 11f);

            if (BeatManager.Instance != null)
            {
                float pulse = 1f + _currentPhase * 0.15f;
                _bodyMaterial.SetColor("_EmissionColor", _bodyColor * pulse);
            }
        }

        private void CreateMaterials()
        {
            _bodyMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            _bodyMaterial.color = _bodyColor;
            _bodyMaterial.EnableKeyword("_EMISSION");
            _bodyMaterial.SetColor("_EmissionColor", _bodyColor * 0.4f);

            _lockPointMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            _lockPointMaterial.color = _lockPointColor;
            _lockPointMaterial.EnableKeyword("_EMISSION");
            _lockPointMaterial.SetColor("_EmissionColor", _lockPointColor * 1.8f);

            _inactiveLockPointMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            _inactiveLockPointMaterial.color = _inactiveColor;
            _inactiveLockPointMaterial.EnableKeyword("_EMISSION");
            _inactiveLockPointMaterial.SetColor("_EmissionColor", _inactiveColor * 0.5f);

            _eyeMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            _eyeMaterial.color = _eyeColor;
            _eyeMaterial.EnableKeyword("_EMISSION");
            _eyeMaterial.SetColor("_EmissionColor", _eyeColor * 3f);
        }

        private void SpawnParticleBurst(Vector3 position, Color color, int count, float size, float speed)
        {
            GameObject burst = new GameObject("SerpentPulse");
            burst.transform.position = position;

            ParticleSystem particles = burst.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.duration = 0.08f;
            main.loop = false;
            main.startLifetime = _particleLife;
            main.startSpeed = speed;
            main.startSize = size;
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = false;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.35f;

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(color, 0f),
                    new GradientColorKey(Color.white, 0.35f),
                    new GradientColorKey(color, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.75f, 0.35f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            particles.Emit(count);
            Destroy(burst, _particleLife + 0.2f);
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

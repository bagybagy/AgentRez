using UnityEngine;
using AreaX.Targets;
using AreaX.Managers;

namespace AreaX.Combat
{
    [RequireComponent(typeof(TrailRenderer))]
    public class Projectile : MonoBehaviour
    {
        [Header("VFX")]
        [SerializeField] private GameObject _impactVFX;
        [SerializeField] private float _initialSpeedRange = 20f;
        [SerializeField] private float _fallbackFlightTime = 0.65f;
        [SerializeField] private Color _projectileColor = new Color(0.4f, 1f, 1f, 1f);
        [SerializeField] private Color _impactColor = new Color(1f, 0.35f, 0.08f, 1f);

        private Target _target;
        private double _impactTime;
        private double _spawnSongTime;
        private float _spawnRealtime;
        private float _fallbackImpactRealtime;
        private Vector3 _velocity;
        private bool _isInitialized = false;
        private bool _hitSoundScheduled;

        public void Initialize(Target target, double impactTime)
        {
            ConfigureTrail();

            _target = target;
            _impactTime = impactTime;
            _spawnSongTime = GetSongTime();
            _spawnRealtime = Time.time;
            _fallbackImpactRealtime = _spawnRealtime + Mathf.Max(0.12f, (float)(_impactTime - _spawnSongTime));
            if (_fallbackImpactRealtime <= _spawnRealtime + 0.05f)
            {
                _fallbackImpactRealtime = _spawnRealtime + _fallbackFlightTime;
            }
            
            // Initial Velocity (Randomized for dispersal)
            _velocity = Random.insideUnitSphere * _initialSpeedRange;
            // Bias forward slightly
            _velocity += transform.forward * 10f;

            _isInitialized = true;

            if (AreaX.Audio.HitSoundSynthesizer.Instance != null)
            {
                _hitSoundScheduled = AreaX.Audio.HitSoundSynthesizer.Instance.ScheduleHitSoundAtSongTime(_impactTime);
            }
        }

        private void Update()
        {
            if (!_isInitialized) return;
            
            // Check if object destroyed externally
            if (_target == null)
            {
                Destroy(gameObject);
                return;
            }

            double currentTime = GetSongTime();
            double timeRemaining = _impactTime - currentTime; // period
            if (MusicManager.Instance == null || MusicManager.Instance.Clock == null || !MusicManager.Instance.Clock.HasStarted)
            {
                timeRemaining = _fallbackImpactRealtime - Time.time;
            }

            // Check for Impact
            if (timeRemaining <= 0f)
            {
                 Impact();
                 return;
            }

            UpdatePosition(timeRemaining);
        }

        private void UpdatePosition(double timeRemaining)
        {
             // Kinematics:
             // diff = v*t + 1/2*a*t^2
             // a = 2*(diff - v*t) / t^2
             
             // Need double precision for time? Unity Vectos are float.
             // Casting timeRemaining to float might be okay for short durations, 
             // but 't^2' gets small as we approach impact.
             
             float t = (float)timeRemaining;
             if (t < Time.deltaTime) t = Time.deltaTime; // Avoid div by zero

             Vector3 diff = _target.transform.position - transform.position;
             
             // Calculate Acceleration required to hit target at exactly t
             Vector3 acceleration = (diff - _velocity * t) * 2f / (t * t);
             
             // Update Velocity
             _velocity += acceleration * Time.deltaTime;
             
             // Update Position
             transform.position += _velocity * Time.deltaTime;
             
             // Rotation (Look at direction of movement)
             if (_velocity.sqrMagnitude > 0.01f)
             {
                 transform.rotation = Quaternion.LookRotation(_velocity);
             }
        }

        private void Impact()
        {
            if (_target != null)
            {
                transform.position = _target.transform.position;
                _target.OnHit();
                if (!_hitSoundScheduled && AreaX.Audio.HitSoundSynthesizer.Instance != null)
                {
                    AreaX.Audio.HitSoundSynthesizer.Instance.PlayHitSound();
                }
            }

            if (_impactVFX != null)
            {
                Instantiate(_impactVFX, transform.position, Quaternion.identity);
            }
            else
            {
                SpawnImpactBurst(transform.position);
            }
            
            Destroy(gameObject);
        }

        private void ConfigureTrail()
        {
            TrailRenderer trail = GetComponent<TrailRenderer>();
            if (trail == null) return;

            trail.time = 0.42f;
            trail.minVertexDistance = 0.04f;
            trail.widthMultiplier = 0.16f;
            trail.numCapVertices = 4;
            trail.numCornerVertices = 2;

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(_projectileColor, 0.45f),
                    new GradientColorKey(new Color(0.05f, 0.25f, 1f, 1f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.75f, 0.35f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            trail.colorGradient = gradient;

            Material material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.color = _projectileColor;
            trail.sharedMaterial = material;
        }

        private void SpawnImpactBurst(Vector3 position)
        {
            GameObject burst = new GameObject("ProjectileImpactBurst");
            burst.transform.position = position;

            ParticleSystem particles = burst.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.duration = 0.05f;
            main.loop = false;
            main.startLifetime = 0.55f;
            main.startSpeed = 7.5f;
            main.startSize = 0.09f;
            main.startColor = _impactColor;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = false;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.18f;

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(_impactColor, 0.35f),
                    new GradientColorKey(_projectileColor, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.8f, 0.35f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            particles.Emit(42);
            Destroy(burst, 0.8f);
        }

        private static double GetSongTime()
        {
            if (MusicManager.Instance == null)
            {
                return 0d;
            }

            return MusicManager.Instance.GetAudioTime();
        }
    }
}

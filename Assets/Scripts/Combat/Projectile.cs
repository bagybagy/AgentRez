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
            
            Destroy(gameObject);
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

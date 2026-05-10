using AreaX.Events;
using AreaX.Managers;
using UnityEngine;

namespace AreaX.Environment
{
    public class RhythmStarfield : MonoBehaviour
    {
        [SerializeField] private int _particleCount = 1200;
        [SerializeField] private Vector3 _fieldSize = new Vector3(120f, 70f, 180f);
        [SerializeField] private Color _baseColor = new Color(0.1f, 0.75f, 1f, 0.55f);
        [SerializeField] private Color _beatColor = new Color(1f, 0.18f, 0.85f, 0.9f);

        private ParticleSystem _particles;
        private ParticleSystem.Particle[] _particleBuffer;
        private float _beatPulse;

        public static RhythmStarfield CreateDefault()
        {
            GameObject starfield = new GameObject("RhythmStarfield");
            RhythmStarfield rhythmStarfield = starfield.AddComponent<RhythmStarfield>();
            rhythmStarfield.Build();
            return rhythmStarfield;
        }

        private void Awake()
        {
            if (_particles == null)
            {
                Build();
            }
        }

        private void Start()
        {
            if (BeatManager.Instance != null)
            {
                BeatManager.Instance.OnBeat.AddListener(HandleBeat);
                BeatManager.Instance.OnMeasure.AddListener(HandleMeasure);
            }
        }

        private void OnDestroy()
        {
            if (BeatManager.Instance != null)
            {
                BeatManager.Instance.OnBeat.RemoveListener(HandleBeat);
                BeatManager.Instance.OnMeasure.RemoveListener(HandleMeasure);
            }
        }

        private void Update()
        {
            _beatPulse = Mathf.MoveTowards(_beatPulse, 0f, Time.deltaTime * 3.2f);
            AnimateParticles();
        }

        public void Build()
        {
            _particles = GetComponent<ParticleSystem>();
            if (_particles == null)
            {
                _particles = gameObject.AddComponent<ParticleSystem>();
            }

            ParticleSystem.MainModule main = _particles.main;
            main.loop = true;
            main.playOnAwake = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = _particleCount;
            main.startLifetime = 9999f;
            main.startSpeed = 0f;
            main.startSize = 0.06f;
            main.startColor = _baseColor;

            ParticleSystem.EmissionModule emission = _particles.emission;
            emission.enabled = false;

            ParticleSystem.ShapeModule shape = _particles.shape;
            shape.enabled = false;

            _particleBuffer = new ParticleSystem.Particle[_particleCount];
            for (int i = 0; i < _particleBuffer.Length; i++)
            {
                Vector3 position = new Vector3(
                    Random.Range(-_fieldSize.x, _fieldSize.x),
                    Random.Range(-_fieldSize.y, _fieldSize.y),
                    Random.Range(8f, _fieldSize.z)
                );

                _particleBuffer[i] = new ParticleSystem.Particle
                {
                    position = position,
                    startLifetime = 9999f,
                    remainingLifetime = 9999f,
                    startSize = Random.Range(0.025f, 0.1f),
                    startColor = _baseColor
                };
            }

            _particles.SetParticles(_particleBuffer, _particleBuffer.Length);
            _particles.Play();
        }

        private void AnimateParticles()
        {
            if (_particles == null || _particleBuffer == null) return;

            int count = _particles.GetParticles(_particleBuffer);
            float songTime = MusicManager.Instance != null ? (float)MusicManager.Instance.GetAudioTime() : Time.time;
            Color color = Color.Lerp(_baseColor, _beatColor, _beatPulse);

            for (int i = 0; i < count; i++)
            {
                Vector3 position = _particleBuffer[i].position;
                float lane = Mathf.Sin(songTime * 0.9f + i * 0.031f);
                position.z -= Time.deltaTime * Mathf.Lerp(5f, 18f, _beatPulse);
                position.x += lane * Time.deltaTime * 0.6f;

                if (position.z < -12f)
                {
                    position.z = _fieldSize.z;
                    position.x = Random.Range(-_fieldSize.x, _fieldSize.x);
                    position.y = Random.Range(-_fieldSize.y, _fieldSize.y);
                }

                _particleBuffer[i].position = position;
                _particleBuffer[i].startColor = color;
                _particleBuffer[i].startSize = Mathf.Lerp(0.04f, 0.14f, _beatPulse);
            }

            _particles.SetParticles(_particleBuffer, count);
        }

        private void HandleBeat(BeatEvent evt)
        {
            _beatPulse = 1f;
        }

        private void HandleMeasure(MeasureEvent evt)
        {
            _beatPulse = 1.35f;
        }
    }
}

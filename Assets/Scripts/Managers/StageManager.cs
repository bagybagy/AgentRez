using UnityEngine;
using AreaX.Targets;
using AreaX.Boss;
using System.Linq;
using System.Collections.Generic;

namespace AreaX.Managers
{
    public class StageManager : MonoBehaviour
    {
        public static StageManager Instance { get; private set; }

        private List<Target> _allTargets = new List<Target>();
        private bool _isGameEnded = false;
        private SeaSerpentBoss _boss;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DisablePrototypeTargetSpawner();
        }

        private void Start()
        {
            EnsureBossStage();

            // Start Game Session
            if (MusicManager.Instance != null)
            {
                MusicManager.Instance.PlayMusic();
            }
            
            if (BeatManager.Instance != null)
            {
                BeatManager.Instance.StartRhythm();
            }
        }

        private void EnsureBossStage()
        {
            _boss = FindObjectOfType<SeaSerpentBoss>();
            if (_boss == null)
            {
                _boss = SeaSerpentBoss.CreateDefault();
            }

            TargetSpawner spawner = FindObjectOfType<TargetSpawner>();
            if (spawner != null)
            {
                spawner.gameObject.SetActive(false);
            }
        }

        private void DisablePrototypeTargetSpawner()
        {
            TargetSpawner spawner = FindObjectOfType<TargetSpawner>();
            if (spawner != null)
            {
                spawner.gameObject.SetActive(false);
            }
        }
        
        public void RegisterTarget(Target t)
        {
            if(!_allTargets.Contains(t)) _allTargets.Add(t);
        }

        private void Update()
        {
            if (_isGameEnded) return;

            // Simple check (Optimization: Event based is better but this is prototype)
            if (_allTargets.Count > 0)
            {
                // Check if any active/idle/locked target exists
                bool anyAlive = _allTargets.Any(t => t != null && t.IsLockable);
                
                if (!anyAlive)
                {
                    EndStage();
                }
            }
        }

        private void EndStage()
        {
            _isGameEnded = true;
            Debug.Log("STAGE COMPLETED - All Targets Processed");
            
            // Stop Music? Or Fade out?
            // "BGM終了と同時に体験を終了する" -> If BGM is long, we might just wait.
            // But spec says "End experience when all targets processed AND BGM ends?"
            // Phase 10 tasks: "Detect all targets processed -> End"
            // "BGM終了と同時に体験を終了する" (Phase 10 spec line 274) logic is slightly ambiguous.
            // Does it mean Force end? Or wait for BGM?
            // "BGM 終了と同時に" implies sync.
            // For now, I will just Log completion.
        }
    }
}

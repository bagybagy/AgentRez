using AreaX.Boss;
using UnityEngine;
using UnityEngine.UI;

namespace AreaX.UI
{
    public class BossStageHUD : MonoBehaviour
    {
        private const int Padding = 28;

        private SeaSerpentBoss _boss;
        private Text _statusText;
        private bool _completed;

        public static BossStageHUD Create(SeaSerpentBoss boss)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObject = new GameObject("BossStageCanvas");
                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();
            }

            GameObject hudObject = new GameObject("BossStageHUD");
            hudObject.transform.SetParent(canvas.transform, false);

            BossStageHUD hud = hudObject.AddComponent<BossStageHUD>();
            hud.Initialize(boss);
            return hud;
        }

        public void Initialize(SeaSerpentBoss boss)
        {
            _boss = boss;
            _statusText = CreateStatusText();
            Refresh();
        }

        private void Update()
        {
            Refresh();
        }

        public void ShowComplete()
        {
            _completed = true;
            if (_statusText != null)
            {
                _statusText.alignment = TextAnchor.MiddleCenter;
                _statusText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                _statusText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                _statusText.rectTransform.anchoredPosition = Vector2.zero;
                _statusText.rectTransform.sizeDelta = new Vector2(520f, 120f);
                _statusText.fontSize = 34;
                _statusText.text = "STAGE CLEAR";
            }
        }

        private Text CreateStatusText()
        {
            GameObject textObject = new GameObject("BossStatusText");
            textObject.transform.SetParent(transform, false);

            Text text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 18;
            text.color = new Color(0.7f, 1f, 1f, 0.92f);
            text.alignment = TextAnchor.UpperLeft;
            text.raycastTarget = false;

            RectTransform rect = text.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(Padding, -Padding);
            rect.sizeDelta = new Vector2(360f, 88f);

            return text;
        }

        private void Refresh()
        {
            if (_completed || _boss == null || _statusText == null) return;

            _statusText.text =
                $"SERPENT CORE\nPHASE {_boss.CurrentPhase}/{_boss.PhaseCount}\nLOCK POINTS {_boss.RemainingLockPoints}";
        }
    }
}

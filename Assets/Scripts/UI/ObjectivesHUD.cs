using System.Collections.Generic;
using TMPro;
using UnityEngine;
using RetwineMake.Gameplay;

namespace RetwineMake.UI
{
    public class ObjectivesHUD : MonoBehaviour
    {
        [SerializeField] ObjectiveManager objectiveManager;
        [SerializeField] Transform listParent;

        readonly Dictionary<string, TMP_Text> entries = new Dictionary<string, TMP_Text>();

        void Start()
        {
            foreach (var o in objectiveManager.Objectives)
            {
                var go = new GameObject($"Obj_{o.id}", typeof(RectTransform));
                go.transform.SetParent(listParent, false);

                var text = go.AddComponent<TextMeshProUGUI>();
                text.fontSize = 18f;
                text.color = UITheme.TextMuted;
                text.outlineWidth = 0.15f;
                text.outlineColor = new Color(0f, 0f, 0f, 0.8f);
                text.text = "- " + o.description;

                entries[o.id] = text;
            }

            objectiveManager.OnObjectiveUpdated += Refresh;
        }

        void OnDestroy()
        {
            if (objectiveManager != null)
                objectiveManager.OnObjectiveUpdated -= Refresh;
        }

        void Refresh(Objective o)
        {
            if (!entries.TryGetValue(o.id, out var text))
                return;

            if (o.state == ObjectiveState.Complete)
            {
                text.color = UITheme.AccentBright;
                text.fontStyle = FontStyles.Strikethrough;
            }
            else if (o.state == ObjectiveState.Failed)
            {
                text.color = new Color(0.85f, 0.25f, 0.25f);
                text.fontStyle = FontStyles.Strikethrough;
            }
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace RetwineMake.UI
{
    [System.Serializable]
    public struct LevelEntry
    {
        public string displayName;
        public string sceneName;
    }

    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] GameObject mainPanel;
        [SerializeField] GameObject levelPanel;
        [SerializeField] Button startButton;
        [SerializeField] Button quitButton;
        [SerializeField] Button backButton;
        [SerializeField] Transform levelListParent;
        [SerializeField] LevelEntry[] levels;
        [SerializeField] Sprite buttonSprite;

        void Awake()
        {
            startButton.onClick.AddListener(ShowLevelSelect);
            quitButton.onClick.AddListener(QuitGame);
            backButton.onClick.AddListener(ShowMainPanel);

            BuildLevelButtons();
            ShowMainPanel();
        }

        void BuildLevelButtons()
        {
            for (int i = levelListParent.childCount - 1; i >= 0; i--)
                Destroy(levelListParent.GetChild(i).gameObject);

            foreach (var level in levels)
            {
                var go = new GameObject($"LevelButton_{level.sceneName}", typeof(RectTransform));
                go.transform.SetParent(levelListParent, false);

                var layoutElement = go.AddComponent<LayoutElement>();
                layoutElement.preferredHeight = 56f;
                layoutElement.preferredWidth = 420f;

                var image = go.AddComponent<Image>();
                image.sprite = buttonSprite;
                image.type = buttonSprite != null ? Image.Type.Sliced : Image.Type.Simple;
                image.color = Color.white;

                var button = go.AddComponent<Button>();
                button.targetGraphic = image;
                button.colors = UITheme.ButtonColors();

                var textGO = new GameObject("Text", typeof(RectTransform));
                textGO.transform.SetParent(go.transform, false);
                var text = textGO.AddComponent<TextMeshProUGUI>();
                text.text = level.displayName;
                text.alignment = TextAlignmentOptions.Center;
                text.fontSize = 24f;
                text.color = UITheme.AccentBright;
                var textRT = text.rectTransform;
                textRT.anchorMin = Vector2.zero;
                textRT.anchorMax = Vector2.one;
                textRT.offsetMin = Vector2.zero;
                textRT.offsetMax = Vector2.zero;

                string sceneToLoad = level.sceneName;
                button.onClick.AddListener(() => LoadLevel(sceneToLoad));
            }
        }

        void ShowMainPanel()
        {
            mainPanel.SetActive(true);
            levelPanel.SetActive(false);
        }

        void ShowLevelSelect()
        {
            mainPanel.SetActive(false);
            levelPanel.SetActive(true);
        }

        void LoadLevel(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        void QuitGame()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}

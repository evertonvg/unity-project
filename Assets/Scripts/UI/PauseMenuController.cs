using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using RetwineMake.Player;

namespace RetwineMake.UI
{
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField] InputActionAsset inputActions;
        [SerializeField] GameObject pausePanel;
        [SerializeField] Button resumeButton;
        [SerializeField] Button mainMenuButton;
        [SerializeField] Button quitButton;
        [SerializeField] FirstPersonController movement;
        [SerializeField] WeaponController weapon;
        [SerializeField] PlayerHealth health;
        [SerializeField] string mainMenuSceneName = "MainMenu";

        InputAction pauseAction;
        bool isPaused;

        void Awake()
        {
            var map = inputActions.FindActionMap("Player", throwIfNotFound: true);
            pauseAction = map.FindAction("Pause", throwIfNotFound: true);

            resumeButton.onClick.AddListener(Resume);
            mainMenuButton.onClick.AddListener(GoToMainMenu);
            quitButton.onClick.AddListener(QuitGame);

            pausePanel.SetActive(false);
        }

        void OnEnable() => pauseAction.Enable();
        void OnDisable() => pauseAction.Disable();

        void Update()
        {
            if (health != null && health.IsDead)
                return;

            if (pauseAction.WasPressedThisFrame())
            {
                if (isPaused)
                    Resume();
                else
                    Pause();
            }
        }

        void Pause()
        {
            isPaused = true;
            Time.timeScale = 0f;
            pausePanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (movement != null) movement.enabled = false;
            if (weapon != null) weapon.enabled = false;
        }

        void Resume()
        {
            isPaused = false;
            Time.timeScale = 1f;
            pausePanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (movement != null) movement.enabled = true;
            if (weapon != null) weapon.enabled = true;
        }

        void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
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

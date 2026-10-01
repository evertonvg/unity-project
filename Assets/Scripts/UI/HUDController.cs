using TMPro;
using UnityEngine;
using RetwineMake.Player;
using RetwineMake.Gameplay;

namespace RetwineMake.UI
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] PlayerHealth playerHealth;
        [SerializeField] WeaponController weapon;
        [SerializeField] TMP_Text healthText;
        [SerializeField] TMP_Text ammoText;
        [SerializeField] GameObject deathPanel;

        [Header("Interaction")]
        [SerializeField] PlayerInteractor interactor;
        [SerializeField] TMP_Text interactionPromptText;

        [Header("Mission")]
        [SerializeField] ObjectiveManager objectiveManager;
        [SerializeField] GameObject missionCompletePanel;
        [SerializeField] GameObject missionFailedPanel;

        void OnEnable()
        {
            if (objectiveManager != null)
            {
                objectiveManager.OnMissionComplete += HandleMissionComplete;
                objectiveManager.OnMissionFailed += HandleMissionFailed;
            }
        }

        void OnDisable()
        {
            if (objectiveManager != null)
            {
                objectiveManager.OnMissionComplete -= HandleMissionComplete;
                objectiveManager.OnMissionFailed -= HandleMissionFailed;
            }
        }

        void Update()
        {
            if (playerHealth != null && healthText != null)
                healthText.text = $"HP {Mathf.CeilToInt(playerHealth.CurrentHealth)}/{Mathf.CeilToInt(playerHealth.MaxHealth)}";

            if (weapon != null && ammoText != null)
                ammoText.text = weapon.IsReloading ? "RELOADING" : $"{weapon.CurrentAmmo} / {weapon.MagazineSize}";

            if (deathPanel != null)
                deathPanel.SetActive(playerHealth != null && playerHealth.IsDead);

            if (interactionPromptText != null)
            {
                var current = interactor != null ? interactor.Current : null;
                bool show = current != null && current.CanInteract && !string.IsNullOrEmpty(current.InteractionPrompt);
                interactionPromptText.gameObject.SetActive(show);
                if (show)
                    interactionPromptText.text = current.InteractionPrompt;
            }
        }

        void HandleMissionComplete()
        {
            if (missionCompletePanel != null)
                missionCompletePanel.SetActive(true);

            SetPlayerControlsEnabled(false);
        }

        void HandleMissionFailed()
        {
            if (missionFailedPanel != null)
                missionFailedPanel.SetActive(true);

            SetPlayerControlsEnabled(false);
        }

        void SetPlayerControlsEnabled(bool enabled)
        {
            Cursor.lockState = enabled ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !enabled;

            if (weapon != null)
                weapon.enabled = enabled;
        }
    }
}

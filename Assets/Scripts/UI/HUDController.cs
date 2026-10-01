using TMPro;
using UnityEngine;
using RetwineMake.Player;

namespace RetwineMake.UI
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] PlayerHealth playerHealth;
        [SerializeField] WeaponController weapon;
        [SerializeField] TMP_Text healthText;
        [SerializeField] TMP_Text ammoText;
        [SerializeField] GameObject deathPanel;

        void Update()
        {
            if (playerHealth != null && healthText != null)
                healthText.text = $"HP {Mathf.CeilToInt(playerHealth.CurrentHealth)}/{Mathf.CeilToInt(playerHealth.MaxHealth)}";

            if (weapon != null && ammoText != null)
                ammoText.text = weapon.IsReloading ? "RELOADING" : $"{weapon.CurrentAmmo} / {weapon.MagazineSize}";

            if (deathPanel != null)
                deathPanel.SetActive(playerHealth != null && playerHealth.IsDead);
        }
    }
}

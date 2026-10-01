using System;
using UnityEngine;

namespace RetwineMake.Player
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] float maxHealth = 100f;

        float currentHealth;
        bool isDead;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsDead => isDead;
        public event Action OnDeath;
        public event Action<float, float> OnHealthChanged;

        void Awake()
        {
            currentHealth = maxHealth;
        }

        public void ApplyDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
        {
            if (isDead)
                return;

            currentHealth = Mathf.Max(0f, currentHealth - amount);
            Debug.Log($"[PlayerHealth] Took {amount} damage ({currentHealth}/{maxHealth})");
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0f)
                Die();
        }

        void Die()
        {
            isDead = true;
            Debug.Log("[PlayerHealth] Player died.");
            OnDeath?.Invoke();

            var movement = GetComponent<FirstPersonController>();
            if (movement != null)
                movement.enabled = false;

            var weapon = GetComponent<WeaponController>();
            if (weapon != null)
                weapon.enabled = false;
        }
    }
}

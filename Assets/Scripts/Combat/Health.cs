using System;
using System.Collections;
using UnityEngine;
using RetwineMake.Player;

namespace RetwineMake.Combat
{
    [RequireComponent(typeof(Collider))]
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] float maxHealth = 50f;
        [SerializeField] Color hitFlashColor = Color.red;
        [SerializeField] float hitFlashDuration = 0.08f;
        [SerializeField] float deathTopplePitch = 85f;
        [SerializeField] float deathToppleTime = 0.35f;

        float currentHealth;
        bool isDead;
        Renderer[] renderers;
        Color[] originalColors;
        Collider col;
        Coroutine flashRoutine;
        Animator animator;

        static readonly int DieHash = Animator.StringToHash("Die");

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsDead => isDead;
        public event Action OnDeath;

        void Awake()
        {
            currentHealth = maxHealth;
            col = GetComponent<Collider>();
            animator = GetComponentInChildren<Animator>();

            renderers = GetComponentsInChildren<Renderer>();
            originalColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
                originalColors[i] = renderers[i].material.color;
        }

        public void ApplyDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
        {
            if (isDead)
                return;

            currentHealth = Mathf.Max(0f, currentHealth - amount);
            Debug.Log($"[Health] {name} took {amount} damage ({currentHealth}/{maxHealth})");

            if (flashRoutine != null)
                StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(HitFlash());

            if (currentHealth <= 0f)
                Die();
        }

        IEnumerator HitFlash()
        {
            foreach (var r in renderers)
                r.material.color = hitFlashColor;

            yield return new WaitForSeconds(hitFlashDuration);

            if (!isDead)
                for (int i = 0; i < renderers.Length; i++)
                    renderers[i].material.color = originalColors[i];

            flashRoutine = null;
        }

        void Die()
        {
            isDead = true;
            col.enabled = false;
            Debug.Log($"[Health] {name} died.");
            OnDeath?.Invoke();

            if (animator != null)
                animator.SetTrigger(DieHash);
            else
                StartCoroutine(ToppleRoutine());
        }

        IEnumerator ToppleRoutine()
        {
            Quaternion start = transform.rotation;
            Quaternion end = start * Quaternion.Euler(0f, 0f, deathTopplePitch);

            float t = 0f;
            while (t < deathToppleTime)
            {
                t += Time.deltaTime;
                transform.rotation = Quaternion.Slerp(start, end, t / deathToppleTime);
                yield return null;
            }

            transform.rotation = end;
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using RetwineMake.Player;

namespace RetwineMake.UI
{
    [RequireComponent(typeof(Image))]
    public class DamageFlashController : MonoBehaviour
    {
        [SerializeField] PlayerHealth playerHealth;
        [SerializeField] Color flashColor = new Color(0.75f, 0.05f, 0.05f, 0.45f);
        [SerializeField] float fadeOutDuration = 0.4f;

        Image image;
        float lastHealth = -1f;
        Coroutine flashRoutine;

        void Awake()
        {
            image = GetComponent<Image>();
            image.raycastTarget = false;
            SetAlpha(0f);

            if (playerHealth == null)
                playerHealth = FindAnyObjectByType<PlayerHealth>();
        }

        void OnEnable()
        {
            if (playerHealth == null)
                return;

            playerHealth.OnHealthChanged += HandleHealthChanged;
            lastHealth = playerHealth.CurrentHealth;
        }

        void OnDisable()
        {
            if (playerHealth != null)
                playerHealth.OnHealthChanged -= HandleHealthChanged;
        }

        void HandleHealthChanged(float current, float max)
        {
            if (lastHealth >= 0f && current < lastHealth)
                TriggerFlash();
            lastHealth = current;
        }

        void TriggerFlash()
        {
            if (flashRoutine != null)
                StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashRoutine());
        }

        IEnumerator FlashRoutine()
        {
            SetAlpha(flashColor.a);
            float t = 0f;
            while (t < fadeOutDuration)
            {
                t += Time.deltaTime;
                SetAlpha(Mathf.Lerp(flashColor.a, 0f, t / fadeOutDuration));
                yield return null;
            }

            SetAlpha(0f);
            flashRoutine = null;
        }

        void SetAlpha(float a)
        {
            image.color = new Color(flashColor.r, flashColor.g, flashColor.b, a);
        }
    }
}

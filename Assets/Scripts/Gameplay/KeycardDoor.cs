using System.Collections;
using UnityEngine;

namespace RetwineMake.Gameplay
{
    public class KeycardDoor : MonoBehaviour, IInteractable
    {
        [SerializeField] Transform door;
        [SerializeField] Vector3 openLocalEulerDelta = new Vector3(0f, 90f, 0f);
        [SerializeField] float openTime = 0.6f;

        bool open;

        public string InteractionPrompt => open ? string.Empty : "Press E to unlock (requires security pass)";
        public bool CanInteract => !open;

        public void Interact(GameObject interactor)
        {
            if (open)
                return;

            var inv = interactor.GetComponent<PlayerInventory>();
            if (inv == null || !inv.HasSecurityPass)
            {
                Debug.Log("[KeycardDoor] Locked - requires the security pass.");
                return;
            }

            open = true;
            StartCoroutine(OpenRoutine());
        }

        IEnumerator OpenRoutine()
        {
            Quaternion start = door.localRotation;
            Quaternion end = Quaternion.Euler(openLocalEulerDelta) * start;

            float t = 0f;
            while (t < openTime)
            {
                t += Time.deltaTime;
                door.localRotation = Quaternion.Slerp(start, end, t / openTime);
                yield return null;
            }

            door.localRotation = end;

            var col = door.GetComponent<Collider>();
            if (col != null)
                col.enabled = false;

            Debug.Log("[KeycardDoor] Vault unlocked.");
        }
    }
}

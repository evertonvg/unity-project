using System.Collections;
using UnityEngine;

namespace RetwineMake.Gameplay
{
    public class SafeContainer : MonoBehaviour, IInteractable
    {
        [SerializeField] float crackDuration = 2.5f;

        bool cracked;
        bool cracking;

        public string InteractionPrompt => cracked ? string.Empty : (cracking ? "Cracking..." : "Press E to crack safe");
        public bool CanInteract => !cracked && !cracking;

        public void Interact(GameObject interactor)
        {
            if (cracked || cracking)
                return;

            StartCoroutine(CrackRoutine(interactor));
        }

        IEnumerator CrackRoutine(GameObject interactor)
        {
            cracking = true;
            Debug.Log("[Safe] Cracking safe...");

            yield return new WaitForSeconds(crackDuration);

            cracked = true;
            cracking = false;
            interactor.GetComponent<PlayerInventory>()?.GrantSecurityPass();
            ObjectiveManager.Instance?.CompleteObjective("crack_safe");
            Debug.Log("[Safe] Cracked - security pass obtained.");
        }
    }
}

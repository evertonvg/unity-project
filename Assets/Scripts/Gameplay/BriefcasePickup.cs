using UnityEngine;

namespace RetwineMake.Gameplay
{
    public class BriefcasePickup : MonoBehaviour, IInteractable
    {
        bool collected;

        public string InteractionPrompt => collected ? string.Empty : "Press E to take the briefcase";
        public bool CanInteract => !collected;

        public void Interact(GameObject interactor)
        {
            if (collected)
                return;

            collected = true;
            interactor.GetComponent<PlayerInventory>()?.GrantBriefcase();
            ObjectiveManager.Instance?.CompleteObjective("collect_money");
            Debug.Log("[Briefcase] Collected Sir Robert's money.");
            gameObject.SetActive(false);
        }
    }
}

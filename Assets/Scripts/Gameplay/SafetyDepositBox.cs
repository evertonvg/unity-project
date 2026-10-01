using UnityEngine;

namespace RetwineMake.Gameplay
{
    public class SafetyDepositBox : MonoBehaviour, IInteractable
    {
        bool used;

        public string InteractionPrompt => used ? string.Empty : "Press E to open deposit box";
        public bool CanInteract => !used;

        public void Interact(GameObject interactor)
        {
            if (used)
                return;

            used = true;
            interactor.GetComponent<PlayerInventory>()?.GrantEquipment();
            ObjectiveManager.Instance?.CompleteObjective("collect_equipment");
            Debug.Log("[SafetyDepositBox] Equipment collected.");
        }
    }
}

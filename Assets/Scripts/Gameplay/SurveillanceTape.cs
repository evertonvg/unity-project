using UnityEngine;

namespace RetwineMake.Gameplay
{
    public class SurveillanceTape : MonoBehaviour, IInteractable
    {
        bool destroyed;

        public string InteractionPrompt => destroyed ? string.Empty : "Press E to destroy security tape";
        public bool CanInteract => !destroyed;

        public void Interact(GameObject interactor)
        {
            if (destroyed)
                return;

            destroyed = true;
            ObjectiveManager.Instance?.CompleteObjective("destroy_tape");
            Debug.Log("[SurveillanceTape] Destroyed.");
            gameObject.SetActive(false);
        }
    }
}

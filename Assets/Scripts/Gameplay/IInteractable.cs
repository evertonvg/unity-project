using UnityEngine;

namespace RetwineMake.Gameplay
{
    public interface IInteractable
    {
        string InteractionPrompt { get; }
        bool CanInteract { get; }
        void Interact(GameObject interactor);
    }
}

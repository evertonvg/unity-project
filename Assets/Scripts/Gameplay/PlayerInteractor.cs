using UnityEngine;
using UnityEngine.InputSystem;

namespace RetwineMake.Gameplay
{
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] InputActionAsset inputActions;
        [SerializeField] Transform eye;
        [SerializeField] float range = 3f;
        [SerializeField] LayerMask interactMask = ~0;

        InputAction interactAction;

        public IInteractable Current { get; private set; }

        void Awake()
        {
            var map = inputActions.FindActionMap("Player", throwIfNotFound: true);
            interactAction = map.FindAction("Interact", throwIfNotFound: true);
        }

        void OnEnable() => interactAction.Enable();
        void OnDisable() => interactAction.Disable();

        void Update()
        {
            Current = FindInteractable();

            if (Current != null && Current.CanInteract && interactAction.WasPressedThisFrame())
                Current.Interact(gameObject);
        }

        IInteractable FindInteractable()
        {
            Vector3 origin = eye != null ? eye.position : transform.position;
            Vector3 direction = eye != null ? eye.forward : transform.forward;

            if (Physics.Raycast(origin, direction, out var hit, range, interactMask, QueryTriggerInteraction.Collide))
                return hit.collider.GetComponentInParent<IInteractable>();

            return null;
        }
    }
}

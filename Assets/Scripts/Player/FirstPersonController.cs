using UnityEngine;
using UnityEngine.InputSystem;

namespace RetwineMake.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField] InputActionAsset inputActions;
        [SerializeField] Transform cameraTransform;

        [Header("Movement")]
        [SerializeField] float walkSpeed = 4.5f;
        [SerializeField] float sprintSpeed = 7.5f;
        [SerializeField] float crouchSpeed = 2.2f;
        [SerializeField] float jumpHeight = 1.2f;
        [SerializeField] float gravity = -18f;

        [Header("Look")]
        [SerializeField] float mouseSensitivity = 0.12f;
        [SerializeField] float minPitch = -85f;
        [SerializeField] float maxPitch = 85f;

        [Header("Crouch")]
        [SerializeField] float standHeight = 1.8f;
        [SerializeField] float crouchHeight = 1.0f;

        CharacterController controller;
        InputAction moveAction;
        InputAction lookAction;
        InputAction jumpAction;
        InputAction sprintAction;
        InputAction crouchAction;

        Vector3 velocity;
        float pitch;

        void Awake()
        {
            controller = GetComponent<CharacterController>();

            var map = inputActions.FindActionMap("Player", throwIfNotFound: true);
            moveAction = map.FindAction("Move", throwIfNotFound: true);
            lookAction = map.FindAction("Look", throwIfNotFound: true);
            jumpAction = map.FindAction("Jump", throwIfNotFound: true);
            sprintAction = map.FindAction("Sprint", throwIfNotFound: true);
            crouchAction = map.FindAction("Crouch", throwIfNotFound: true);
        }

        void OnEnable()
        {
            moveAction.Enable();
            lookAction.Enable();
            jumpAction.Enable();
            sprintAction.Enable();
            crouchAction.Enable();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void OnDisable()
        {
            moveAction.Disable();
            lookAction.Disable();
            jumpAction.Disable();
            sprintAction.Disable();
            crouchAction.Disable();
        }

        void Update()
        {
            HandleLook();
            HandleMove();
        }

        void HandleLook()
        {
            Vector2 look = lookAction.ReadValue<Vector2>();
            float yaw = look.x * mouseSensitivity;
            pitch = Mathf.Clamp(pitch - look.y * mouseSensitivity, minPitch, maxPitch);

            transform.Rotate(Vector3.up, yaw);
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        void HandleMove()
        {
            bool grounded = controller.isGrounded;
            if (grounded && velocity.y < 0f)
                velocity.y = -2f;

            bool crouching = crouchAction.IsPressed();
            controller.height = Mathf.Lerp(controller.height, crouching ? crouchHeight : standHeight, Time.deltaTime * 10f);

            Vector2 input = moveAction.ReadValue<Vector2>();
            float speed = crouching ? crouchSpeed : (sprintAction.IsPressed() ? sprintSpeed : walkSpeed);
            Vector3 move = (transform.right * input.x + transform.forward * input.y) * speed;
            controller.Move(move * Time.deltaTime);

            if (jumpAction.WasPressedThisFrame() && grounded && !crouching)
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
    }
}

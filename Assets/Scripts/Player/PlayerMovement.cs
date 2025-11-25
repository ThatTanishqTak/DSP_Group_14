using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float swimSpeed = 2.5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Jump / Gravity")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 2f;

    [Header("Swimming")]
    [Tooltip("Layers considered water")]
    [SerializeField] private LayerMask waterLayer;
    [Tooltip("Radius around the player used to check if inside water")]
    [SerializeField] private float waterCheckRadius = 0.5f;
    [Tooltip("Offset from player position used for water check")]
    [SerializeField] private Vector3 waterCheckOffset = new(0, 0.5f, 0);
    [SerializeField] private float swimVerticalSpeed = 2.5f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isJumping;
    private bool isSwimming;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null) { cameraTransform = Camera.main.transform; } // Auto-fill main camera here
    }

    private void Update()
    {
        //Check if player is inside water volume by layer
        bool inWater = Physics.CheckSphere(transform.position + waterCheckOffset, waterCheckRadius, waterLayer);

        // Handle state transition between walking and swimming
        if (inWater && !isSwimming)
        {
            // Entering water
            isSwimming = true;
            velocity.y = 0f;   // Kill vertical velocity so we don't drag gravity underwater
        }
        else if (!inWater && isSwimming) { isSwimming = false; }// Exiting water

        //Handle movement based on state
        if (isSwimming) { HandleSwimming(); }
        else
        {
            HandleMovement();
            HandleGravityAndJump();
        }
    }

    private Vector2 ReadMovementInput()
    {
        Vector2 input = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) { input.y += 1; }
            if (Keyboard.current.sKey.isPressed) { input.y -= 1; }
            if (Keyboard.current.aKey.isPressed) { input.x -= 1; }
            if (Keyboard.current.dKey.isPressed) { input.x += 1; }
        }

        if (input.sqrMagnitude > 1f) { input = input.normalized; }

        return input;
    }

    private void HandleMovement()
    {
        Vector2 input = ReadMovementInput();
        Vector3 moveDir = Vector3.zero;

        if (input.sqrMagnitude > 0.01f)
        {
            // Camera-relative movement
            Vector3 forward = Vector3.forward;
            Vector3 right = Vector3.right;

            if (cameraTransform != null)
            {
                forward = cameraTransform.forward;
                right = cameraTransform.right;

                forward.y = 0f;
                right.y = 0f;

                forward.Normalize();
                right.Normalize();
            }

            moveDir = forward * input.y + right * input.x;
            moveDir.Normalize();

            // Rotate player towards movement direction
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        controller.Move(moveSpeed * Time.deltaTime * moveDir);
    }

    private void HandleGravityAndJump()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0f)
        {
            // Small downward force to keep grounded
            velocity.y = -2f;
            isJumping = false;
        }

        // Jump
        if (isGrounded && Keyboard.current.spaceKey.isPressed && !isJumping)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            isJumping = true;
        }

        // Gravity
        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleSwimming()
    {
        // Horizontal input (WASD)
        Vector2 input = ReadMovementInput();

        // Camera-relative horizontal direction
        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;

        if (cameraTransform != null)
        {
            forward = cameraTransform.forward;
            right = cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();
        }

        Vector3 horizontalDir = forward * input.y + right * input.x;

        // Vertical swimming (Space = up, LeftCtrl = down)
        float vertical = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.spaceKey.isPressed) { vertical += 1f; }
            if (Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.leftShiftKey.isPressed) { vertical -= 1f; }
        }

        Vector3 swimDir = horizontalDir + Vector3.up * vertical;

        if (swimDir.sqrMagnitude > 1f) { swimDir = swimDir.normalized; }

        // Rotate towards swimming direction if moving
        if (horizontalDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(horizontalDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        controller.Move(swimDir * swimSpeed * Time.deltaTime);
    }

    // Editor: visualize the water check in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + waterCheckOffset, waterCheckRadius);
    }
}
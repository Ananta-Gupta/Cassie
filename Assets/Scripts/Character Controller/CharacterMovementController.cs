using UnityEngine;
using UnityEngine.InputSystem;

namespace Character
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class CharacterMovementController : MonoBehaviour
    {
        [SerializeField] float walkSpeed = 4.0f;
        [SerializeField] float sprintSpeed = 8.0f;
        [SerializeField] float acceleration = 10.0f;
        [SerializeField] float deceleration = 10.0f;

        [SerializeField] float jumpForce = 5.0f;
        [SerializeField] float desiredJumpHeight = 2.0f;


        [SerializeField] private bool isJumping = false;
        [SerializeField] private bool isMoving = false;
        [SerializeField] private bool isSprinting = false;

        private Rigidbody rb;
        private CapsuleCollider capsuleCollider;
        private Vector2 moveInput;
        private Vector3 moveDirection = Vector3.zero;

        public static CharacterInputController InputController { get; private set; }

        private void Awake()
        {
            InputController = new CharacterInputController();
        }

        private void OnEnable()
        {
            InputController.Enable();

            InputConfig();
        }

        private void OnDisable()
        {
            InputController.Disable();
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            rb = GetComponent<Rigidbody>();
            capsuleCollider = GetComponent<CapsuleCollider>();
        }

        #region event callbacks
        void InputConfig()
        {
            InputController.Player.Move.performed += Input_Move;
            InputController.Player.Sprint.performed += ctx =>
            {
                if (isMoving)
                    isSprinting = ctx.ReadValue<float>() > 0;
            };
        }

        void Input_Move(InputAction.CallbackContext Action)
        {
            moveInput = Action.ReadValue<Vector2>();
        }
        #endregion

        private void Update()
        {
            HandleMovement();
            HandleJump();
        }

        private void HandleMovement()
        {
            Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

            isMoving = direction.magnitude > 0.1f;

            float targetSpeed = isSprinting ? sprintSpeed : walkSpeed;

            if (isMoving)
            {
                float currentSpeed = rb.velocity.magnitude;
                float speedDifference = targetSpeed - currentSpeed;
                float accelerationFactor = Mathf.Clamp01(speedDifference / targetSpeed) * acceleration;

                if (!isSprinting && currentSpeed > walkSpeed)
                {
                    rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.deltaTime * deceleration);
                }

                moveDirection = direction * targetSpeed;
                rb.velocity = Vector3.Lerp(rb.velocity, moveDirection, Time.deltaTime * accelerationFactor);
            }
            else
            {
                rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.deltaTime * deceleration);
            }

            UpdateAnimator(isMoving, direction.x * (isSprinting ? 2 : 1), direction.z * (isSprinting ? 2 : 1));
        }

        private void HandleJump()
        {
            if (isJumping && capsuleCollider.bounds.min.y < 0.01f)
            {
                float jumpForce = Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * desiredJumpHeight);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
                isJumping = false;
            }

            if (InputController.Player.Jump.triggered && !isJumping && capsuleCollider.bounds.min.y < 0.01f)
            {
                isJumping = true;
            }

            CharacterManager.Instance.animator.SetBool("isJumping", isJumping);

           // float currentHeight = 1 - transform.localPosition.y;

           // CharacterManager.Instance.animator.SetFloat("height", Mathf.Clamp(transform.localPosition.y, 0f, 1f));
        }

        private void UpdateAnimator(bool isMoving, float x, float y)
        {
            CharacterManager.Instance.animator.SetBool("isWalking", isMoving);
            CharacterManager.Instance.animator.SetFloat("x", x);
            CharacterManager.Instance.animator.SetFloat("y", y);
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles all player input and movement for the Battle Duck.
/// Attach to the Player GameObject with a Rigidbody2D and BoxCollider2D.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 14f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Combat")]
    [SerializeField] private WeaponSystem weaponSystem;
    [SerializeField] private float quackStunRadius = 3f;
    [SerializeField] private float quackStunDuration = 1.5f;
    [SerializeField] private float quackCooldown = 5f;

    [Header("References")]
    [SerializeField] private Transform firePoint;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded;
    private float horizontalInput;
    private bool facingRight = true;
    private float quackTimer;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction shootAction;
    private InputAction grenadeAction;
    private InputAction quackAction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (weaponSystem == null)
            weaponSystem = GetComponent<WeaponSystem>();

        SetupInputActions();
    }

    private void SetupInputActions()
    {
        moveAction = new InputAction("Move", InputActionType.Value);
        moveAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/a")
            .With("Positive", "<Keyboard>/d");
        moveAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/leftArrow")
            .With("Positive", "<Keyboard>/rightArrow");

        jumpAction = new InputAction("Jump", InputActionType.Button);
        jumpAction.AddBinding("<Keyboard>/space");
        jumpAction.AddBinding("<Keyboard>/w");
        jumpAction.AddBinding("<Keyboard>/upArrow");

        shootAction = new InputAction("Shoot", InputActionType.Button);
        shootAction.AddBinding("<Mouse>/leftButton");
        shootAction.AddBinding("<Keyboard>/leftCtrl");

        grenadeAction = new InputAction("Grenade", InputActionType.Button);
        grenadeAction.AddBinding("<Mouse>/rightButton");
        grenadeAction.AddBinding("<Keyboard>/leftAlt");

        quackAction = new InputAction("Quack", InputActionType.Button);
        quackAction.AddBinding("<Keyboard>/q");
    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        shootAction.Enable();
        grenadeAction.Enable();
        quackAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        shootAction.Disable();
        grenadeAction.Disable();
        quackAction.Disable();
    }

    private void OnDestroy()
    {
        moveAction.Dispose();
        jumpAction.Dispose();
        shootAction.Dispose();
        grenadeAction.Dispose();
        quackAction.Dispose();
    }

    private void Update()
    {
        horizontalInput = moveAction.ReadValue<float>();

        if (horizontalInput > 0 && !facingRight)
            Flip();
        else if (horizontalInput < 0 && facingRight)
            Flip();

        isGrounded = false;
        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (jumpAction.WasPressedThisFrame() && isGrounded)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        if (shootAction.WasPressedThisFrame() && weaponSystem != null)
            weaponSystem.Shoot(GetAimDirection());

        if (grenadeAction.WasPressedThisFrame() && weaponSystem != null)
            weaponSystem.ThrowGrenade(GetAimDirection());

        quackTimer -= Time.deltaTime;
        if (quackAction.WasPressedThisFrame() && quackTimer <= 0f)
        {
            Quack();
            quackTimer = quackCooldown;
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    /// <summary>
    /// Flips the player sprite horizontally.
    /// </summary>
    private void Flip()
    {
        facingRight = !facingRight;
        spriteRenderer.flipX = !facingRight;

        if (firePoint != null)
        {
            Vector3 pos = firePoint.localPosition;
            pos.x = -pos.x;
            firePoint.localPosition = pos;
        }
    }

    /// <summary>
    /// Returns the aim direction based on mouse position or facing direction.
    /// </summary>
    private Vector2 GetAimDirection()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
            return facingRight ? Vector2.right : Vector2.left;

        Mouse mouse = Mouse.current;
        if (mouse == null)
            return facingRight ? Vector2.right : Vector2.left;

        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(
            new Vector3(mouse.position.x.ReadValue(), mouse.position.y.ReadValue(), 0f));
        return (mouseWorldPos - transform.position).normalized;
    }

    /// <summary>
    /// QUACK! Stuns all enemies within radius. THE DUCK REVOLUTION HAS BEGUN!
    /// </summary>
    private void Quack()
    {
        Debug.Log("QUAAACK! *stun*");

        if (UIManager.Instance != null)
            UIManager.Instance.ShowTextPopup("QUAAACK!", transform.position + Vector3.up);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, quackStunRadius);
        foreach (Collider2D hit in hits)
        {
            EnemyBase enemy = hit.GetComponent<EnemyBase>();
            if (enemy != null)
                enemy.Stun(quackStunDuration);
        }
    }

    /// <summary>
    /// Draw ground check gizmo in editor for debugging.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, quackStunRadius);
    }
}


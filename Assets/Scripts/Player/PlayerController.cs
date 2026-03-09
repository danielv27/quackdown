using UnityEngine;

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

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Auto-find weapon system if not assigned
        if (weaponSystem == null)
            weaponSystem = GetComponent<WeaponSystem>();
    }

    private void Update()
    {
        // Read horizontal input (A/D or Left/Right arrows)
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Flip sprite based on movement direction
        if (horizontalInput > 0 && !facingRight)
            Flip();
        else if (horizontalInput < 0 && facingRight)
            Flip();

        // Ground check using overlap circle
        isGrounded = false;
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // Shoot (left mouse button or left ctrl)
        if (Input.GetButtonDown("Fire1"))
        {
            if (weaponSystem != null)
                weaponSystem.Shoot(GetAimDirection());
        }

        // Throw egg grenade (right mouse button or left alt)
        if (Input.GetButtonDown("Fire2"))
        {
            if (weaponSystem != null)
                weaponSystem.ThrowGrenade(GetAimDirection());
        }

        // Quack ability (Q key) - stuns nearby enemies
        quackTimer -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Q) && quackTimer <= 0f)
        {
            Quack();
            quackTimer = quackCooldown;
        }
    }

    private void FixedUpdate()
    {
        // Apply horizontal movement
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    /// <summary>
    /// Flips the player sprite horizontally.
    /// </summary>
    private void Flip()
    {
        facingRight = !facingRight;
        spriteRenderer.flipX = !facingRight;

        // Also flip the fire point
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

        // Use mouse position for aiming
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mouseWorldPos - transform.position).normalized;
        return direction;
    }

    /// <summary>
    /// QUACK! Stuns all enemies within radius. THE DUCK REVOLUTION HAS BEGUN!
    /// </summary>
    private void Quack()
    {
        Debug.Log("QUAAACK! *stun*");

        // Show quack popup
        if (UIManager.Instance != null)
            UIManager.Instance.ShowTextPopup("QUAAACK!", transform.position + Vector3.up);

        // Find all enemies in radius and stun them
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, quackStunRadius);
        foreach (Collider2D hit in hits)
        {
            EnemyBase enemy = hit.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.Stun(quackStunDuration);
            }
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

        // Draw quack radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, quackStunRadius);
    }
}

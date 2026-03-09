using UnityEngine;

/// <summary>
/// Controls the player character (Battle Duck).
/// Handles movement, jumping, shooting, egg grenades, and the Quack ability.
///
/// Required Components:
///   - Rigidbody2D
///   - Collider2D (CapsuleCollider2D or BoxCollider2D)
///   - SpriteRenderer
///   - WeaponSystem
///   - HealthSystem
///
/// Input Mapping (Legacy Input):
///   Horizontal   → A/D or Left/Right  (move)
///   Space        → Jump
///   Left Ctrl / Mouse0  → Shoot  (Fire1)
///   Mouse1       → Throw egg grenade (Fire2)
///   Q            → Quack ability
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(HealthSystem))]
[RequireComponent(typeof(WeaponSystem))]
public class PlayerController : MonoBehaviour
{
    // ---- Movement ----
    [Header("Movement")]
    [SerializeField] private float moveSpeed  = 8f;
    [SerializeField] private float jumpForce  = 16f;
    [SerializeField] private int   maxJumps   = 2;      // allows one double-jump

    // ---- Ground Detection ----
    [Header("Ground Check")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.15f;

    // ---- Quack ----
    [Header("Quack Ability")]
    [SerializeField] private float quackRadius   = 3f;
    [SerializeField] private float quackStunTime = 1.5f;
    [SerializeField] private float quackCooldown = 4f;

    // ---- References ----
    private Rigidbody2D   _rb;
    private SpriteRenderer _sr;
    private WeaponSystem   _weapon;
    private HealthSystem   _health;

    // ---- State ----
    private bool  _isGrounded;
    private int   _jumpsLeft;
    private float _quackTimer;

    // ------------------------------------------------
    void Awake()
    {
        _rb     = GetComponent<Rigidbody2D>();
        _sr     = GetComponent<SpriteRenderer>();
        _weapon = GetComponent<WeaponSystem>();
        _health = GetComponent<HealthSystem>();

        // Lock rotation so duck doesn't tip over
        _rb.freezeRotation = true;
    }

    void Start()
    {
        // Tell the camera to follow us
        CameraFollow cam = Camera.main?.GetComponent<CameraFollow>();
        cam?.SetTarget(transform);
    }

    void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        HandleMovement();
        HandleJump();
        HandleShooting();
        HandleQuack();
        UpdateSprite();
    }

    // ---- Movement ----
    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        _rb.velocity = new Vector2(h * moveSpeed, _rb.velocity.y);
    }

    // ---- Jump ----
    private void HandleJump()
    {
        // Ground check
        _isGrounded = Physics2D.OverlapCircle(
            groundCheck != null ? groundCheck.position : transform.position + Vector3.down * 0.5f,
            groundCheckRadius,
            groundLayer);

        if (_isGrounded)
            _jumpsLeft = maxJumps;

        if (Input.GetButtonDown("Jump") && _jumpsLeft > 0)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, jumpForce);
            _jumpsLeft--;
        }
    }

    // ---- Shooting ----
    private void HandleShooting()
    {
        // Continuous fire while button held
        if (Input.GetButton("Fire1"))
            _weapon.Shoot();

        // Throw egg grenade on Fire2
        if (Input.GetButtonDown("Fire2"))
            _weapon.ThrowGrenade();
    }

    // ---- Quack Ability ----
    private void HandleQuack()
    {
        _quackTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Q) && _quackTimer <= 0f)
        {
            _quackTimer = quackCooldown;
            PerformQuack();
        }
    }

    private void PerformQuack()
    {
        Debug.Log("QUAAAAACK!");

        // Find all enemies in range and stun them
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, quackRadius);
        foreach (Collider2D hit in hits)
        {
            EnemyBase enemy = hit.GetComponent<EnemyBase>();
            if (enemy != null)
                enemy.Stun(quackStunTime);
        }

        // Flash the sprite yellow as visual feedback
        StartCoroutine(FlashColor(Color.yellow, 0.2f));
    }

    // ---- Sprite Flip ----
    private void UpdateSprite()
    {
        float h = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(h) > 0.1f)
            _sr.flipX = h < 0f;
    }

    // ---- Utility ----
    private System.Collections.IEnumerator FlashColor(Color color, float duration)
    {
        Color original = _sr.color;
        _sr.color = color;
        yield return new WaitForSeconds(duration);
        _sr.color = original;
    }

    // ---- Debug Gizmos ----
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, quackRadius);
    }
}

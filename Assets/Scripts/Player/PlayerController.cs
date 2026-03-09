using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles all player input and movement for the Battle Duck.
/// Includes coyote time, jump buffering, squash/stretch, Wing Dash, and Ground Pound.
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

    [Header("Coyote Time & Jump Buffer")]
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.12f;

    [Header("Squash & Stretch")]
    [SerializeField] private float squashAmount = 0.3f;
    [SerializeField] private float squashRecoverSpeed = 8f;

    [Header("Wing Dash")]
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.18f;
    [SerializeField] private float dashCooldown = 1.5f;
    [SerializeField] private int dashAfterimageCount = 4;

    [Header("Ground Pound")]
    [SerializeField] private float groundPoundForce = 28f;
    [SerializeField] private float groundPoundRadius = 2.5f;
    [SerializeField] private float groundPoundDamage = 35f;

    [Header("Combat")]
    [SerializeField] private WeaponSystem weaponSystem;
    [SerializeField] private float quackStunRadius = 3f;
    [SerializeField] private float quackStunDuration = 1.5f;
    [SerializeField] private float quackCooldown = 5f;

    [Header("References")]
    [SerializeField] private Transform firePoint;

    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // State
    private bool isGrounded;
    private bool wasGrounded;
    private float horizontalInput;
    private bool facingRight = true;
    private float quackTimer;
    private float coyoteCounter;
    private float jumpBufferCounter;
    private float dashTimer;
    private bool isDashing;
    private bool isGroundPounding;
    private Vector3 baseScale;
    private float speedMultiplier = 1f;

    // Wall-cling state
    private bool isWallClinging;
    private int wallClingDir; // -1 = left wall, +1 = right wall

    // Input actions
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction shootAction;
    private InputAction grenadeAction;
    private InputAction quackAction;
    private InputAction dashAction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;

        if (weaponSystem == null)
            weaponSystem = GetComponent<WeaponSystem>();

        // Auto-attach combo system
        if (GetComponent<ComboSystem>() == null)
            gameObject.AddComponent<ComboSystem>();

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

        dashAction = new InputAction("Dash", InputActionType.Button);
        dashAction.AddBinding("<Keyboard>/leftShift");
        dashAction.AddBinding("<Keyboard>/rightShift");
    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        shootAction.Enable();
        grenadeAction.Enable();
        quackAction.Enable();
        dashAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        shootAction.Disable();
        grenadeAction.Disable();
        quackAction.Disable();
        dashAction.Disable();
    }

    private void OnDestroy()
    {
        moveAction.Dispose();
        jumpAction.Dispose();
        shootAction.Dispose();
        grenadeAction.Dispose();
        quackAction.Dispose();
        dashAction.Dispose();
    }

    private void Update()
    {
        if (isDashing) return;

        horizontalInput = moveAction.ReadValue<float>();

        if (horizontalInput > 0.1f && !facingRight) Flip();
        else if (horizontalInput < -0.1f && facingRight) Flip();

        // Ground check
        wasGrounded = isGrounded;
        isGrounded = groundCheck != null && Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Landing effects
        if (isGrounded && !wasGrounded)
            OnLand();

        // Wall-cling detection — airborne + pressing into a wall surface
        UpdateWallCling();

        // Coyote time counter
        if (isGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        // Jump buffer counter
        if (jumpAction.WasPressedThisFrame())
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        // Jump — consume buffer if grounded or within coyote window
        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            Jump();
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
        }

        // Ground pound — hold down while airborne
        if (!isGrounded && !isGroundPounding && moveAction.ReadValue<float>() == 0f)
        {
            // Check for down input (S or down arrow)
            if (Keyboard.current != null && (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed))
                StartCoroutine(DoGroundPound());
        }

        // Shoot
        if (shootAction.WasPressedThisFrame() && weaponSystem != null)
            weaponSystem.Shoot(GetAimDirection());

        // Auto-fire
        if (shootAction.IsPressed() && weaponSystem != null)
            weaponSystem.ShootAuto(GetAimDirection());

        // Grenade
        if (grenadeAction.WasPressedThisFrame() && weaponSystem != null)
            weaponSystem.ThrowGrenade(GetAimDirection());

        // Quack
        quackTimer -= Time.deltaTime;
        if (quackAction.WasPressedThisFrame() && quackTimer <= 0f)
        {
            Quack();
            quackTimer = quackCooldown;
        }

        // Dash
        dashTimer -= Time.deltaTime;
        if (dashAction.WasPressedThisFrame() && dashTimer <= 0f)
            StartCoroutine(DoDash());

        // Squash/stretch recover — skip when wall-clinging so the squash pose holds
        if (!isWallClinging)
            transform.localScale = Vector3.Lerp(transform.localScale, baseScale, squashRecoverSpeed * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (isDashing) return;
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed * speedMultiplier, rb.linearVelocity.y);
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        AudioManager.PlaySFX("jump");
        if (isWallClinging) EndWallCling();

        // Stretch upward on jump
        transform.localScale = new Vector3(baseScale.x * (1f - squashAmount * 0.5f), baseScale.y * (1f + squashAmount), baseScale.z);
    }

    private void OnLand()
    {
        // Squash on landing — intensity based on fall velocity
        float fallSpeed = Mathf.Abs(rb.linearVelocity.y);
        float squash = Mathf.Clamp(fallSpeed / 20f, 0.1f, 0.4f);
        transform.localScale = new Vector3(baseScale.x * (1f + squash), baseScale.y * (1f - squash), baseScale.z);

        // Landing dust
        Vector3 dustPos = groundCheck != null ? groundCheck.position : transform.position;
        ParticleManager.SpawnLandingDust(dustPos);

        // Shake based on fall speed
        CameraFollow.ShakeCamera(squash * 0.3f);
        AudioManager.PlaySFX("land");

        // If ground pounding, do shockwave
        if (isGroundPounding)
        {
            isGroundPounding = false;
            DoGroundPoundImpact();
        }
    }

    private IEnumerator DoGroundPound()
    {
        isGroundPounding = true;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, -groundPoundForce);
        AudioManager.PlaySFX("ground_pound");
        yield return new WaitUntil(() => isGrounded || !isGroundPounding);
    }

    private void DoGroundPoundImpact()
    {
        CameraFollow.ShakeCamera(0.4f);
        JuiceManager.Instance?.KillStop();
        AudioManager.PlaySFX("explosion");

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, groundPoundRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player")) continue;

            HealthSystem health = hit.GetComponent<HealthSystem>();
            if (health != null)
            {
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                float dmg = groundPoundDamage * (1f - dist / groundPoundRadius);
                health.TakeDamage(dmg);
            }

            Rigidbody2D hitRb = hit.GetComponent<Rigidbody2D>();
            if (hitRb != null)
            {
                Vector2 dir = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized + Vector2.up;
                hitRb.AddForce(dir.normalized * 400f);
            }

            DestructibleProp prop = hit.GetComponent<DestructibleProp>();
            prop?.TakeDamage(groundPoundDamage);
        }

        UIManager.Instance?.ShowTextPopup("GROUND POUND!", transform.position + Vector3.up);
    }

    private IEnumerator DoDash()
    {
        isDashing = true;
        dashTimer = dashCooldown;

        float dashDir = facingRight ? 1f : -1f;
        Vector2 dashVelocity = new Vector2(dashDir * dashSpeed, 0f);

        // Spawn afterimages
        StartCoroutine(SpawnAfterimages());

        AudioManager.PlaySFX("dash");

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            rb.linearVelocity = dashVelocity;
            elapsed += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
    }

    private IEnumerator SpawnAfterimages()
    {
        if (spriteRenderer == null) yield break;

        for (int i = 0; i < dashAfterimageCount; i++)
        {
            var ghost = new GameObject("DashAfterimage");
            ghost.transform.position = transform.position;
            ghost.transform.localScale = transform.localScale;

            var ghostSr = ghost.AddComponent<SpriteRenderer>();
            ghostSr.sprite = spriteRenderer.sprite;
            ghostSr.flipX = spriteRenderer.flipX;
            ghostSr.sortingOrder = spriteRenderer.sortingOrder - 1;
            ghostSr.color = new Color(0.4f, 0.8f, 1f, 0.6f);

            Destroy(ghost, 0.2f);
            yield return new WaitForSeconds(dashDuration / dashAfterimageCount);
        }
    }

    private void Quack()
    {
        AudioManager.PlaySFX("quack");
        UIManager.Instance?.ShowTextPopup("QUAAACK!", transform.position + Vector3.up);

        // Visual shockwave ring
        CreateQuackShockwave();

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, quackStunRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player")) continue;

            // Stun enemies
            EnemyBase enemy = hit.GetComponent<EnemyBase>();
            if (enemy != null)
                enemy.Stun(quackStunDuration);

            // Knockback all rigidbodies in range
            Rigidbody2D hitRb = hit.GetComponent<Rigidbody2D>();
            if (hitRb != null)
            {
                Vector2 dir = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
                hitRb.AddForce(dir * 500f);
            }

            // Break destructible props in range
            DestructibleProp prop = hit.GetComponent<DestructibleProp>();
            prop?.TakeDamage(50f);
        }

        CameraFollow.ShakeCamera(0.3f);
        JuiceManager.Instance?.TriggerSlowMo();
    }

    private void CreateQuackShockwave()
    {
        var ring = new GameObject("QuackRing");
        ring.transform.position = transform.position;

        var sr = ring.AddComponent<SpriteRenderer>();
        sr.color = new Color(1f, 1f, 0.3f, 0.7f);
        sr.sortingOrder = 10;

        var fx = ring.AddComponent<ExplosionEffect>();
        fx.Initialize(quackStunRadius * 0.8f, new Color(1f, 1f, 0.3f, 0.7f));
    }

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

    /// <summary>Apply a temporary speed multiplier (from powerup).</summary>
    public void ApplySpeedBoost(float multiplier, float duration)
    {
        StartCoroutine(SpeedBoostCoroutine(multiplier, duration));
    }

    private IEnumerator SpeedBoostCoroutine(float multiplier, float duration)
    {
        speedMultiplier = multiplier;
        yield return new WaitForSeconds(duration);
        speedMultiplier = 1f;
    }

    // ── Wall-cling ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Detects when the duck is airborne and pressing against a vertical wall surface.
    /// Applies a squash-and-tilt visual so the mechanic looks intentional rather than buggy.
    /// </summary>
    private void UpdateWallCling()
    {
        if (isGrounded || isDashing)
        {
            if (isWallClinging) EndWallCling();
            return;
        }

        bool pressingRight = horizontalInput > 0.1f;
        bool pressingLeft  = horizontalInput < -0.1f;

        bool wallR = pressingRight && CheckWallInDirection(1f);
        bool wallL = pressingLeft  && CheckWallInDirection(-1f);
        bool nowClinging = wallR || wallL;

        if (nowClinging && !isWallClinging)
        {
            isWallClinging = true;
            wallClingDir   = wallR ? 1 : -1;

            // Squash the duck flat against the wall — wider & slightly shorter
            transform.localScale = new Vector3(
                baseScale.x * 0.65f,
                baseScale.y * 1.1f,
                baseScale.z
            );

            // Tilt slightly toward the wall
            float tilt = wallClingDir * -8f; // degrees
            transform.rotation = Quaternion.Euler(0f, 0f, tilt);

            ParticleManager.SpawnLandingDust(transform.position + Vector3.up * 0.3f);
        }
        else if (!nowClinging && isWallClinging)
        {
            EndWallCling();
        }
    }

    private void EndWallCling()
    {
        isWallClinging = false;
        transform.rotation = Quaternion.identity;
        // Scale will be recovered by the squash/stretch recover in Update
    }

    /// <summary>Returns true if there is a ground-layer surface directly beside the player.</summary>
    private bool CheckWallInDirection(float dir)
    {
        // Use a thin BoxCast slightly narrower than the player collider
        const float castDist  = 0.22f;
        const float castWidth = 0.08f;
        const float castHeight = 0.55f;

        RaycastHit2D hit = Physics2D.BoxCast(
            (Vector2)transform.position,
            new Vector2(castWidth, castHeight),
            0f,
            new Vector2(dir, 0f),
            castDist,
            groundLayer
        );
        return hit.collider != null;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, quackStunRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, groundPoundRadius);
    }
}

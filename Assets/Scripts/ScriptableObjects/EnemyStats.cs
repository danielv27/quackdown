using UnityEngine;

/// <summary>
/// ScriptableObject holding configurable stats for each enemy type.
/// Create via: Right-click in Project → Create → DuckRevolution → EnemyStats
/// </summary>
[CreateAssetMenu(
    fileName = "NewEnemyStats",
    menuName  = "DuckRevolution/EnemyStats",
    order     = 1)]
public class EnemyStats : ScriptableObject
{
    [Header("Identity")]
    public string enemyName = "Enemy";

    [Header("Health")]
    public float maxHealth = 50f;

    [Header("Movement")]
    public float moveSpeed   = 2.5f;
    public float patrolRange = 5f;     // half-width of patrol area

    [Header("Combat")]
    public float detectionRange  = 8f;
    public float attackRange     = 6f;
    public float attackDamage    = 10f;
    public float attackCooldown  = 1.5f;  // seconds between shots/hits
    public float bulletSpeed     = 8f;

    [Header("Score")]
    public int scoreValue = 50;

    [Header("Death")]
    [Tooltip("Optional funny line shown on death")]
    public string[] deathQuotes = { "NOOOO!", "I'll be back!", "The humans win this round…" };
}

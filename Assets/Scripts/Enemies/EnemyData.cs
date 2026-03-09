using UnityEngine;

/// <summary>
/// ScriptableObject that holds configurable enemy stats.
/// Create instances via Assets > Create > DuckRevolution > Enemy Data.
/// </summary>
[CreateAssetMenu(fileName = "NewEnemyData", menuName = "DuckRevolution/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string enemyName = "Enemy";
    public Sprite sprite;
    public Color spriteColor = Color.white;

    [Header("Stats")]
    public float maxHealth = 100f;
    public float moveSpeed = 3f;
    public float damage = 10f;
    public float attackRange = 5f;
    public float attackCooldown = 1f;

    [Header("AI")]
    public float detectionRange = 10f;
    public float patrolSpeed = 1.5f;

    [Header("Drops")]
    public int scoreValue = 100;

    [Header("Spawn")]
    [Tooltip("Announcement text when this enemy type first appears")]
    public string spawnAnnouncement = "";
}

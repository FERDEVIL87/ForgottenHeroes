using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FireBall : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Kecepatan terbang bola api")]
    [SerializeField] private float speed = 12f;

    [Header("Environment Collision")]
    [Tooltip("Pilih layer untuk lantai/tembok")]
    [SerializeField] private LayerMask obstacleLayer;

    private Rigidbody2D rb;
    private bool piercesObstacles = false; // <-- Variabel penyimpan status tembus

    // Variabel Single Target
    private float impactDamage;
    private GameObject singleImpactVFX;
    private float singleVFXLifetime;

    // Variabel AOE
    private bool isExplosive = false;
    private float explosionRadius = 0f;
    private float explosionDamage = 0f;
    private GameObject explosionVFX;
    private float explosionVFXLifetime;
    private LayerMask enemyLayer;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearVelocity = (Vector2)transform.right * speed;
    }

    // FUNGSI BARU: Menerima instruksi dari Heider apakah boleh tembus tembok
    public void SetObstacleBehavior(bool canPierce)
    {
        piercesObstacles = canPierce;
    }

    public void SetSingleTargetStats(float damage, GameObject vfxPrefab, float vfxLifetime)
    {
        impactDamage = damage;
        singleImpactVFX = vfxPrefab;
        singleVFXLifetime = vfxLifetime;
    }

    public void SetExplosionStats(float radius, float aoeDamage, GameObject vfxPrefab, float vfxLifetime, LayerMask layer)
    {
        isExplosive = true;
        explosionRadius = radius;
        explosionDamage = aoeDamage;
        explosionVFX = vfxPrefab;
        explosionVFXLifetime = vfxLifetime;
        enemyLayer = layer;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (isExplosive) Explode(other.transform.position);
            else SpawnSingleImpact(other.transform.position, other);

            Destroy(gameObject);
        }
        else if (((1 << other.gameObject.layer) & obstacleLayer) != 0)
        {
            // CEK STATUS TEMBUS TEMBOK
            if (piercesObstacles)
            {
                // Jika boleh tembus, abaikan tabrakan dan biarkan bola api terus terbang
                return;
            }

            // Jika tidak boleh tembus, hancur/meledak di tembok
            if (isExplosive)
            {
                Explode(transform.position);
            }
            else
            {
                if (singleImpactVFX != null)
                {
                    GameObject fx = Instantiate(singleImpactVFX, transform.position, Quaternion.identity);
                    Destroy(fx, singleVFXLifetime);
                }
            }

            Destroy(gameObject);
        }
    }

    private void SpawnSingleImpact(Vector2 hitPosition, Collider2D other)
    {
        if (singleImpactVFX != null)
        {
            GameObject fx = Instantiate(singleImpactVFX, hitPosition, Quaternion.identity);
            Destroy(fx, singleVFXLifetime);
        }

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy == null) enemy = other.GetComponentInParent<Enemy>();

        if (enemy != null) enemy.EnemyHit(impactDamage, transform.right, 2f);
    }

    private void Explode(Vector2 hitPosition)
    {
        if (explosionVFX != null)
        {
            GameObject fx = Instantiate(explosionVFX, hitPosition, Quaternion.identity);
            Destroy(fx, explosionVFXLifetime);
        }

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(hitPosition, explosionRadius, enemyLayer);

        foreach (Collider2D musuh in hitEnemies)
        {
            Enemy enemy = musuh.GetComponent<Enemy>();
            if (enemy == null) enemy = musuh.GetComponentInParent<Enemy>();

            if (enemy != null)
            {
                Vector2 pushDirection = (musuh.transform.position - (Vector3)hitPosition).normalized;
                enemy.EnemyHit(explosionDamage, pushDirection, 4f);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (isExplosive)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
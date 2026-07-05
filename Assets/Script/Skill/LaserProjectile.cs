using UnityEngine;

public class LaserProjectile : MonoBehaviour
{
    [Header("Laser Stats")]
    [SerializeField] private float speed = 35f;
    [SerializeField] private float damage = 15f;
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("VFX Prefabs (2 Prefab System)")]
    [SerializeField] private GameObject trailPrefab;
    [SerializeField] private GameObject impactVFXPrefab;

    private Rigidbody2D rb;
    private GameObject spawnedTrail;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        // Mendorong laser lurus ke arah rotasi tembak
        rb.linearVelocity = transform.right * speed;

        if (trailPrefab != null)
        {
            spawnedTrail = Instantiate(trailPrefab, transform.position, transform.rotation);
            spawnedTrail.transform.SetParent(transform);
        }

        Destroy(gameObject, lifetime);
    }

    // === PINDAHKAN ROTASI KE UPDATE ===
    private void Update()
    {
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                Vector2 hitDir = rb.linearVelocity.normalized;
                player.TakeDamage(damage, hitDir, 5f);
            }
            HandleImpact(transform.position);
        }
        else if (((1 << other.gameObject.layer) & obstacleLayer) != 0)
        {
            HandleImpact(transform.position);
        }
    }

    private void HandleImpact(Vector2 pos)
    {
        // Munculkan efek ledakan kecil laser jika ada
        if (impactVFXPrefab != null)
        {
            // === UBAH BAGIAN INI ===
            // Ganti Quaternion.identity menjadi transform.rotation
            // Agar efek ledakannya miring sesuai arah tembakan peluru
            GameObject fx = Instantiate(impactVFXPrefab, pos, transform.rotation);
            Destroy(fx, 1f);
        }

        // === TRIK DETACH TRAIL ===
        if (spawnedTrail != null)
        {
            spawnedTrail.transform.SetParent(null);
            Destroy(spawnedTrail, 1f);
        }

        // Hancurkan peluru utama
        Destroy(gameObject);
    }
}
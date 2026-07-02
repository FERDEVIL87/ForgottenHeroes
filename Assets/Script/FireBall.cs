using UnityEngine;

public class FireBall : MonoBehaviour
{
    [SerializeField] float damage;
    [SerializeField] float hitForce;
    [SerializeField] float speed = 10f; // Diubah jadi float agar lebih fleksibel
    [SerializeField] float lifetime = 1f;

    [Header("Visual Effects")]
    [SerializeField] private GameObject impactEffectPrefab;

    void Start()
    {
        // Bola api akan hancur otomatis dalam waktu 'lifetime' jika tidak kena apa-apa
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        // Menambahkan Time.fixedDeltaTime agar kecepatan stabil di semua komputer
        transform.position += transform.right * speed * Time.fixedDeltaTime;
    }

    private void OnTriggerEnter2D(Collider2D _other)
    {
        // 1. Cegah bola api meledak jika mengenai badan Player sendiri
        if (_other.CompareTag("Player")) return;

        // 2. Jika menabrak musuh, berikan damage
        if (_other.CompareTag("Enemy"))
        {
            Enemy e = _other.GetComponent<Enemy>();
            if (e != null)
            {
                // Catatan: Aku hilangkan tanda minus (-) pada hitForce 
                // karena di perbaikan Enemy.cs sebelumnya kita sudah membuat sistem pentalannya menjadi positif
                Vector2 hitDir = (_other.transform.position - transform.position).normalized;
                e.EnemyHit(damage, hitDir, hitForce);
            }
        }

        // 3. Munculkan efek animasi ledakan di posisi bola api ini
        if (impactEffectPrefab != null)
        {
            // UBAH Quaternion.identity MENJADI transform.rotation
            // Agar ledakannya menoleh ke arah yang sama dengan Fireball (kiri/kanan)
            Instantiate(impactEffectPrefab, transform.position, transform.rotation);
        }

        // 4. Hancurkan bola api (Kode ini ditaruh di luar pengecekan Enemy agar 
        // bola api hancur saat menabrak APAPUN selain Player, termasuk tembok)
        Destroy(gameObject);
    }
}
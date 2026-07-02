using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] protected float maxHealth = 60f;
    protected float currentHealth;

    [Header("Rewards")]
    [SerializeField] protected int manaReward = 15;

    [Header("Death Settings")]
    [SerializeField] protected float disappearDelay = 2.5f; // Jeda sebelum mayat hilang

    [Header("Knockback Settings")]
    [SerializeField] protected float knockbackLength = 0.2f;

    [Header("Stun Settings")]
    public bool isStunned = false;

    private Coroutine stunCoroutine;

    // UBAH BARIS INI: Tambahkan [SerializeField] agar muncul di Inspector
    [SerializeField] protected float stunDuration = 1f;

    private float stunTimer;

    // UBAH: Menggunakan protected agar bisa dibaca oleh EnemyAIBase.cs
    // Dan mengubah isKnockback menjadi isRecoiling menyesuaikan kodemu sebelumnya
    protected bool isRecoiling;
    protected float knockbackTimer;
    private Coroutine recoilCoroutine;

    // UBAH: Dari private menjadi protected
    protected Rigidbody2D rb;
    protected Animator anim;
    protected Collider2D col;

    protected bool isDead = false;

    [HideInInspector] public bool isChasing = false;

    // UBAH: Tambahkan "protected virtual" agar EnemyAIBase bisa melakukan "override"
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();

        currentHealth = maxHealth;
    }

    protected virtual void Update()
    {
        if (isDead) return;

        HandleAnimations();
    }

    protected virtual void FixedUpdate()
    {
        if (isDead) return;

        if (isStunned) return;


    }

    protected virtual void HandleAnimations()
    {
        if (anim != null)
        {
            // 🌟 TAMBAHKAN '|| isStunned' DI SINI
            if (isRecoiling || isStunned)
            {
                anim.SetBool("isRunning", false);
                anim.SetBool("isJumping", false);
                return;
            }

            anim.SetBool("isRunning", isChasing);
            bool isJumping = Mathf.Abs(rb.linearVelocity.y) > 0.1f;
            anim.SetBool("isJumping", isJumping);
        }
    }

    // FUNGSI SAAT TERKENA HIT OLEH PLAYER
    // FUNGSI SAAT TERKENA HIT OLEH PLAYER
    public virtual void EnemyHit(float _damage, Vector2 _hitDirection, float _recoilStrength)
    {
        if (isDead) return;

        // =========================================================
        // 🌟 BARU: Deteksi Shockwave Parry (Jika Damage yang masuk 0)
        // =========================================================
        if (_damage == 0f)
        {
            Debug.Log(gameObject.name + " Terkena Shockwave Parry! Masuk mode Stun.");

            // Aktifkan status Stun selama 1 detik menggunakan Coroutine
            StartCoroutine(StunRoutine());

            // Berikan efek dorongan/knockback khusus shockwave
            if (rb != null)
            {
                rb.linearVelocity = _hitDirection * _recoilStrength;
            }

            return; // Hentikan pembacaan ke bawah agar musuh tidak memutar animasi Hurt biasa
        }

        // =========================================================
        // 💥 KODE ASLI: Logika saat terkena serangan biasa (Damage > 0)
        // =========================================================
        currentHealth -= _damage;
        Debug.Log(gameObject.name + " Terkena Hit! Sisa Darah: " + currentHealth);

        // === TAMBAHAN PENTING: Cek apakah musuh mati setelah kena damage ===
        if (currentHealth <= 0)
        {
            StartCoroutine(DieRoutine());
            return; // Hentikan kode agar tidak kena efek terpental saat mati
        }
        // ===================================================================

        // Hentikan coroutine knockback/stun yang sedang berjalan sebelumnya
        if (recoilCoroutine != null) StopCoroutine(recoilCoroutine);
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);

        // === PERBAIKAN ERROR ===
        // Ganti _hitForce menjadi _recoilStrength menyesuaikan parameter di atas
        recoilCoroutine = StartCoroutine(HitRoutine(_hitDirection, _recoilStrength));
    }

    public void Stun()
    {
        if (isDead) return;

        // Hentikan stun sebelumnya jika ada
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);

        stunCoroutine = StartCoroutine(StunRoutine());
    }

    protected virtual IEnumerator HitRoutine(Vector2 _hitDirection, float _hitForce)
    {
        isRecoiling = true;

        if (rb != null)
        {
            // === PERBAIKAN UTAMA: Jangan setel kecepatan vertikal ke 0 secara mutlak! ===
            // Biarkan sumbu Y (rb.linearVelocity.y) tetap berjalan normal agar musuh tetap jatuh/terpengaruh gravitasi.
            rb.linearVelocity = new Vector2(_hitDirection.x * _hitForce, rb.linearVelocity.y);
        }

        yield return new WaitForSecondsRealtime(knockbackLength);

        if (rb != null)
        {
            // Berhenti meluncur secara horizontal SAJA.
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        isRecoiling = false;
        recoilCoroutine = null;
    }

    protected virtual IEnumerator HitRecoilRoutine(Vector2 hitDirection, float recoilStrength)
    {
        isRecoiling = true;

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(hitDirection.x * recoilStrength, hitDirection.y * (recoilStrength * 0.3f));
        }

        // 🌟 BARU: Wajib pakai Realtime agar tidak nyangkut saat game sedang Hit-Stop!
        yield return new WaitForSecondsRealtime(knockbackLength);

        if (rb != null)
        {
            // Berhenti meluncur
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        isRecoiling = false;
        recoilCoroutine = null; // Kosongkan penyimpan
    }

    protected virtual IEnumerator StunRoutine()
    {
        isStunned = true;
        if (anim != null) anim.SetBool("isStunned", true);

        yield return new WaitForSecondsRealtime(0.2f);

        // Rem total kecepatan meluncurnya agar berhenti di tempat, TETAPI jaga kecepatan jatuhnya!
        if (rb != null) rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        yield return new WaitForSecondsRealtime(0.8f); // Sisa waktu stun

        // Waktu habis, kembalikan ke normal
        isStunned = false;
        if (anim != null) anim.SetBool("isStunned", false);
        stunCoroutine = null;
    }

    protected virtual IEnumerator DieRoutine()
    {
        isDead = true;

        // === GAIN MANA REWARD ===
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.GainEnergy(manaReward);
        }

        // === FIX TENGGELAM DI SINI ===
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic; // Mengunci fisik musuh agar tidak terpengaruh gravitasi saat mati
        }

        if (col != null) col.enabled = false; // Matikan collider agar player bisa lewat bebas tanpa menabrak mayat

        // Putar animasi mati
        if (anim != null) anim.SetTrigger("Die");

        // Tunggu beberapa detik sebelum mayat hilang
        yield return new WaitForSeconds(disappearDelay);

        // Hancurkan objek musuh
        Destroy(gameObject);
    }
}
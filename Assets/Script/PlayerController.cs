using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // === SINGLETON ===
    public static PlayerController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 7f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private float groundCheckX = 0.4f;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private LayerMask wallLayer;

    [Header("Jump Buffering")]
    [SerializeField] private float jumpBufferTime = 0.13f;
    private float jumpBufferCounter;

    [Header("Coyote Time")]
    [SerializeField] private float coyoteTime = 0.15f;
    private float coyoteTimeCounter;

    [Header("Double Jump")]
    [SerializeField] private int maxJumps = 2;
    private int jumpsRemaining;

    public enum DashMode { Normal, EaseOut, Blink }

    [Header("Dash Settings")]
    public DashMode currentDashMode = DashMode.Normal;
    [SerializeField] private float dashPower = 20f;
    [SerializeField] private float dashTime = 0.15f;
    [SerializeField] private float dashCooldown = 2f;

    [Header("Blink Settings")]
    [SerializeField] private float blinkDistance = 6f;
    [SerializeField] private LayerMask blinkObstacleLayer;

    private bool canDash = true;
    private bool isDashing;

    [Header("Attack Settings")]
    [SerializeField] private Transform sideAttackTransform;
    [SerializeField] private Vector2 sideAttackArea;
    [SerializeField] private LayerMask attackableLayer;
    [SerializeField] private float damage = 20f;
    [SerializeField] private float attackDuration = 0.25f;
    private bool isAttacking;

    [Header("Player Recoil Settings (When Attacking)")]
    [SerializeField] private float recoilXForce = 5f;
    [SerializeField] private float recoilXLength = 0.1f;
    private bool recoilX;
    private float recoilXTimer;

    [Header("Block & Parry Settings")]
    public bool isBlocking = false;
    public bool isParrying = false;
    private bool isBlockHitLocked = false;

    [SerializeField] private float parryTimeWindow = 0.2f; // Waktu (detik) untuk parry sukses
    private float parryTimer;

    private bool isBlockLocked = false; // Pengunci agar animasi block tidak memotong parry

    [Header("Parry Rewards")]
    [SerializeField] private float parrySlowMoDuration = 0.15f;
    [SerializeField] private float parrySlowMoFactor = 0.2f;

    // ==============================================================
    // 🌟 BARU: VARIABEL SHOCKWAVE & CAMERA SHAKE PARRY
    // ==============================================================
    [SerializeField] private float parryShockwaveRadius = 4f;    // Seberapa jauh jangkauan ledakan shockwave
    [SerializeField] private float parryKnockbackForce = 25f;    // Kekuatan musuh terpental (bikin tinggi agar terasa mantap)
    [SerializeField] private GameObject parryShockwavePrefab;   // Tempat menaruh prefab visual lingkaran shockwave (VFX)

    [Space(5)]
    [SerializeField] private float parryShakeDuration = 0.25f;   // Durasi kamera bergetar saat parry sukses
    [SerializeField] private float parryShakeMagnitude = 0.3f;   // Kekuatan getaran (bikin lebih besar dari kena hit biasa)

    public bool hasCriticalBuff { get; private set; } = false;

    // ================= HEALTH SYSTEM =================
    [Header("Player Health & Damage Settings")]
    public int health;
    public int maxHealth;
    public System.Action onHealthChangedCallback;

    public int Health
    {
        get { return health; }
        set
        {
            if (health != value)
            {
                health = Mathf.Clamp(value, 0, maxHealth);
                if (onHealthChangedCallback != null)
                {
                    onHealthChangedCallback.Invoke();
                }
            }
        }
    }

    //[SerializeField] private float damageRecoilForce = 10f;
    [SerializeField] private float damageRecoilLength = 0.25f;
    private bool isDamageRecoiling;
    private float damageRecoilTimer;

    [Header("Hit Stop & I-Frame Settings")]
    [SerializeField] private float hitStopScale = 0.2f;
    [SerializeField] private float hitStopDuration = 0.15f;
    [SerializeField] private float invincibilityDuration = 1.5f;
    [SerializeField] private float flashInterval = 0.1f;
    private bool isInvincible = false;

    // === TAMBAHAN BARU: Parameter Kamera Getar ===
    [Header("Camera Shake Settings")]
    [SerializeField] private float damageShakeDuration = 0.2f;   // Durasi getar (detik)
    [SerializeField] private float damageShakeMagnitude = 0.15f; // Kekuatan getaran

    // ================= ENERGI / MANA SYSTEM =================
    [Header("Mana / Energy Settings")]
    public int currentEnergy = 0;
    public int maxEnergy = 100;
    public int healCost = 100;
    public System.Action onManaChangedCallback;

    [Header("Stamina System")]
    public float maxStamina = 100f;
    public float currentStamina;
    [SerializeField] private float staminaRegenRate = 15f; // Kecepatan regen per detik
    [SerializeField] private float staminaRegenDelay = 1f; // Jeda waktu sebelum regen mulai setelah pakai stamina
    private float staminaRegenTimer;

    [Header("Stamina Costs")]
    [SerializeField] private float jumpCost = 10f;
    [SerializeField] private float dashCost = 15f;
    [SerializeField] private float attackCost = 10f;
    [SerializeField] private float skillCost = 20f;
    [SerializeField] private float blockHitCost = 15f; // Stamina ngurang saat block kena hit

    // Event callback untuk mengabari UI agar update
    public System.Action onStaminaChangedCallback;

    [Header("Visual Effects (VFX)")]
    [SerializeField] private GameObject bloodSpurt;
    [SerializeField] private GameObject dashEffectPrefab;
    [SerializeField] private Vector3 dashEffectOffset;
    [SerializeField] private float effectDestroyTime = 1f;

    [Space(10)]
    [SerializeField] private GameObject slashEffectPrefab;
    [SerializeField] private Vector3 slashEffectOffset;
    [SerializeField] private float slashEffectDelay = 0.1f;
    [SerializeField] private float slashDestroyTime = 0.3f;

    [Header("Skill Settings")]
    [SerializeField] private int fireBallCost = 33;
    [SerializeField] private GameObject fireBallPrefab;
    [SerializeField] private Transform fireBallSpawnPoint;

    [Header("Casting Settings")]
    [SerializeField] private float castDuration = 0.7f;
    private bool isCasting = false;
    private float defaultGravity;
    private Coroutine currentCastCoroutine;

    [Header("Ground Smash Settings")]
    [SerializeField] private int groundSmashCost = 40;
    [SerializeField] private float smashDropSpeed = 30f;
    [SerializeField] private float smashDamage = 40f;
    [SerializeField] private float smashRadius = 2.5f;
    [SerializeField] private GameObject airSlamEffectPrefab;
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private Transform explosionSpawnPoint;
    private bool isSmashing = false;

    [Header("Barrage Skill Settings")]
    [SerializeField] private int barrageCost = 45;
    [SerializeField] private int barrageCount = 6;
    [SerializeField] private float barrageInterval = 0.1f;
    [SerializeField] private float barrageSpreadAngle = 15f;
    [SerializeField] private float barrageSpreadY = 0.3f;

    [Header("Heider Modular Spawning (Revision 8)")]
    // Tentukan jarak menjauh: di belakang dan sedikit di atas player
    [SerializeField] private Vector3 spawnOffsetR = new Vector3(-1.8f, 1.2f, 0f); // Untuk Player hadap Kanan
    [SerializeField] private Vector3 spawnOffsetL = new Vector3(1.8f, 1.2f, 0f); // Untuk Player hadap Kiri

    [Header("Audio & SFX Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip jumpSFX;
    [SerializeField] private AudioClip dashSFX;
    [SerializeField] private AudioClip attackSFX;
    [SerializeField] private AudioClip hurtSFX;
    [SerializeField] private AudioClip deathSFX;

    [Header("Skill SFX Settings")]
    [SerializeField] private AudioClip skillChargeSFX;
    [SerializeField] private AudioClip fireBallSFX;
    [SerializeField] private AudioClip smashDropSFX;
    [SerializeField] private AudioClip smashExplodeSFX;
    [SerializeField] private AudioClip barrageShootSFX;

    private float xAxis;

    void Start()
    {
        // === TAMBAHAN SISTEM CHECKPOINT ===
        // Cek apakah ada data save ("HasSaveData") yang aktif
        // Ini akan bernilai True jika kita klik Continue, atau Restart setelah mati.
        // Ini bernilai False jika kita klik "Play/New Game" dari Main Menu.
        if (PlayerPrefs.HasKey("HasSaveData"))
        {
            // Ambil koordinat terakhir
            float savedX = PlayerPrefs.GetFloat("PlayerPosX");
            float savedY = PlayerPrefs.GetFloat("PlayerPosY");

            // Pindahkan player ke koordinat tersebut
            transform.position = new Vector3(savedX, savedY, transform.position.z);
        }

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        Health = maxHealth;
        currentStamina = maxStamina;
        defaultGravity = rb.gravityScale;
    }

    void Update()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
        HandleStaminaRegen();
        if (isCasting) return;

        if (xAxis != 0 && !isDashing && !recoilX && !isDamageRecoiling)
        {
            transform.localScale = new Vector3(Mathf.Sign(xAxis), 1, 1);
        }

        if (Input.GetButtonDown("Attack") && !isAttacking)
        {
            StartCoroutine(Attack());
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && !isAttacking)
        {
            StartCoroutine(Dash());
        }

        // ==========================================
        // SISTEM INPUT SKILL (Kombinasi Arah + Skill)
        // ==========================================
        // BARU: Ditambahkan '&& !isBlocking' agar semua skill terkunci saat menangkis
        if (Input.GetButtonDown("Skill") && !isBlocking)
        {
            // 1. ATAS + SKILL : Coba Panggil Support / Kameo
            if (Input.GetAxisRaw("Vertical") > 0.5f)
            {
                TryCallSupport();
            }
            // 2. BAWAH + SKILL
            else if (Input.GetAxisRaw("Vertical") < -0.5f)
            {
                if (!Grounded()) TryGroundSmash();
                else TryFireballBarrage();
            }
            // 3. MAJU/SAMPING + SKILL
            else if (xAxis != 0)
            {
                TryCastFireBall();
            }
            // 4. NETRAL + SKILL
            else
            {
                if (Grounded()) TryHeal();
            }
        }
        // ==========================================
        // ==========================================

        if (Grounded())
        {
            coyoteTimeCounter = coyoteTime;
            jumpsRemaining = maxJumps;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
            if (coyoteTimeCounter <= 0f && jumpsRemaining == maxJumps)
            {
                jumpsRemaining--;
            }
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (!isAttacking || !Grounded())
        {
            Jump();
        }

        HandleBlockAndParry();

        UpdateAnimations();
    }

    void TryCallSupport()
    {
        Debug.Log("Tombol Atas + Skill Ditekan!");

        if (SupportManager.Instance != null && SupportManager.Instance.equippedSkill != null)
        {
            Debug.Log("Support Manager terbaca, bersiap memanggil Kameo!");

            // 1. Tentukan Arah Hadap (0=Right, 1=Left)
            int facingInt = (int)Mathf.Sign(transform.localScale.x);
            int faceDirParam = (facingInt == 1) ? 0 : 1; // Konversi ke parameter integer (0/1)

            // 2. Hitung Titik Spawn Modular (Berdasarkan Arah)
            Vector3 finalSpawnPos = transform.position;
            if (faceDirParam == 1) // Player faces Left
            {
                finalSpawnPos += spawnOffsetL;
            }
            else // Player faces Right
            {
                finalSpawnPos += spawnOffsetR;
            }

            // 3. Panggil Manager dengan Data Modular
            // Kami perlu sedikit memperbarui CallSupport() untuk menerima Vector3 posisi.
            // UNTUK SAAT INI (Uji Coba), kita gunakan fungsi ini dan beritahu apa yang harus diubah:
            SupportManager.Instance.CallSupportWithPosition(finalSpawnPos, faceDirParam);
        }
    }

    private void FixedUpdate()
    {
        if (isDamageRecoiling)
        {
            if (damageRecoilTimer < damageRecoilLength)
            {
                damageRecoilTimer += Time.fixedDeltaTime;
                return;
            }
            isDamageRecoiling = false;
            damageRecoilTimer = 0;
        }

        if (recoilX)
        {
            if (recoilXTimer < recoilXLength)
            {
                recoilXTimer += Time.fixedDeltaTime;
                rb.linearVelocity = new Vector2(-transform.localScale.x * recoilXForce, rb.linearVelocity.y);
                return;
            }
            recoilX = false;
            recoilXTimer = 0;
        }

        if (isDashing) return;

        if (isCasting)
        {
            if (!isSmashing) rb.linearVelocity = Vector2.zero;
            return;
        }

        // 🛠️ UBAH DI SINI: Tambahkan || isBlocking
        if ((isAttacking || isBlocking) && Grounded())
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y); // Menghentikan jalan saat menyerang ATAU block
        }
        else
        {
            rb.linearVelocity = new Vector2(xAxis * walkSpeed, rb.linearVelocity.y);
        }
    }

    void UpdateAnimations()
    {
        // 🛠️ UBAH DI SINI: Tambahkan && !isBlocking
        anim.SetBool("Walking", xAxis != 0 && !isBlocking);
        anim.SetBool("Jumping", !Grounded());
        anim.SetBool("Dashing", isDashing);
    }

    void Jump()
    {
        // === FIX LOMPAT DI SINI ===
        // Suara hanya akan diputar TEPAT saat energi lompat dikeluarkan, bukan pas lagi melayang!
        if (jumpBufferCounter > 0f && (coyoteTimeCounter > 0f || jumpsRemaining > 0))
        {
            // === TAMBAHAN BARU: Cek & Kurangi Stamina sebelum lompat ===
            if (HasEnoughStamina(jumpCost))
            {
                UseStamina(jumpCost); // Potong stamina

                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpsRemaining--;
                jumpBufferCounter = 0f;
                coyoteTimeCounter = 0f;

                // PUTAR SUARA LOMPAT DI DALAM BLOK INI!
                PlaySFX(jumpSFX);
            }
        }

        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }
    }

    private IEnumerator Attack()
    {
        // ✅ TAMBAHKAN CEK STAMINA
        if (!HasEnoughStamina(attackCost))
        {
            yield break; // Batal serang
        }
        UseStamina(attackCost); // Kurangi stamina
        isAttacking = true;
        anim.SetTrigger("Attacking");
        PlaySFX(attackSFX);

        if (slashEffectPrefab != null)
        {
            StartCoroutine(SpawnSlashWithDelay());
        }

        Hit(sideAttackTransform, sideAttackArea, ref recoilX, recoilXForce);

        yield return new WaitForSeconds(attackDuration);
        isAttacking = false;
    }

    void Hit(Transform _attackTransform, Vector2 _attackArea, ref bool _recoilDir, float _recoilStrength)
    {
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(_attackTransform.position, _attackArea, 0, attackableLayer);
        List<Enemy> hitEnemies = new List<Enemy>();

        if (objectsToHit.Length > 0) _recoilDir = true;

        // ==============================================================
        // 🔥 TAMBAHAN BARU: HITUNG DAMAGE CRITICAL
        // ==============================================================
        float finalDamage = damage; // Ambil nilai damage dasar (20)

        if (hasCriticalBuff)
        {
            finalDamage *= 2f; // Kalikan damage menjadi 2x lipat (40)
            hasCriticalBuff = false; // LANGSUNG RESET agar pukulan berikutnya normal lagi
            Debug.Log("CRITICAL COUNTER ATTACK! Damage dikeluarkan: " + finalDamage);
        }
        // ==============================================================

        for (int i = 0; i < objectsToHit.Length; i++)
        {
            Enemy e = objectsToHit[i].GetComponent<Enemy>();
            if (e != null && !hitEnemies.Contains(e))
            {
                Vector2 hitDirection = (objectsToHit[i].transform.position - transform.position).normalized;

                // 🟥 GANTI parameter pertama dari 'damage' menjadi 'finalDamage'
                e.EnemyHit(finalDamage, hitDirection, _recoilStrength);

                hitEnemies.Add(e);
            }
        }
    }

    private IEnumerator ParrySlowMotionRoutine(float duration, float slowFactor)
    {
        Time.timeScale = slowFactor; // Membuat game menjadi lambat
        Time.fixedDeltaTime = 0.02f * Time.timeScale; // Mengunci perhitungan fisika/physics agar tidak patah-patah

        // Menggunakan WaitForSecondsRealtime agar hitungan detiknya akurat menggunakan jam dunia nyata, 
        // karena kalau menggunakan WaitForSeconds biasa, delay-nya akan ikut melambat akibat timeScale.
        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1f; // Kembalikan waktu game menjadi normal
        Time.fixedDeltaTime = 0.02f; // Kembalikan kestabilan physics menjadi normal
    }

    public void TakeDamage(float _damage, Vector2 _damageDir, float _recoilForce)
    {
        if (isInvincible) return;

        // ======================================================================
        // 🛡️ TAMBAHKAN LOGIKA BLOCK & PARRY DI SINI (SETELAH ISINVINCIBLE)
        // ======================================================================

        // 1. Cek jika berhasil melakukan Parry
        if (isParrying)
        {
            Debug.Log("PARRY BERHASIL! Slow-mo, Critical, Getar Kamera & Shockwave Aktif. (Tidak menguras Stamina)");

            anim.SetTrigger("ParrySuccess");
            PlaySFX(hurtSFX);

            // A. Berikan Buff & Efek Slow Motion
            hasCriticalBuff = true;
            StartCoroutine(ParrySlowMotionRoutine(parrySlowMoDuration, parrySlowMoFactor));

            // B. Efek Kamera Getar Khusus Parry
            if (CameraFollow.Instance != null)
            {
                CameraFollow.Instance.Shake(parryShakeDuration, parryShakeMagnitude);
            }

            // C. Spawn Visual Efek Shockwave (Jika kamu punya Prefab VFX-nya)
            if (parryShockwavePrefab != null)
            {
                GameObject wave = Instantiate(parryShockwavePrefab, transform.position, Quaternion.identity);
                Destroy(wave, 0.5f); // Langsung hancurkan setelah setengah detik
            }

            // D. Logika Shockwave Physics (Mementalkan Musuh di Sekitar)
            Collider2D[] surroundingObjects = Physics2D.OverlapCircleAll(transform.position, parryShockwaveRadius);
            foreach (Collider2D obj in surroundingObjects)
            {
                Enemy e = obj.GetComponent<Enemy>();
                if (e != null)
                {
                    // Hitung arah dorongan (dari posisi Player menuju posisi Musuh)
                    Vector2 knockbackDirection = (obj.transform.position - transform.position).normalized;
                    // Berikan sedikit dorongan ke atas (sumbu Y) agar musuhnya agak terpental melayang
                    knockbackDirection.y = 0.4f;
                    // Pukul musuh dengan DAMAGE 0 tetapi dengan KNOCKBACK TINGGI (parryKnockbackForce)
                    e.EnemyHit(0f, knockbackDirection.normalized, parryKnockbackForce);
                }
            }

            // JALANKAN PENGUNCI ANIMASI DI SINI
            StartCoroutine(ParryAnimationLockRoutine(0.5f));
            return; // Hentikan eksekusi, player terhindar dari damage
        }

        // 2. Cek jika Player sedang menahan serangan (Block biasa)
        if (isBlocking)
        {
            Debug.Log("Serangan ditahan! Stamina dikurangi.");

            // === 🟢 TAMBAHAN BARU: SISTEM STAMINA PADA BLOCK ===
            UseStamina(blockHitCost);

            // GUARD BREAK: Jika stamina habis akibat menahan serangan
            if (currentStamina <= 0)
            {
                isBlocking = false; // Buka paksa perisai pelindung
                anim.SetBool("isBlocking", false);
                Debug.Log("Guard Break! Stamina habis.");
                // Jika kamu punya animasi GuardBreak, kamu bisa panggil: anim.SetTrigger("GuardBreak");
            }
            // ====================================================

            // Cukup panggil trigger ini, tidak perlu mematikan isBlocking lewat kode lagi!
            anim.SetTrigger("BlockHit");

            float reducedDamage = _damage * 0.2f;
            Health -= Mathf.RoundToInt(reducedDamage);

            if (CameraFollow.Instance != null)
            {
                CameraFollow.Instance.Shake(damageShakeDuration * 0.5f, damageShakeMagnitude * 0.5f);
            }

            isDamageRecoiling = true;
            damageRecoilTimer = 0f;
            rb.linearVelocity = new Vector2(_damageDir.x * (_recoilForce * 0.3f), _recoilForce * 0.1f);

            if (Health <= 0) PlayerDie();

            return;
        }

        // ======================================================================
        // 💥 KODE ASLI/LAMA MILIKMU (JANGAN DIUBAH, BIARKAN DI BAWAH)
        // ======================================================================
        CancelCasting(); // Menghentikan aktivitas magic/skill kalau player kena damage

        Health -= Mathf.RoundToInt(_damage);

        anim.SetTrigger("TakeDamage");
        PlaySFX(hurtSFX);

        if (CameraFollow.Instance != null)
        {
            CameraFollow.Instance.Shake(damageShakeDuration, damageShakeMagnitude);
        }

        if (bloodSpurt != null)
        {
            GameObject blood = Instantiate(bloodSpurt, transform.position, Quaternion.identity);
            Destroy(blood, 1.5f);
        }

        StartCoroutine(HitStopRoutine());
        StartCoroutine(InvincibilityRoutine());
        StartCoroutine(FlashRoutine());

        isDamageRecoiling = true;
        damageRecoilTimer = 0f;
        rb.linearVelocity = new Vector2(_damageDir.x * _recoilForce, _recoilForce * 0.4f);

        if (Health <= 0) PlayerDie();
    }

    private IEnumerator ParryAnimationLockRoutine(float duration)
    {
        isBlockLocked = true;   // Aktifkan pengunci input
        isBlocking = false;     // Matikan status block di script
        isParrying = false;     // Matikan status parry
        anim.SetBool("isBlocking", false); // Paksa animasi Block mati di Animator

        // Tunggu sampai animasi parry selesai diputar sepenuhnya
        yield return new WaitForSeconds(duration);

        isBlockLocked = false;  // Buka kembali pengunci
    }

    private IEnumerator BlockHitAnimationLockRoutine(float duration)
    {
        isBlockHitLocked = true;           // Aktifkan pengunci hit
        isBlocking = false;                // Matikan status block di script sementara
        anim.SetBool("isBlocking", false); // Matikan paksa animasi loop menahan di Animator

        // Berikan waktu agar animasi BlockHit selesai berputar (misal 0.25 detik)
        yield return new WaitForSeconds(duration);

        isBlockHitLocked = false;          // Buka kembali kunci hit
    }

    void PlayerDie() {
        Debug.Log("Player telah mati!");

        // 1. Putar suara kematian player
        PlaySFX(deathSFX);

        // 2. Jalankan Coroutine untuk menangani jeda animasi mati
        StartCoroutine(DieRoutine());
    }

    private IEnumerator DieRoutine()
    {
        // 1. Hentikan total pergerakan fisik player agar tidak merosot/meluncur
        rb.linearVelocity = Vector2.zero;

        // Opsional: Jika player sering menembus lantai saat mati, matikan rotasi atau ubah body type ke Kinematic
        // rb.bodyType = RigidbodyType2D.Kinematic;

        // 2. Matikan script PlayerController agar pemain tidak bisa menekan tombol bergerak/menyerang lagi
        this.enabled = false;

        // 3. Mainkan animasi mati (Pastikan di Animator-mu ada trigger bernama "Die")
        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        // 4. JEDA TIMING (Tunggu sampai animasi selesai)
        // GANTI angka 2.0f di bawah ini sesuai dengan durasi detik animasi matimu di Unity (misal: 1.5f atau 2.0f)
        yield return new WaitForSeconds(2.0f);

        // 5. Setelah animasi selesai, BARU PANGGIL PANEL GAME OVER
        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.ShowGameOver();
        }
    }

    public void GainEnergy(int amount)
    {
        currentEnergy = Mathf.Clamp(currentEnergy + amount, 0, maxEnergy);
        if (onManaChangedCallback != null) onManaChangedCallback.Invoke();
    }

    private void TryHeal()
    {
        if (currentEnergy >= healCost && Health < maxHealth)
        {
            currentEnergy -= healCost;
            Health += 1;
            anim.SetTrigger("Healing");
            if (onManaChangedCallback != null) onManaChangedCallback.Invoke();
        }
    }

    private IEnumerator SpawnSlashWithDelay()
    {
        yield return new WaitForSeconds(slashEffectDelay);
        Vector3 spawnPosition = transform.position + new Vector3(slashEffectOffset.x * transform.localScale.x, slashEffectOffset.y, slashEffectOffset.z);
        GameObject currentSlash = Instantiate(slashEffectPrefab, spawnPosition, Quaternion.identity);
        currentSlash.transform.localScale = new Vector3(transform.localScale.x, 1, 1);
        Destroy(currentSlash, slashDestroyTime);
    }

    private IEnumerator Dash()
    {
        // ✅ TAMBAHKAN CEK STAMINA
        if (!HasEnoughStamina(dashCost))
        {
            yield break; // Batal dash
        }
        UseStamina(dashCost); // Kurangi stamina
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        // SUARA DASH
        PlaySFX(dashSFX);

        if (dashEffectPrefab != null)
        {
            Vector3 spawnPosition = transform.position + new Vector3(dashEffectOffset.x * transform.localScale.x, dashEffectOffset.y, dashEffectOffset.z);
            GameObject currentEffect = Instantiate(dashEffectPrefab, spawnPosition, Quaternion.identity);
            currentEffect.transform.localScale = new Vector3(transform.localScale.x, 1, 1);
            Destroy(currentEffect, effectDestroyTime);
        }

        float dir = transform.localScale.x;

        switch (currentDashMode)
        {
            case DashMode.Normal:
                rb.linearVelocity = new Vector2(dir * dashPower, 0f);
                yield return new WaitForSeconds(dashTime);
                break;
            case DashMode.EaseOut:
                float elapsedTime = 0f;
                while (elapsedTime < dashTime)
                {
                    float currentSpeed = Mathf.Lerp(dashPower, 0f, elapsedTime / dashTime);
                    rb.linearVelocity = new Vector2(dir * currentSpeed, 0f);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                break;
            case DashMode.Blink:
                rb.linearVelocity = Vector2.zero;
                sr.enabled = false;
                yield return new WaitForSeconds(0.1f);

                Vector2 dashDirection = new Vector2(dir, 0);
                RaycastHit2D hit = Physics2D.Raycast(transform.position, dashDirection, blinkDistance, blinkObstacleLayer);

                if (hit.collider != null) transform.position = hit.point - (dashDirection * 0.5f);
                else transform.position = (Vector2)transform.position + (dashDirection * blinkDistance);

                sr.enabled = true;
                yield return new WaitForSeconds(0.1f);
                break;
        }

        rb.gravityScale = originalGravity;
        rb.linearVelocity = Vector2.zero;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public bool Grounded()
    {
        LayerMask walkableLayers = whatIsGround | wallLayer;
        return Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckY, walkableLayers) ||
               Physics2D.Raycast(groundCheckPoint.position + new Vector3(-groundCheckX, 0, 0), Vector2.down, groundCheckY, walkableLayers) ||
               Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, walkableLayers);
    }

    private void OnDrawGizmosSelected()
    {
        // 1. Gambar kotak jangkauan Attack biasa (Warna Merah)
        if (sideAttackTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(sideAttackTransform.position, sideAttackArea);
        }

        // 2. Gambar lingkaran jangkauan Shockwave Parry (Warna Biru)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, parryShockwaveRadius);
    }

    private IEnumerator HitStopRoutine()
    {
        Time.timeScale = hitStopScale;
        yield return new WaitForSecondsRealtime(hitStopDuration);
        Time.timeScale = 1f;
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    private IEnumerator FlashRoutine()
    {
        Color originalColor = sr.color;
        Color flashColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0.3f);
        float elapsed = 0f;
        while (elapsed < invincibilityDuration)
        {
            sr.color = flashColor;
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
            sr.color = originalColor;
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }
        sr.color = originalColor;
    }

    private void TryCastFireBall()
    {
        if (currentEnergy >= fireBallCost && !isCasting)
            currentCastCoroutine = StartCoroutine(CastFireBallRoutine());
    }

    private System.Collections.IEnumerator CastFireBallRoutine()
    {
        // ✅ TAMBAHKAN CEK STAMINA
        if (!HasEnoughStamina(skillCost))
        {
            yield break;
        }
        UseStamina(skillCost);
        isCasting = true;
        currentEnergy -= fireBallCost;
        if (onManaChangedCallback != null) onManaChangedCallback.Invoke();

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        anim.Play("CastSpellF", 0, 0f);

        // === FIX SFX FIREBALL DELAY ===
        // Putar suara SEBELUM WAKTU TUNGGU (curi start) untuk menutup celah hening di file audio
        PlaySFX(fireBallSFX);

        yield return new WaitForSeconds(castDuration);

        GameObject newFireBall = Instantiate(fireBallPrefab, fireBallSpawnPoint.position, fireBallSpawnPoint.rotation);
        if (transform.localScale.x < 0)
        {
            newFireBall.transform.eulerAngles = new Vector3(0, 180, 0);
        }

        rb.gravityScale = defaultGravity;
        isCasting = false;
    }

    private void TryGroundSmash()
    {
        if (currentEnergy >= groundSmashCost && !isCasting)
            currentCastCoroutine = StartCoroutine(GroundSmashRoutine());
    }

    private System.Collections.IEnumerator GroundSmashRoutine()
    {
        // ✅ TAMBAHKAN CEK STAMINA
        if (!HasEnoughStamina(skillCost))
        {
            yield break;
        }
        UseStamina(skillCost);
        isCasting = true;
        isSmashing = true;
        isInvincible = true;
        sr.enabled = false;

        Collider2D playerCol = GetComponent<Collider2D>();
        if (playerCol != null) playerCol.enabled = false;

        currentEnergy -= groundSmashCost;
        if (onManaChangedCallback != null) onManaChangedCallback.Invoke();

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        yield return new WaitForSeconds(0.15f);

        PlaySFX(smashDropSFX);
        rb.linearVelocity = new Vector2(0, -smashDropSpeed);

        GameObject airSlam = null;
        if (airSlamEffectPrefab != null)
        {
            airSlam = Instantiate(airSlamEffectPrefab, transform.position, Quaternion.identity, transform);
            airSlam.transform.localEulerAngles = new Vector3(0, 0, -90f);
        }

        while (!Grounded())
        {
            rb.linearVelocity = new Vector2(0, -smashDropSpeed);
            yield return null;
        }

        if (airSlam != null) Destroy(airSlam);

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        float explosionDuration = 0.5f;

        PlaySFX(smashExplodeSFX);

        if (explosionEffectPrefab != null)
        {
            GameObject explosion = Instantiate(explosionEffectPrefab, explosionSpawnPoint.position, Quaternion.identity);
            Destroy(explosion, explosionDuration);
        }

        Collider2D[] objectsToHit = Physics2D.OverlapCircleAll(groundCheckPoint.position, smashRadius);
        foreach (Collider2D obj in objectsToHit)
        {
            if (obj.CompareTag("Enemy"))
            {
                Enemy e = obj.GetComponent<Enemy>();
                if (e != null)
                {
                    Vector2 hitDir = (obj.transform.position - groundCheckPoint.position).normalized;
                    hitDir.y = 0.8f;
                    e.EnemyHit(smashDamage, hitDir.normalized, 20f);
                }
            }
        }

        yield return new WaitForSeconds(explosionDuration);

        sr.enabled = true;
        isInvincible = false;
        if (playerCol != null) playerCol.enabled = true;
        rb.gravityScale = defaultGravity;
        isSmashing = false;
        isCasting = false;
    }

    private void TryFireballBarrage()
    {
        if (currentEnergy >= barrageCost && !isCasting)
            currentCastCoroutine = StartCoroutine(FireballBarrageRoutine());
    }

    private System.Collections.IEnumerator FireballBarrageRoutine()
    {
        // ✅ TAMBAHKAN CEK STAMINA
        if (!HasEnoughStamina(skillCost))
        {
            yield break;
        }
        UseStamina(skillCost);
        isCasting = true;
        currentEnergy -= barrageCost;
        if (onManaChangedCallback != null) onManaChangedCallback.Invoke();

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        anim.Play("CastBarrage", 0, 0f);

        PlaySFX(skillChargeSFX);

        yield return new WaitForSeconds(0.2f);

        for (int i = 0; i < barrageCount; i++)
        {
            float randomY = Random.Range(-barrageSpreadY, barrageSpreadY);
            Vector3 spawnPos = fireBallSpawnPoint.position + new Vector3(0, randomY, 0);

            // PUTAR SUARA TEPAT SEBELUM SPAWN
            PlaySFX(barrageShootSFX);

            GameObject newFireBall = Instantiate(fireBallPrefab, spawnPos, fireBallSpawnPoint.rotation);
            float randomZ = Random.Range(-barrageSpreadAngle, barrageSpreadAngle);

            if (transform.localScale.x < 0)
                newFireBall.transform.eulerAngles = new Vector3(0, 180, randomZ);
            else
                newFireBall.transform.eulerAngles = new Vector3(0, 0, randomZ);

            yield return new WaitForSeconds(barrageInterval);
        }

        yield return new WaitForSeconds(0.3f);

        rb.gravityScale = defaultGravity;
        isCasting = false;
    }

    private void CancelCasting()
    {
        if (isCasting)
        {
            if (currentCastCoroutine != null)
            {
                StopCoroutine(currentCastCoroutine);
                currentCastCoroutine = null;
            }

            isCasting = false;
            isSmashing = false;
            rb.gravityScale = defaultGravity;
            sr.enabled = true;

            Collider2D playerCol = GetComponent<Collider2D>();
            if (playerCol != null) playerCol.enabled = true;
        }
    }

    private void HandleBlockAndParry()
    {
        if (isBlockLocked) return; // Kunci PARRY yang kemarin tetap dipertahankan

        // 1. KETIKA TOMBOL BLOCK MULAI DITEKAN
        if (Input.GetButtonDown("Block"))
        {
            isBlocking = true;
            isParrying = true;
            parryTimer = parryTimeWindow;
            anim.SetBool("isBlocking", true);
        }

        // 2. KETIKA TOMBOL BLOCK TERUS DITAHAN
        if (Input.GetButton("Block"))
        {
            isBlocking = true;
            if (parryTimer > 0)
            {
                parryTimer -= Time.deltaTime;
            }
            else
            {
                isParrying = false;
            }
        }

        // 3. KETIKA TOMBOL BLOCK DILEPAS
        if (Input.GetButtonUp("Block"))
        {
            isBlocking = false;
            isParrying = false;
            parryTimer = 0;
            anim.SetBool("isBlocking", false);
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // Cek apakah stamina cukup
    public bool HasEnoughStamina(float cost)
    {
        return currentStamina >= cost;
    }

    // Fungsi untuk mengurangi stamina
    public void UseStamina(float cost)
    {
        currentStamina -= cost;
        if (currentStamina < 0) currentStamina = 0;

        staminaRegenTimer = staminaRegenDelay; // Reset jeda regen setiap kali stamina dipakai
        onStaminaChangedCallback?.Invoke(); // Update UI
    }

    // Fungsi ini HARUS dipanggil di dalam Update()
    private void HandleStaminaRegen()
    {
        if (staminaRegenTimer > 0)
        {
            staminaRegenTimer -= Time.deltaTime;
        }
        else if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            if (currentStamina > maxStamina) currentStamina = maxStamina;

            onStaminaChangedCallback?.Invoke(); // Update UI saat regen
        }
    }
}
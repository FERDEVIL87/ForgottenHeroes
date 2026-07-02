using UnityEngine;

public class EnemyAIBase : Enemy
{
    public enum EnemyStates { Idle, Patrol, Chase }

    [Header("AI States")]
    [SerializeField] protected EnemyStates currentState = EnemyStates.Idle;

    [Header("Movement & Patrol")]
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected float jumpForce = 8f;
    [SerializeField] protected Transform wallCheckPoint;
    [SerializeField] protected float wallCheckDistance = 0.5f;
    [SerializeField] protected Transform ledgeCheckPoint;
    [SerializeField] protected float ledgeCheckDistance = 0.6f;

    [SerializeField] protected LayerMask groundLayer;
    [SerializeField] protected LayerMask wallLayer;

    protected bool facingRight = true;

    [Header("Player Detection")]
    [SerializeField] protected float detectionRange = 5f;
    [SerializeField] protected float loseDetectionRange = 7.5f;
    [SerializeField] protected float verticalDetectionRange = 4f;

    [Header("Enemy Attack Settings")]
    [SerializeField] protected float attackDamage = 10f;
    [SerializeField] protected float attackKnockback = 15f;

    // === BARU: SETTING SERANGAN BERBASIS ANIMASI ===
    [SerializeField] protected float attackRange = 1.5f;     // Jarak minimal musuh untuk memukul
    [SerializeField] protected float attackCooldown = 2f;    // Jeda waktu antar serangan (detik)
    [SerializeField] protected Transform attackPoint;        // Objek kosong penanda titik pusat pukulan
    [SerializeField] protected float attackRadius = 0.6f;     // Radius bulatan jangkauan hit pedang/tangan
    [SerializeField] protected LayerMask playerLayer;        // Pilih layer "Player"

    protected float attackCooldownTimer = 0f;
    protected bool isAttacking = false;

    [Header("AI Jump Limiter")]
    [SerializeField] protected float jumpCooldown = 1.5f;
    protected float jumpCooldownTimer = 0f;

    [Header("Anti-Stuck Settings")]
    [SerializeField] protected float headCheckDistance = 1.0f; // Jarak tembakan raycast ke bawah kaki
    protected bool isStandingOnPlayer = false;

    protected Transform playerTransform;

    protected bool isAtWall;
    protected bool isAtLedge;

    protected override void Start()
    {
        base.Start();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;

        facingRight = transform.localScale.x > 0;
    }

    protected override void Update()
    {
        base.Update();
        if (isDead) return;
        if (isRecoiling) return;

        // === PERBAIKAN: Hentikan AI berpikir jika sedang ter-Stun akibat Parry ===
        if (isStunned) return;

        // Hitung mundur timer cooldown lompat
        if (jumpCooldownTimer > 0)
        {
            jumpCooldownTimer -= Time.deltaTime;
        }

        // Hitung mundur timer cooldown serangan
        if (attackCooldownTimer > 0)
        {
            attackCooldownTimer -= Time.deltaTime;
        }

        CheckPhysicalObstacles();
        CheckIfStandingOnPlayer();

        isChasing = (currentState == EnemyStates.Chase);

        switch (currentState)
        {
            case EnemyStates.Idle: LogicIdleState(); break;
            case EnemyStates.Patrol: LogicPatrolState(); break;
            case EnemyStates.Chase: LogicChaseState(); break;
        }
    }

    protected void CheckPhysicalObstacles()
    {
        Vector2 wallDir = facingRight ? Vector2.right : Vector2.left;

        isAtWall = Physics2D.Raycast(wallCheckPoint.position, wallDir, wallCheckDistance, wallLayer);

        LayerMask walkableLayers = groundLayer | wallLayer;
        isAtLedge = !Physics2D.Raycast(ledgeCheckPoint.position, Vector2.down, ledgeCheckDistance, walkableLayers);
    }

    protected virtual void LogicIdleState()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        if (SeePlayer(detectionRange)) currentState = EnemyStates.Chase;
    }

    protected virtual void LogicPatrolState()
    {
        if (SeePlayer(detectionRange))
        {
            currentState = EnemyStates.Chase;
            return;
        }

        rb.linearVelocity = new Vector2(facingRight ? moveSpeed : -moveSpeed, rb.linearVelocity.y);

        if (isAtWall || isAtLedge)
        {
            Flip();
        }
    }

    protected virtual void LogicChaseState()
    {
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        if (playerTransform == null || !SeePlayer(loseDetectionRange))
        {
            currentState = EnemyStates.Patrol;
            return;
        }

        if (isAttacking) return;

        float dirToPlayer = playerTransform.position.x - transform.position.x;
        bool playerToRight = dirToPlayer > 0;

        // PERBAIKAN 1: Jangan ijinkan musuh otomatis berbalik arah jika sedang di atas kepala player
        // Ini untuk menghindari musuh gemetar (jittering) bolak-balik
        if (!isStandingOnPlayer)
        {
            if (playerToRight != facingRight)
            {
                Flip();
            }
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // PERBAIKAN 2: Jika di atas kepala player, jangan menyerang dulu. Fokus meluncur turun.
        if (distanceToPlayer <= attackRange && attackCooldownTimer <= 0f && !isStandingOnPlayer)
        {
            StartMeleeAttack();
            return;
        }

        float moveX = playerToRight ? moveSpeed * 1.2f : -moveSpeed * 1.2f;

        if (isStandingOnPlayer)
        {
            // 1. TAMBAH KECEPATAN: Naikkan multiplier menjadi 2.5f agar dorongan menjauhnya lebih jauh dan cepat
            moveX = facingRight ? moveSpeed * 2.5f : -moveSpeed * 2.5f;
        }

        float moveY = rb.linearVelocity.y;

        // LOGIKA LOMPAT
        bool isGrounded = Mathf.Abs(moveY) < 0.1f;
        // 2. KUNCI LOMPATAN: Jangan biarkan lompat kalau sedang di atas player
        bool canJump = isGrounded && (jumpCooldownTimer <= 0f) && !isStandingOnPlayer;

        if (canJump)
        {
            bool triggeredJump = false;

            if (isAtWall)
            {
                moveY = jumpForce;
                triggeredJump = true;
            }
            else if (isAtLedge)
            {
                moveY = jumpForce;
                triggeredJump = true;
            }
            else if (playerTransform.position.y > transform.position.y + 1.5f)
            {
                if (Mathf.Abs(dirToPlayer) < 3.0f)
                {
                    moveY = jumpForce;
                    triggeredJump = true;
                }
            }

            if (triggeredJump)
            {
                jumpCooldownTimer = jumpCooldown;
            }
        }

        // 3. BYPASS REM OTOMATIS: Tambahkan kondisi !isStandingOnPlayer
        // Supaya kecepatan X tidak di-nol-kan selama musuh berusaha menjauh dari kepala player
        if (Mathf.Abs(dirToPlayer) < 0.8f && !isStandingOnPlayer)
            rb.linearVelocity = new Vector2(0, moveY);
        else
            rb.linearVelocity = new Vector2(moveX, moveY);
    }

    // === PERBAIKAN: Override fungsi hit agar status serang di-reset saat musuh dipukul/di-parry ===
    public override void EnemyHit(float _damage, Vector2 _hitDirection, float _recoilStrength)
    {
        // Paksa reset status menyerang agar tidak mengunci pergerakan musuh selamanya
        isAttacking = false;

        // Panggil logika dasar dari Enemy.cs (untuk menangani pengurangan darah, knockback, dan StunRoutine)
        base.EnemyHit(_damage, _hitDirection, _recoilStrength);
    }

    // === BARU: FUNGSI-FUNGSI EKSTENSIONAL UNTUK ATTACK ===
    protected void StartMeleeAttack()
    {
        isAttacking = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // Berhenti melangkah maju saat mengayunkan senjata

        if (anim != null)
        {
            anim.SetBool("isRunning", false);
            anim.SetTrigger("Attack"); // Memicu parameter Trigger "Attack" di Animator
        }

        attackCooldownTimer = attackCooldown;
    }

    // PENTING: Fungsi ini wajib dipasang pada frame ayunan pedang lewat Animation Event di Unity!
    public void AnimationTriggerDamage()
    {
        if (isDead || attackPoint == null) return;

        // Ambil SEMUA objek (tanpa peduli layer apa pun) yang masuk ke dalam lingkaran pukulan
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius);

        // Cek satu-per-satu objek yang kena
        foreach (Collider2D hit in hitObjects)
        {
            // Jika objek tersebut memiliki Tag "Player"
            if (hit.CompareTag("Player"))
            {
                PlayerController player = hit.GetComponent<PlayerController>();
                if (player != null)
                {
                    // Hitung arah dorongan (knockback)
                    Vector2 hitDir = (player.transform.position - transform.position).normalized;

                    // Berikan damage
                    player.TakeDamage(attackDamage, hitDir, attackKnockback);

                    // Hentikan pencarian karena Player sudah ketemu dan dipukul (agar tidak double hit)
                    break;
                }
            }
        }
    }

    // PENTING: Fungsi ini wajib dipasang pada frame paling akhir animasi menyerang Anda!
    public void AnimationTriggerEndAttack()
    {
        isAttacking = false;
    }

    protected bool SeePlayer(float horizontalRange)
    {
        if (playerTransform == null) return false;
        float distX = Mathf.Abs(playerTransform.position.x - transform.position.x);
        float distY = Mathf.Abs(playerTransform.position.y - transform.position.y);
        return distX <= horizontalRange && distY <= verticalDetectionRange;
    }

    protected void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x = facingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    protected override void HandleAnimations()
    {
        if (anim != null)
        {
            // === BARU: Tambahkan kondisi isAttacking agar animasi lari/lompat tidak menimpa animasi menyerang ===
            if (isRecoiling || isAttacking)
            {
                anim.SetBool("isRunning", false);
                anim.SetBool("isJumping", false);
                return;
            }

            anim.SetBool("isRunning", isChasing);

            bool isJumping = Mathf.Abs(rb.linearVelocity.y) > 1.0f;
            anim.SetBool("isJumping", isJumping);
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(detectionRange * 2, verticalDetectionRange * 2, 1));

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(loseDetectionRange * 2, (verticalDetectionRange + 1.5f) * 2, 1));

        // Menggambar garis deteksi bawah kaki berwarna hijau
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3.down * headCheckDistance));

        if (wallCheckPoint != null)
        {
            Gizmos.color = Color.blue;
            Vector2 wallDir = facingRight ? Vector2.right : Vector2.left;
            Gizmos.DrawLine(wallCheckPoint.position, (Vector2)wallCheckPoint.position + (wallDir * wallCheckDistance));
        }

        if (ledgeCheckPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(ledgeCheckPoint.position, (Vector2)ledgeCheckPoint.position + (Vector2.down * ledgeCheckDistance));
        }

        // === BARU: Membantu visualisasi bulatan jangkauan serang berwarna MERAH di Scene View ===
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }

    // === BARU: MENONAKTIFKAN CONTACT DAMAGE LAMA ===
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        // Kode sengaja dikosongkan agar musuh tidak lagi merusak player hanya karena bersentuhan badan.
        // Sekarang damage murni diatur penuh dari frame animasi tebasan pedang/pukulan di atas!
    }

    protected void CheckIfStandingOnPlayer()
    {
        if (playerTransform == null) return;

        // Hitung jarak X dan Y antara musuh dan player
        float jarakX = Mathf.Abs(transform.position.x - playerTransform.position.x);
        float jarakY = transform.position.y - playerTransform.position.y;

        // Cek apakah musuh berada sangat dekat secara horizontal (X < 0.8f)
        // DAN posisinya berada di atas player (Y antara 0.5f sampai 2.5f)
        // Sesuaikan angka 2.5f dengan tinggi sprite karaktermu jika dirasa kurang pas
        if (jarakX < 0.8f && jarakY > 0.5f && jarakY < 2.5f)
        {
            isStandingOnPlayer = true;
        }
        else
        {
            isStandingOnPlayer = false;
        }
    }
}
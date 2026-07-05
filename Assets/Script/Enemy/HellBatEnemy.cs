using System.Collections;
using UnityEngine;

public class HellBatEnemy : EnemyAIBase
{
    [Header("HellBat Patrol Points & Hover")]
    [Tooltip("Titik A batas patroli")]
    [SerializeField] private Transform pointA;
    [Tooltip("Titik B batas patroli")]
    [SerializeField] private Transform pointB;
    private Transform currentPatrolTarget;

    [Tooltip("Ketinggian melayang dari tanah (Patroli) atau atas kepala (Chase)")]
    [SerializeField] private float flyHoverHeight = 3f;
    [Tooltip("Tinggi ayunan naik-turun otomatis")]
    [SerializeField] private float waveAmplitude = 0.4f;
    [Tooltip("Kecepatan ayunan naik-turun")]
    [SerializeField] private float waveFrequency = 3.5f;
    [Tooltip("Jarak tembakan sensor deteksi tanah ke bawah")]
    [SerializeField] private float groundCheckDistance = 15f;

    [Header("Laser Launcher Settings")]
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private Transform firePoint;

    protected override void Start()
    {
        base.Start();
        if (rb != null)
        {
            rb.gravityScale = 0f;
        }

        // Lepaskan Titik A dan B dari induk (Kelelawar) agar posisinya diam tertanam di dunia
        if (pointA != null) pointA.SetParent(null);
        if (pointB != null) pointB.SetParent(null);

        currentPatrolTarget = pointB; // Default terbang ke Titik B pertama kali
    }

    protected override void Update()
    {
        base.Update();

        // === FIX PARRY / KNOCKBACK MELAYANG KE ANGKASA ===
        // Berikan gaya "gesekan udara" saat dia sedang terlempar (Stun/Recoil)
        // Kecepatannya akan direm perlahan menjadi 0 agar dia tidak meluncur bablas
        if ((isStunned || isRecoiling) && rb != null && !isDead)
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.deltaTime * 3f);
        }
    }

    // OVERRIDE: Logika Patroli Point A ke Point B dengan Ground Check
    protected override void LogicPatrolState()
    {
        if (SeePlayer(detectionRange))
        {
            currentState = EnemyStates.Chase;
            return;
        }

        if (pointA == null || pointB == null)
        {
            Debug.LogWarning("Titik A atau B pada HellBat belum dimasukkan di Inspector!");
            return;
        }

        // 1. CEK TANAH (GROUND CHECK) UNTUK MENGATUR KETINGGIAN TARGET
        float targetY = transform.position.y;
        RaycastHit2D groundHit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);

        if (groundHit.collider != null)
        {
            targetY = groundHit.point.y + flyHoverHeight;
        }

        // Tambahkan efek naik-turun natural (sinus wave)
        targetY += Mathf.Sin(Time.time * waveFrequency) * waveAmplitude;

        // 2. TERBANG MENUJU TITIK TARGET
        float dirX = currentPatrolTarget.position.x - transform.position.x;
        bool targetToRight = dirX > 0;

        if (targetToRight != facingRight) Flip();

        float moveX = targetToRight ? moveSpeed : -moveSpeed;
        float moveY = (targetY - transform.position.y) * 5f; // Lerp vertikal agar mulus

        rb.linearVelocity = new Vector2(moveX, moveY);

        // Jika jarak dengan titik tujuan sudah dekat (< 0.5f), balik target rutenya
        if (Mathf.Abs(dirX) < 0.5f)
        {
            currentPatrolTarget = (currentPatrolTarget == pointA) ? pointB : pointA;
        }
    }

    // OVERRIDE: Logika Mengejar Player
    protected override void LogicChaseState()
    {
        if (playerTransform == null) return;

        if (!SeePlayer(loseDetectionRange))
        {
            currentState = EnemyStates.Patrol;
            return;
        }

        if (isAttacking) return;

        float dirToPlayer = playerTransform.position.x - transform.position.x;
        bool playerToRight = dirToPlayer > 0;
        if (playerToRight != facingRight) Flip();

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= attackRange && attackCooldownTimer <= 0f)
        {
            StartMeleeAttack();
            return;
        }

        // Kejar dengan target di atas kepala player
        Vector2 targetChasePos = new Vector2(playerTransform.position.x, playerTransform.position.y + flyHoverHeight);
        Vector2 moveDir = (targetChasePos - (Vector2)transform.position).normalized;

        rb.linearVelocity = moveDir * (moveSpeed * 1.2f);
    }

    // OVERRIDE: Tembakan Laser
    protected override IEnumerator RandomizedAttackRoutine()
    {
        isPreparingAttack = true;
        isAttacking = true;

        rb.linearVelocity = Vector2.zero;

        if (anim != null)
        {
            anim.SetBool("isRunning", false);
            anim.SetTrigger("LaserCharge");
        }

        float chargeTime = Random.Range(minPreAttackDelay, maxPreAttackDelay);
        yield return new WaitForSeconds(chargeTime);

        if (!isDead && !isStunned && playerTransform != null)
        {
            ShootLaserBeam();
            if (anim != null) anim.SetTrigger("Attack");
            attackCooldownTimer = attackCooldown;
        }

        yield return new WaitForSeconds(0.5f);
        isPreparingAttack = false;
        isAttacking = false;
        currentAttackRoutine = null;
    }

    private void ShootLaserBeam()
    {
        if (laserPrefab == null || firePoint == null || playerTransform == null) return;
        Vector2 shootDirection = (playerTransform.position - firePoint.position).normalized;
        float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        Instantiate(laserPrefab, firePoint.position, targetRotation);
    }

    // === FIX BUG GK BISA MATI ===
    // Menindih/Override fungsi mati bawaan musuh darat
    protected override IEnumerator DieRoutine()
    {
        isDead = true;

        // 1. Matikan Collider agar player bisa lewat di bawahnya tanpa menabrak
        if (col != null) col.enabled = false;

        // 2. KUNCI POSISI DI UDARA (Biar ngambang)
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;        // Langsung hentikan sisa kecepatan terbangnya
            rb.gravityScale = 0f;                   // Pastikan gravitasinya tetap nol (tidak jatuh)
            rb.bodyType = RigidbodyType2D.Kinematic; // Mengunci fisik musuh agar tidak bisa terdorong/bergeser saat mati
        }

        // 3. Putar Animasi Dead
        if (anim != null)
        {
            anim.SetTrigger("Die");
            anim.SetBool("isDead", true);
        }

        // 4. Tunggu sampai delay selesai, lalu hilangkan dari game
        yield return new WaitForSecondsRealtime(disappearDelay);
        Destroy(gameObject);
    }
}
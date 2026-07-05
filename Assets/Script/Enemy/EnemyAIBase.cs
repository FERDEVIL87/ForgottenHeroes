using System.Collections; // PENTING: Untuk Coroutine (IEnumerator)
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

    // === MODIFIKASI JANGKAUAN SERANG ===
    [Tooltip("Jarak dari jauh di mana musuh mulai bersiap/ancang-ancang untuk menerjang player")]
    [SerializeField] protected float attackRange = 4f;       // DIUBAH JADI LEBIH JAUH (Misal: 3.5 - 5f)
    [SerializeField] protected float attackCooldown = 2f;    // Jeda waktu antar serangan (detik)
    [SerializeField] protected Transform attackPoint;        // Objek kosong penanda titik pusat pukulan
    [SerializeField] protected float attackRadius = 0.6f;     // Radius bulatan jangkauan hit pedang/tangan
    [SerializeField] protected LayerMask playerLayer;        // Pilih layer "Player"

    // === RANDOM ATTACK & LUNGE SETTINGS ===
    [Header("Random Attack & Lunge Timing")]
    [Tooltip("Waktu minimal musuh menahan tebasannya / ancang-ancang (detik)")]
    [SerializeField] protected float minPreAttackDelay = 0.1f;
    [Tooltip("Waktu maksimal musuh menahan tebasannya / ancang-ancang (detik)")]
    [SerializeField] protected float maxPreAttackDelay = 0.6f;

    [Tooltip("Kekuatan dorongan maju musuh saat menerjang dari jauh")]
    [SerializeField] protected float attackLungeForce = 12f;  // DINAIKKAN nilainya agar dorongannya cepat dan jauh!

    protected bool isPreparingAttack = false;
    protected Coroutine currentAttackRoutine; // Untuk menyimpan dan menghentikan persiapan serangan

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

        // Hentikan AI berpikir jika sedang ter-Stun akibat Parry
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

        if (!isStandingOnPlayer)
        {
            if (playerToRight != facingRight)
            {
                Flip();
            }
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // === DETEKSI SERANG DARI JAUH ===
        if (distanceToPlayer <= attackRange && attackCooldownTimer <= 0f && !isStandingOnPlayer)
        {
            StartMeleeAttack();
            return;
        }

        float moveX = playerToRight ? moveSpeed * 1.2f : -moveSpeed * 1.2f;

        if (isStandingOnPlayer)
        {
            moveX = facingRight ? moveSpeed * 2.5f : -moveSpeed * 2.5f;
        }

        float moveY = rb.linearVelocity.y;

        bool isGrounded = Mathf.Abs(moveY) < 0.1f;
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

        if (Mathf.Abs(dirToPlayer) < 0.8f && !isStandingOnPlayer)
            rb.linearVelocity = new Vector2(0, moveY);
        else
            rb.linearVelocity = new Vector2(moveX, moveY);
    }

    public override void EnemyHit(float _damage, Vector2 _hitDirection, float _recoilStrength)
    {
        // Batal serang jika musuh dipukul player saat bersiap
        isAttacking = false;
        isPreparingAttack = false;

        if (currentAttackRoutine != null)
        {
            StopCoroutine(currentAttackRoutine);
            currentAttackRoutine = null;
        }

        base.EnemyHit(_damage, _hitDirection, _recoilStrength);
    }

    protected void StartMeleeAttack()
    {
        if (!isPreparingAttack)
        {
            currentAttackRoutine = StartCoroutine(RandomizedAttackRoutine());
        }
    }

    protected virtual IEnumerator RandomizedAttackRoutine()
    {
        isPreparingAttack = true;
        isAttacking = true;

        // 1. AN_CANG-ANCANG: Hentikan langkah saat musuh bersiap memukul (mengecoh parry player dari jauh)
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (anim != null)
        {
            anim.SetBool("isRunning", false);
        }

        // Jeda acak ancang-ancang
        float randomDelay = Random.Range(minPreAttackDelay, maxPreAttackDelay);
        yield return new WaitForSeconds(randomDelay);

        // 2. EKSEKUSI TERJANGAN DAN SERANGAN
        if (!isDead && !isStunned)
        {
            if (playerTransform != null)
            {
                // Re-tracking arah player sebelum melesat maju
                float dirToPlayer = playerTransform.position.x - transform.position.x;
                bool playerToRight = dirToPlayer > 0;

                if (playerToRight != facingRight)
                {
                    Flip();
                }

                // LUNGE: Melesat maju dengan kecepatan tinggi menuju player!
                float lungeDir = facingRight ? 1f : -1f;
                rb.linearVelocity = new Vector2(lungeDir * attackLungeForce, rb.linearVelocity.y);
            }

            // Putar animasi tebasan (Pastikan hitboxes/Animation Event kamu berada di frame tebasan yang pas)
            if (anim != null)
            {
                anim.SetTrigger("Attack");
            }
            attackCooldownTimer = attackCooldown;
        }

        isPreparingAttack = false;
        currentAttackRoutine = null;
    }

    public void AnimationTriggerDamage()
    {
        if (isDead || attackPoint == null) return;

        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius);

        foreach (Collider2D hit in hitObjects)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerController player = hit.GetComponent<PlayerController>();
                if (player != null)
                {
                    Vector2 hitDir = (player.transform.position - transform.position).normalized;
                    player.TakeDamage(attackDamage, hitDir, attackKnockback);
                    break;
                }
            }
        }
    }

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

        // Mengganti warna jangkauan deteksi serangan awal (attackRange) menjadi MAGENTA agar mudah dibedakan
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
    }

    protected void CheckIfStandingOnPlayer()
    {
        if (playerTransform == null) return;

        float jarakX = Mathf.Abs(transform.position.x - playerTransform.position.x);
        float jarakY = transform.position.y - playerTransform.position.y;

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
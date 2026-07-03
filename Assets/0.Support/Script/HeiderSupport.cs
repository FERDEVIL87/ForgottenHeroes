using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeiderSupport : SupportBase
{
    [Header("Heider Base Settings")]
    [SerializeField] private float castDelay = 0.3f;
    [SerializeField] private Transform firePoint;

    // =================================================================
    // STAT PROJECTILE MODULAR
    // =================================================================
    [Header("Heider Projectile Settings")]
    [Tooltip("Centang jika ingin bola api menembus tembok. Kosongkan jika ingin hancur/meledak saat kena tembok.")]
    [SerializeField] private bool projectilePiercesObstacles = false; // <-- TOGGLE BARU

    [Space]
    [Tooltip("Damage jika menabrak 1 target secara langsung (tanpa ledakan)")]
    [SerializeField] private float projectileImpactDamage = 45f;
    [Tooltip("Prefab efek visual saat peluru mengenai musuh dalam mode Single Target")]
    [SerializeField] private GameObject singleTargetImpactPrefab;
    [Tooltip("Berapa detik efek hantaman single target akan bertahan sebelum hilang otomatis")]
    [SerializeField] private float singleTargetVFXLifetime = 1.0f;

    [Space]
    [Header("--- AOE Explosion Settings ---")]
    [Tooltip("Centang untuk mengubah bola api menjadi bom yang meledak saat menyentuh musuh.")]
    [SerializeField] private bool projectileIsAOE = false;
    [Tooltip("Radius area ledakan")]
    [SerializeField] private float aoeRadius = 2.5f;
    [Tooltip("Damage yang dihasilkan oleh ledakan ke semua musuh di area")]
    [SerializeField] private float aoeDamage = 60f;
    [Tooltip("Prefab efek visual (VFX) ledakan yang akan muncul")]
    [SerializeField] private GameObject aoeExplosionPrefab;
    [Tooltip("Berapa detik efek ledakan AOE akan bertahan sebelum hilang otomatis")]
    [SerializeField] private float aoeExplosionLifetime = 1.5f;

    // =================================================================
    // STAT DASH MODULAR
    // =================================================================
    [Header("Heider Dash Settings")]
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.25f;
    [SerializeField] private float dashDamage = 35f;
    [SerializeField] private float dashRadius = 1.3f;

    protected override IEnumerator ExecuteCharacterSkill()
    {
        if (anim != null && !string.IsNullOrEmpty(activeSkill.animationTriggerName))
            anim.SetTrigger(activeSkill.animationTriggerName);

        if (activeSkill.skillSFX != null)
            AudioSource.PlayClipAtPoint(activeSkill.skillSFX, transform.position);

        yield return new WaitForSeconds(castDelay);

        if (activeSkill.skillType == SupportSkillType.Projectile)
        {
            ExecuteProjectileSkill();
        }
        else if (activeSkill.skillType == SupportSkillType.Dash)
        {
            yield return StartCoroutine(ExecuteDashSkill());
        }

        yield return new WaitForSeconds(lingerTime);
    }

    private void ExecuteProjectileSkill()
    {
        if (activeSkill.effectPrefab != null && firePoint != null)
        {
            Quaternion bulletRotation = (faceDir == 1) ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.identity;
            GameObject projectile = Instantiate(activeSkill.effectPrefab, firePoint.position, bulletRotation);

            FireBall fb = projectile.GetComponent<FireBall>();
            if (fb != null)
            {
                // 1. Tentukan apakah bola api milik Heider bisa tembus tembok atau tidak
                fb.SetObstacleBehavior(projectilePiercesObstacles);

                // 2. Suntikkan data dasar Single Target
                fb.SetSingleTargetStats(projectileImpactDamage, singleTargetImpactPrefab, singleTargetVFXLifetime);

                // 3. Suntikkan tambahan data Ledakan jika fitur AOE diaktifkan
                if (projectileIsAOE)
                {
                    fb.SetExplosionStats(aoeRadius, aoeDamage, aoeExplosionPrefab, aoeExplosionLifetime, activeSkill.enemyLayer);
                }
            }
        }
    }

    private IEnumerator ExecuteDashSkill()
    {
        Vector2 dashDirection = (faceDir == 0) ? Vector2.right : Vector2.left;
        List<Collider2D> damagedEnemies = new List<Collider2D>();
        float dashTimer = 0f;

        while (dashTimer < dashDuration)
        {
            dashTimer += Time.deltaTime;

            if (rb != null)
                rb.linearVelocity = dashDirection * dashSpeed;
            else
                transform.position += (Vector3)(dashDirection * dashSpeed * Time.deltaTime);

            Collider2D[] hitMusuh = Physics2D.OverlapCircleAll(transform.position, dashRadius, activeSkill.enemyLayer);

            if (hitMusuh.Length == 0)
            {
                Collider2D[] semuaSekitar = Physics2D.OverlapCircleAll(transform.position, dashRadius);
                List<Collider2D> daftarMusuhTag = new List<Collider2D>();
                foreach (Collider2D col in semuaSekitar)
                {
                    if (col.CompareTag("Enemy")) daftarMusuhTag.Add(col);
                }
                if (daftarMusuhTag.Count > 0) hitMusuh = daftarMusuhTag.ToArray();
            }

            foreach (Collider2D musuh in hitMusuh)
            {
                if (!damagedEnemies.Contains(musuh))
                {
                    damagedEnemies.Add(musuh);

                    Enemy enemyScript = musuh.GetComponent<Enemy>();
                    if (enemyScript == null) enemyScript = musuh.GetComponentInParent<Enemy>();

                    if (enemyScript != null)
                    {
                        enemyScript.EnemyHit(dashDamage, dashDirection, 5f);
                    }
                }
            }

            yield return null;
        }

        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, dashRadius);
    }
}
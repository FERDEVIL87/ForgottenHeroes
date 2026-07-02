using System.Collections;
using System.Collections.Generic; // Dibutuhkan untuk List
using UnityEngine;

public class HeiderSupport : MonoBehaviour
{
    private SpriteRenderer sr;
    private Animator anim;
    private bool hasFired = false;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float castDelay = 0.3f;
    [SerializeField] private float lingerTime = 0.5f;

    [Header("Spawn Point")]
    [SerializeField] private Transform firePoint;

    public int faceDir { get; set; }

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        if (faceDir == 1) // Jika Player menghadap KIRI
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else // Jika Player menghadap KANAN
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        Color c = sr.color;
        c.a = 0f;
        sr.color = c;

        StartCoroutine(SupportRoutine());
    }

    private IEnumerator SupportRoutine()
    {
        // === FASE 1: FADE IN ===
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
            yield return null;
        }
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);

        // Ambil data skill yang sedang aktif dari SupportManager
        SupportSkillSO activeSkill = (SupportManager.Instance != null) ? SupportManager.Instance.equippedSkill : null;

        if (activeSkill != null)
        {
            // =================================================================
            // CABANG A: JIKA SKILL ADALAH TIPE PROJECTILE (LOGIKA LAMA)
            // =================================================================
            if (activeSkill.skillType == SupportSkillType.Projectile)
            {
                if (anim != null && !string.IsNullOrEmpty(activeSkill.animationTriggerName))
                {
                    anim.SetTrigger(activeSkill.animationTriggerName);
                }

                yield return new WaitForSeconds(castDelay);

                if (!hasFired)
                {
                    hasFired = true;

                    if (activeSkill.skillSFX != null)
                    {
                        AudioSource.PlayClipAtPoint(activeSkill.skillSFX, transform.position);
                    }

                    GameObject skillPrefab = activeSkill.effectPrefab;
                    if (skillPrefab != null && firePoint != null)
                    {
                        Quaternion bulletRotation = Quaternion.identity;
                        if (faceDir == 1)
                        {
                            bulletRotation = Quaternion.Euler(0f, 180f, 0f);
                        }
                        Instantiate(skillPrefab, firePoint.position, bulletRotation);
                    }
                }

                yield return new WaitForSeconds(lingerTime);
            }
            // =================================================================
            // CABANG B: JIKA SKILL ADALAH TIPE DASH ATTACK (SKILL BARU)
            // =================================================================
            else if (activeSkill.skillType == SupportSkillType.Dash)
            {
                // 1. MAINKAN SFX DASH (Jika diinput)
                if (activeSkill.skillSFX != null)
                {
                    AudioSource.PlayClipAtPoint(activeSkill.skillSFX, transform.position);
                }

                // Ambil Rigidbody2D jika ada di prefab Heider untuk pergerakan fisik yang lebih halus
                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                Vector2 dashDirection = (faceDir == 0) ? Vector2.right : Vector2.left;

                // List penampung musuh agar tidak terkena hit berkali-kali di frame berturut-turut
                List<Collider2D> damagedEnemies = new List<Collider2D>();

                float dashTimer = 0f;

                // 2. FASE DASH KEDEPAN + DEAL DAMAGE
                while (dashTimer < activeSkill.dashDuration)
                {
                    dashTimer += Time.deltaTime;
                    // Eksekusi pergerakan maju
                    if (rb != null)
                    {
                        rb.linearVelocity = dashDirection * activeSkill.dashSpeed; // <--- SUDAH DIPERBARUI
                    }
                    else
                    {
                        transform.position += (Vector3)(dashDirection * activeSkill.dashSpeed * Time.deltaTime);
                    }

                    // Deteksi lingkaran area badan Heider sepanjang jalur dash
                    Collider2D[] hitMusuh = Physics2D.OverlapCircleAll(transform.position, 1.0f, activeSkill.enemyLayer);
                    foreach (Collider2D musuh in hitMusuh)
                    {
                        if (!damagedEnemies.Contains(musuh))
                        {
                            damagedEnemies.Add(musuh);

                            // Panggil fungsi damage musuh (Sesuaikan nama fungsi "TakeDamage" dengan skrip musuhmu)
                            musuh.SendMessage("TakeDamage", activeSkill.dashDamage, SendMessageOptions.DontRequireReceiver);
                            Debug.Log($"Kameo menghantam {musuh.name} sebesar {activeSkill.dashDamage} damage!");
                        }
                    }

                    yield return null;
                }
                // Dash selesai, hentikan sisa dorongan kecepatan fisik
                if (rb != null) rb.linearVelocity = Vector2.zero; // <--- SUDAH DIPERBARUI

                // 3. FASE CASTING (Memicu animasi casting/pose setelah dash selesai)
                if (anim != null && !string.IsNullOrEmpty(activeSkill.animationTriggerName))
                {
                    anim.SetTrigger(activeSkill.animationTriggerName);
                }

                // Heider diam melakukan cast/pose selama durasi lingerTime sebelum menghilang
                yield return new WaitForSeconds(lingerTime);
            }
        }

        // === FASE 4: FADE OUT & DESTROY ===
        t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }

    // Menggambar area hit/deteksi dash di Scene View Unity biar gampang kamu debug ukurannya
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1.0f);
    }
}
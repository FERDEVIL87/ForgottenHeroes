using System.Collections;
using UnityEngine;

public class HeiderSupport : MonoBehaviour
{
    private SpriteRenderer sr;
    private Animator anim;
    private bool hasFired = false; // Pengunci agar hanya menembak 1 kali

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float castDelay = 0.3f;
    [SerializeField] private float lingerTime = 0.5f;

    [Header("Spawn Point")]
    [SerializeField] private Transform firePoint;

    // Menampung arah hadap yang dikirim dari SupportManager (0 = Kanan, 1 = Kiri)
    public int faceDir { get; set; }

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        // === 1. LOGIKA MEMBALIKKAN BADAN (LOCAL SCALE) ===
        if (faceDir == 1) // Jika Player menghadap KIRI
        {
            // Balikkan scale X menjadi minus agar gambar menghadap ke kiri dengan rapi
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else // Jika Player menghadap KANAN
        {
            // Pastikan scale X tetap positif (menghadap kanan)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        // Mulai dari kondisi transparan (Fade In)
        Color c = sr.color;
        c.a = 0f;
        sr.color = c;

        StartCoroutine(SupportRoutine());
    }

    private IEnumerator SupportRoutine()
    {
        // === FASE 1: FADE IN ===
        // (Kode lamamu tetap sama...)
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
            yield return null;
        }
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);

        // === FASE 2: MEMULAI ANIMASI SERANGAN ===
        if (SupportManager.Instance != null && SupportManager.Instance.equippedSkill != null)
        {
            string triggerName = SupportManager.Instance.equippedSkill.animationTriggerName;
            if (anim != null && !string.IsNullOrEmpty(triggerName))
            {
                anim.SetTrigger(triggerName);
            }
        }

        // Tunggu sampai momen yang pas sebelum bola energi keluar
        yield return new WaitForSeconds(castDelay);

        // === FASE 3: TEMBAK BOLA ENERGI & PLAY SFX (UPDATED) ===
        if (!hasFired && SupportManager.Instance != null && SupportManager.Instance.equippedSkill != null)
        {
            hasFired = true;

            // =========================================================
            // === LOGIKA BARU: PLAY SFX SKILL ===
            // =========================================================
            AudioClip audioToPlay = SupportManager.Instance.equippedSkill.skillSFX;
            if (audioToPlay != null)
            {
                // Memutar suara di posisi Heider berada tanpa memotong suara saat objek di-destroy
                AudioSource.PlayClipAtPoint(audioToPlay, transform.position);
            }
            // =========================================================

            GameObject skillPrefab = SupportManager.Instance.equippedSkill.effectPrefab;
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

        // Tunggu beberapa saat di arena setelah menyerang sebelum menghilang
        yield return new WaitForSeconds(lingerTime);

        // === FASE 4: FADE OUT & DESTROY ===
        // (Kode lamamu tetap sama...)
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
}
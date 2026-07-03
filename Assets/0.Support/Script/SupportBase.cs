using System.Collections;
using UnityEngine;

public abstract class SupportBase : MonoBehaviour
{
    protected SpriteRenderer sr;
    protected Animator anim;
    protected Rigidbody2D rb; // Proteksi agar bisa diakses oleh skrip anak

    [Header("Base Fade Settings")]
    [SerializeField] protected float fadeDuration = 0.3f;
    [SerializeField] protected float lingerTime = 0.5f;

    protected int faceDir;
    protected SupportSkillSO activeSkill;

    public virtual void Init(int direction, SupportSkillSO skill)
    {
        faceDir = direction;
        activeSkill = skill;
    }

    protected virtual void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>(); // Deteksi komponen fisik di Unity 6

        // Atur arah hadap balik badan
        if (faceDir == 1)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        if (sr != null)
        {
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;
        }

        StartCoroutine(MasterRoutine());
    }

    private IEnumerator MasterRoutine()
    {
        yield return StartCoroutine(FadeRoutine(0f, 1f)); // Fade In

        if (activeSkill != null)
        {
            yield return StartCoroutine(ExecuteCharacterSkill()); // Skill Unik
        }

        yield return StartCoroutine(FadeRoutine(1f, 0f)); // Fade Out
        Destroy(gameObject);
    }

    private IEnumerator FadeRoutine(float startAlpha, float endAlpha)
    {
        if (sr == null) yield break;
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t / fadeDuration);
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
            yield return null;
        }
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, endAlpha);
    }

    protected abstract IEnumerator ExecuteCharacterSkill();
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IntroCutsceneManager : MonoBehaviour
{
    [System.Serializable]
    public struct PrologueSlide
    {
        public Sprite image;
        [TextArea(3, 5)] public string dialogueText;
    }

    public enum AnimParamType { Float, Bool, Trigger }

    [Header("Developer Testing")]
    [SerializeField] private bool forcePlayInEditor = false;

    [Header("UI Canvas Utama (HUD Game)")]
    [SerializeField] private GameObject gameHUDCanvas;

    [Header("UI Components (Prolog)")]
    [SerializeField] private GameObject cutsceneCanvas;
    [SerializeField] private CanvasGroup bgCanvasGroup;
    [SerializeField] private CanvasGroup slideCanvasGroup;
    [SerializeField] private Image prologueImageUI;
    [SerializeField] private TextMeshProUGUI dialogueTextUI;
    [SerializeField] private float fadeDuration = 1.0f;

    [Header("Data Prolog (Gambar & Cerita)")]
    [SerializeField] private List<PrologueSlide> prologueSlides;

    [Header("Mekanik Goa (In-Game Cutscene)")]
    [SerializeField] private Transform caveStartPoint;
    [SerializeField] private Transform caveExitPoint;
    [SerializeField] private float playerRunSpeed = 5f;
    [TextArea(2, 4)]
    [SerializeField] private string caveDialogue = "Aku harus segera keluar dari goa yang runtuh ini...!";

    [Header("Pengaturan Animasi Karakter Jalan")]
    [SerializeField] private string walkParameterName = "walkSpeed";
    [SerializeField] private AnimParamType animationType = AnimParamType.Float;
    [Tooltip("Nilai yang dikirim ke animator jika memilih tipe Float/Bool saat jalan")]
    [SerializeField] private float floatWalkValue = 1f;
    [SerializeField] private bool autoFlipFacingDirection = true;

    private bool isWaitingForInput = false;

    void Start()
    {
        if (!PlayerPrefs.HasKey("HasSaveData") || forcePlayInEditor)
        {
            StartCoroutine(PlayIntroCutsceneRoutine());
        }
        else
        {
            SkipCutsceneImmediate();
        }
    }

    void Update()
    {
        if (isWaitingForInput && (Input.GetButtonDown("Jump") || Input.GetButtonDown("Interact") || Input.GetButtonDown("Attack")))
        {
            isWaitingForInput = false;
        }
    }

    private IEnumerator PlayIntroCutsceneRoutine()
    {
        while (PlayerController.Instance == null) yield return null;

        PlayerController.Instance.isCutsceneMode = true;
        PlayerController.Instance.enabled = false;

        if (gameHUDCanvas != null) gameHUDCanvas.SetActive(false);

        if (caveStartPoint != null)
        {
            PlayerController.Instance.transform.position = caveStartPoint.position;
        }

        // ==========================================
        // PHASE 1: PROLOG GAMBAR (FREEZE GAME)
        // ==========================================
        Time.timeScale = 0f;

        if (cutsceneCanvas != null && prologueSlides.Count > 0)
        {
            cutsceneCanvas.SetActive(true);

            if (bgCanvasGroup != null) bgCanvasGroup.alpha = 1f;
            if (slideCanvasGroup != null) slideCanvasGroup.alpha = 0f;

            foreach (PrologueSlide slide in prologueSlides)
            {
                if (prologueImageUI != null) prologueImageUI.sprite = slide.image;
                if (dialogueTextUI != null) dialogueTextUI.text = slide.dialogueText;

                yield return StartCoroutine(FadeRoutine(slideCanvasGroup, 0f, 1f));

                isWaitingForInput = true;
                while (isWaitingForInput) yield return null;

                yield return StartCoroutine(FadeRoutine(slideCanvasGroup, 1f, 0f));
                yield return new WaitForSecondsRealtime(0.3f);
            }
        }

        // ==========================================
        // PHASE 2: PLAYER LARI KELUAR GOA (UNFREEZE)
        // ==========================================
        Time.timeScale = 1f;

        if (caveExitPoint != null)
        {
            if (prologueImageUI != null) prologueImageUI.gameObject.SetActive(false);
            if (dialogueTextUI != null) dialogueTextUI.text = caveDialogue;

            yield return StartCoroutine(FadeRoutine(slideCanvasGroup, 0f, 1f));

            Rigidbody2D playerRb = PlayerController.Instance.GetComponent<Rigidbody2D>();
            Animator playerAnim = PlayerController.Instance.GetComponent<Animator>();
            float targetX = caveExitPoint.position.x;

            StartCoroutine(FadeRoutine(bgCanvasGroup, 1f, 0f));
            StartCoroutine(FadeRoutine(slideCanvasGroup, 1f, 0f));

            // AKSI: Jalankan Animasi Jalan di Awal Fase Lari
            if (playerAnim != null && !string.IsNullOrEmpty(walkParameterName))
            {
                switch (animationType)
                {
                    case AnimParamType.Float:
                        playerAnim.SetFloat(walkParameterName, floatWalkValue);
                        break;
                    case AnimParamType.Bool:
                        playerAnim.SetBool(walkParameterName, true);
                        break;
                    case AnimParamType.Trigger:
                        playerAnim.SetTrigger(walkParameterName);
                        break;
                }
            }

            while (Mathf.Abs(PlayerController.Instance.transform.position.x - targetX) > 0.2f)
            {
                float direction = Mathf.Sign(targetX - PlayerController.Instance.transform.position.x);

                if (playerRb != null) playerRb.linearVelocity = new Vector2(direction * playerRunSpeed, playerRb.linearVelocity.y);

                // AUTO FLIP: Membalik badan karakter agar menghadap ke arah jalan keluar
                if (autoFlipFacingDirection)
                {
                    Vector3 currentScale = PlayerController.Instance.transform.localScale;
                    if (direction > 0)
                        PlayerController.Instance.transform.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
                    else if (direction < 0)
                        PlayerController.Instance.transform.localScale = new Vector3(-Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
                }

                yield return null;
            }

            // AKSI: Hentikan Fisik & Animasi Jalan karena sudah sampai tujuan
            if (playerRb != null) playerRb.linearVelocity = new Vector2(0f, playerRb.linearVelocity.y);

            if (playerAnim != null && !string.IsNullOrEmpty(walkParameterName))
            {
                if (animationType == AnimParamType.Float) playerAnim.SetFloat(walkParameterName, 0f);
                else if (animationType == AnimParamType.Bool) playerAnim.SetBool(walkParameterName, false);
            }

            if (cutsceneCanvas != null) cutsceneCanvas.SetActive(false);
        }

        // ==========================================
        // PHASE 3: KEMBALIKAN KENDALI
        // ==========================================
        if (gameHUDCanvas != null) gameHUDCanvas.SetActive(true);

        PlayerController.Instance.enabled = true;
        PlayerController.Instance.isCutsceneMode = false;
    }

    private void SkipCutsceneImmediate()
    {
        Time.timeScale = 1f;
        if (cutsceneCanvas != null) cutsceneCanvas.SetActive(false);
        if (gameHUDCanvas != null) gameHUDCanvas.SetActive(true);

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.enabled = true;
            PlayerController.Instance.isCutsceneMode = false;
        }
    }

    private IEnumerator FadeRoutine(CanvasGroup cg, float startAlpha, float endAlpha)
    {
        if (cg == null) yield break;
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / fadeDuration);
            yield return null;
        }
        cg.alpha = endAlpha;
    }
}
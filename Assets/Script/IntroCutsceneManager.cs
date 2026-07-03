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

    [Header("Cutscene Trigger Condition")]
    [Tooltip("Centang ini jika ingin menguji cutscene langsung di Unity Editor tanpa harus lewat Main Menu")]
    [SerializeField] private bool forcePlayInEditor = false;

    [Header("UI Components (Prolog)")]
    [SerializeField] private GameObject cutsceneCanvas;
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

    private bool isWaitingForInput = false;

    void Start()
    {
        // CEK: Apakah ini New Game atau Game Lanjutan (Continue)
        if (!PlayerPrefs.HasKey("HasSaveData") || forcePlayInEditor)
        {
            StartCoroutine(PlayWholeCutsceneRoutine());
        }
        else
        {
            SkipCutsceneImmediate();
        }
    }

    void Update()
    {
        // Deteksi klik atau spasi untuk lanjut ke dialog berikutnya
        if (isWaitingForInput && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E)))
        {
            isWaitingForInput = false;
        }
    }

    private IEnumerator PlayWholeCutsceneRoutine()
    {
        // 1. Amankan Player agar tidak bisa digerakkan dulu
        while (PlayerController.Instance == null) yield return null;
        PlayerController.Instance.isCutsceneMode = true;

        // Posisikan awal player di dalam goa secara tak terlihat dulu
        if (caveStartPoint != null)
        {
            PlayerController.Instance.transform.position = caveStartPoint.position;
        }

        // ==========================================
        // PHASE 1: PROLOG GAMBAR & DIALOG (UI)
        // ==========================================
        if (cutsceneCanvas != null && prologueSlides.Count > 0)
        {
            cutsceneCanvas.SetActive(true);
            slideCanvasGroup.alpha = 0f;

            foreach (PrologueSlide slide in prologueSlides)
            {
                // Set data visual dan teks
                if (prologueImageUI != null) prologueImageUI.sprite = slide.image;
                if (dialogueTextUI != null) dialogueTextUI.text = slide.dialogueText;

                // Fade In Slide
                yield return StartCoroutine(FadeRoutine(slideCanvasGroup, 0f, 1f));

                // Tunggu pemain menekan tombol Space/Klik
                isWaitingForInput = true;
                while (isWaitingForInput) yield return null;

                // Fade Out Slide
                yield return StartCoroutine(FadeRoutine(slideCanvasGroup, 1f, 0f));
                yield return new WaitForSeconds(0.2f); // Jeda antar slide
            }

            cutsceneCanvas.SetActive(false);
        }

        // ==========================================
        // PHASE 2: ANIMASI PLAYER LARI KELUAR GOA
        // ==========================================
        if (caveExitPoint != null)
        {
            // Aktifkan UI dialog in-game jika ada komponen teks khusus gameplay
            if (cutsceneCanvas != null && dialogueTextUI != null)
            {
                cutsceneCanvas.SetActive(true);
                dialogueTextUI.text = caveDialogue;
                yield return StartCoroutine(FadeRoutine(slideCanvasGroup, 0f, 1f));
            }

            Rigidbody2D playerRb = PlayerController.Instance.GetComponent<Rigidbody2D>();
            Animator playerAnim = PlayerController.Instance.GetComponent<Animator>();

            // Loop pergerakan otomatis menggunakan target koordinat X
            float targetX = caveExitPoint.position.x;
            while (Mathf.Abs(PlayerController.Instance.transform.position.x - targetX) > 0.2f)
            {
                float direction = Mathf.Sign(targetX - PlayerController.Instance.transform.position.x);

                // Gerakkan fisik player (Menggunakan .linearVelocity khas Unity 6 Anda)
                if (playerRb != null)
                {
                    playerRb.linearVelocity = new Vector2(direction * playerRunSpeed, playerRb.linearVelocity.y);
                }

                // Putar animasi lari player (Sesuaikan parameter animator Anda, misal "walkSpeed" atau "isRunning")
                if (playerAnim != null)
                {
                    playerAnim.SetFloat("walkSpeed", Mathf.Abs(playerRunSpeed));
                }

                yield return null;
            }

            // Berhenti setelah keluar goa
            if (playerRb != null) playerRb.linearVelocity = new Vector2(0f, playerRb.linearVelocity.y);
            if (playerAnim != null) playerAnim.SetFloat("walkSpeed", 0f);

            // Tunggu pembacaan dialog terakhir di luar goa
            isWaitingForInput = true;
            while (isWaitingForInput) yield return null;

            // Tutup UI Dialog akhir
            yield return StartCoroutine(FadeRoutine(slideCanvasGroup, 1f, 0f));
            if (cutsceneCanvas != null) cutsceneCanvas.SetActive(false);
        }

        // ==========================================
        // PHASE 3: MENYERAHKAN KENDALI KE PLAYER
        // ==========================================
        PlayerController.Instance.isCutsceneMode = false;
        Debug.Log("Cutscene Selesai! Selamat Bermain.");
    }

    private void SkipCutsceneImmediate()
    {
        if (cutsceneCanvas != null) cutsceneCanvas.SetActive(false);
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.isCutsceneMode = false;
        }
    }

    private IEnumerator FadeRoutine(CanvasGroup cg, float startAlpha, float endAlpha)
    {
        if (cg == null) yield break;
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / fadeDuration);
            yield return null;
        }
        cg.alpha = endAlpha;
    }
}
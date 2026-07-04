using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SupportUnlockNPC : MonoBehaviour
{
    [Header("Data Kameo Terkait")]
    [SerializeField] private SupportCharacterSO supportToUnlock;

    [Header("Pengaturan Dialog Cutscene")]
    [SerializeField] private string npcName = "Heider";
    [TextArea(2, 4)]
    [SerializeField] private List<string> dialogueLines;

    [Header("Komponen UI Dialog (Gunakan Canvas Cutscene/HUD)")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI nameTextUI;
    [SerializeField] private TextMeshProUGUI dialogueTextUI;
    [SerializeField] private GameObject interactPromptUI; // Tulisan "Tekan E untuk Bicara"

    [Header("UI Pop-up Reward")]
    [SerializeField] private GameObject rewardPanel; // Panel "Kameo Baru Didapatkan!"
    [SerializeField] private TextMeshProUGUI rewardTextUI;

    private bool isPlayerNear = false;
    private bool isCutsceneActive = false;
    private int currentLineIndex = 0;
    private string saveKey;

    void Start()
    {
        if (supportToUnlock == null) return;

        saveKey = "NPC_Unlocked_Event_" + supportToUnlock.characterName;

        // Cek apakah event dialog dengan NPC ini sudah pernah selesai di save data sebelumnya
        if (PlayerPrefs.GetInt(saveKey, 0) == 1)
        {
            // Jika sudah selesai (Continue game), matikan NPC agar tidak muncul lagi
            gameObject.SetActive(false);
        }
        else
        {
            // Jika belum selesai (New game / belum diambil), pastikan NPC AKTIF di peta!
            gameObject.SetActive(true);
        }

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (interactPromptUI != null) interactPromptUI.SetActive(false);
        if (rewardPanel != null) rewardPanel.SetActive(false);
    }

    void Update()
    {
        if (PlayerPrefs.GetInt(saveKey, 0) == 1) return; // Abaikan jika sudah kelar

        // Triger Mulai Dialog
        if (isPlayerNear && !isCutsceneActive && Input.GetButtonDown("Interact"))
        {
            StartCutsceneDialogue();
        }
        // Navigasi Lanjut Dialog
        else if (isCutsceneActive && (Input.GetButtonUp("Jump") || Input.GetButtonDown("Interact") || Input.GetButtonDown("Attack")))
        {
            AdvanceDialogue();
        }
    }

    private void StartCutsceneDialogue()
    {
        isCutsceneActive = true;
        currentLineIndex = 0;

        if (interactPromptUI != null) interactPromptUI.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        // Bekukan pergerakan fisik & kontrol Player
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.enabled = false;
            Rigidbody2D playerRb = PlayerController.Instance.GetComponent<Rigidbody2D>();
            if (playerRb != null) playerRb.linearVelocity = new Vector2(0f, playerRb.linearVelocity.y);
        }

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (nameTextUI != null) nameTextUI.text = npcName;
        if (dialogueTextUI != null) dialogueTextUI.text = dialogueLines[currentLineIndex];
    }

    private void AdvanceDialogue()
    {
        currentLineIndex++;

        if (currentLineIndex < dialogueLines.Count)
        {
            ShowCurrentLine();
        }
        else
        {
            // Dialog Selesai! Saatnya Berikan Reward Kameo
            StartCoroutine(GiveSupportRewardRoutine());
        }
    }

    private IEnumerator GiveSupportRewardRoutine()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        // 1. Eksekusi Unlock di System Utama
        if (supportToUnlock != null)
        {
            SupportManager.Instance.UnlockCharacter(supportToUnlock.characterName);

            // Tandai bahwa event NPC ini sudah selesai secara permanen
            PlayerPrefs.SetInt(saveKey, 1);
            PlayerPrefs.Save();

            // 2. Tampilkan UI Pop-up Reward yang Keren
            if (rewardPanel != null)
            {
                if (rewardTextUI != null)
                    rewardTextUI.text = $"Kameo Berhasil Didapatkan:\n<color=#FFD700>{supportToUnlock.characterName}</color> telah bergabung!";

                rewardPanel.SetActive(true);

                // Tunggu beberapa saat agar player bisa membaca pop-up rewardnya
                yield return new WaitForSecondsRealtime(2.5f);
                rewardPanel.SetActive(false);
            }
        }

        // Kembalikan kendali kontrol ke Player
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.enabled = true;
        }

        isCutsceneActive = false;

        // Hilangkan NPC dari peta karena dia sudah resmi ikut bertualang dalam bentuk Kameo
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && PlayerPrefs.GetInt(saveKey, 0) == 0)
        {
            isPlayerNear = true;
            if (interactPromptUI != null && !isCutsceneActive) interactPromptUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (interactPromptUI != null) interactPromptUI.SetActive(false);
        }
    }
}
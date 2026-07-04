using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Checkpoint : MonoBehaviour
{
    [Header("Visual & UI")]
    [SerializeField] private GameObject interactUI;
    [SerializeField] private GameObject checkpointMenuUI;
    [SerializeField] private GameObject saveEffectPrefab;

    // === BARU: Referensi untuk Panel Menu Support ===
    [Header("Sub-Menus")]
    [SerializeField] private GameObject supportMenuUI; // Tarik 'RestMenuPanel' ke sini di Inspector

    [Header("Soulslike Settings")]
    [SerializeField] private float restDelay = 1.2f;

    private bool isPlayerNear = false;
    private bool isMenuOpen = false;

    void Start()
    {
        if (interactUI != null) interactUI.SetActive(false);
        if (checkpointMenuUI != null) checkpointMenuUI.SetActive(false);

        // Pastikan menu support juga tertutup saat game dimulai
        if (supportMenuUI != null) supportMenuUI.SetActive(false);
    }

    void Update()
    {
        if (isPlayerNear && Input.GetButtonDown("Interact") && !isMenuOpen)
        {
            OpenMenu();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNear = true;
            if (interactUI != null && !isMenuOpen) interactUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (interactUI != null) interactUI.SetActive(false);
            CloseMenu();
        }
    }

    private void OpenMenu()
    {
        isMenuOpen = true;
        if (interactUI != null) interactUI.SetActive(false);
        if (checkpointMenuUI != null) checkpointMenuUI.SetActive(true);

        Time.timeScale = 0f;
    }

    public void CloseMenu()
    {
        isMenuOpen = false;
        if (checkpointMenuUI != null) checkpointMenuUI.SetActive(false);
        if (supportMenuUI != null) supportMenuUI.SetActive(false); // Tutup juga menu support jika sedang buka

        Time.timeScale = 1f;
    }

    // ==========================================
    // FUNGSI-FUNGSI UNTUK TOMBOL DI UI KELAK
    // ==========================================

    public void RestButton()
    {
        CloseMenu();
        StartCoroutine(RestAndSaveRoutine());
    }

    public void LeaveButton()
    {
        CloseMenu();
    }

    // === DIUBAH: Fungsi ini sekarang membuka sub-menu Support ===
    public void SupportButton()
    {
        // Sembunyikan menu utama checkpoint
        if (checkpointMenuUI != null) checkpointMenuUI.SetActive(false);

        // Buka panel menu pilih support
        if (supportMenuUI != null) supportMenuUI.SetActive(true);

        Debug.Log("Menu Pilih Support Dibuka!");
    }

    // === BARU: Fungsi untuk tombol "Kembali" di menu Support ===
    public void BackToMainMenu()
    {
        // Tutup panel menu support
        if (supportMenuUI != null) supportMenuUI.SetActive(false);

        // Buka kembali menu utama checkpoint
        if (checkpointMenuUI != null) checkpointMenuUI.SetActive(true);
    }

    // ==========================================

    private IEnumerator RestAndSaveRoutine()
    {
        // 1. OTOMATIS SAVE POSISI
        PlayerPrefs.SetFloat("PlayerPosX", transform.position.x);
        PlayerPrefs.SetFloat("PlayerPosY", transform.position.y);
        PlayerPrefs.SetInt("HasSaveData", 1);
        PlayerPrefs.Save();
        Debug.Log("Game Saved!");

        // 2. SPAWN EFEK PREFAB
        if (saveEffectPrefab != null)
        {
            Instantiate(saveEffectPrefab, transform.position, Quaternion.identity);
        }

        // 3. RESTORE HP & MANA PLAYER
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.health = PlayerController.Instance.maxHealth;
            PlayerController.Instance.currentEnergy = PlayerController.Instance.maxEnergy;
        }

        // 4. JEDA TIMING 
        yield return new WaitForSecondsRealtime(restDelay); // Gunakan WaitForSecondsRealtime karena timeScale = 0

        // 5. RELOAD SCENE 
        Time.timeScale = 1f; // Pastikan timeScale kembali ke 1 sebelum reload
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
using UnityEngine;
using UnityEngine.SceneManagement; // Diperlukan untuk pindah scene

public class PauseMenuManager : MonoBehaviour
{
    // Singleton agar bisa diakses dengan mudah jika diperlukan
    public static PauseMenuManager Instance { get; private set; }

    [Header("UI Components")]
    [SerializeField] private GameObject pauseMenuPanel; // Tarik Panel Pause Menu ke sini
    [SerializeField] private GameObject settingsPanel;  // Tarik Panel Settings ke sini (jika ada)

    private bool isPaused = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Pastikan panel menu tertutup saat awal game mulai
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void Update()
    {
        // Mengambil input dari Input Manager dengan nama "Pause"
        if (Input.GetButtonDown("pause"))
        {
            if (isPaused)
            {
                ContinueGame();
            }
            else
            {
                OpenPauseMenu();
            }
        }
    }

    public void OpenPauseMenu()
    {
        // SEKARANG MENGGUNAKAN FUNGSI BARU: Hanya batalkan pause jika panel game over benar-benar muncul
        if (GameOverManager.Instance != null && GameOverManager.Instance.IsGameOverActive()) return;

        isPaused = true;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    // 2. FUNGSI UNTUK TOMBOL CONTINUE
    public void ContinueGame()
    {
        isPaused = false;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false); // Pastikan panel setting ikut tertutup

        // Kembalikan waktu game menjadi normal berjalan
        Time.timeScale = 1f;
    }

    // 3. FUNGSI UNTUK TOMBOL SETTING (Buka/Tutup Sub-Panel Setting)
    public void OpenSettings(bool open)
    {
        if (pauseMenuPanel != null && settingsPanel != null)
        {
            pauseMenuPanel.SetActive(!open); // Sembunyikan pause menu jika setting dibuka, dan sebaliknya
            settingsPanel.SetActive(open);
        }
    }

    // 4. FUNGSI UNTUK TOMBOL QUIT TO MAIN MENU
    public void QuitToMainMenu()
    {
        // WAJIB kembalikan waktu menjadi normal sebelum pindah scene, 
        // jika tidak, Main Menu Anda akan ikut nge-freeze/macet!
        Time.timeScale = 1f;

        // Kembali ke Main Menu (Index 0 di Build Settings)
        SceneManager.LoadScene(0);
    }
}
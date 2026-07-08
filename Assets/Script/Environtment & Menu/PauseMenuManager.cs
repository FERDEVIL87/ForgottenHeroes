using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance { get; private set; }

    [Header("UI Components")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    private bool isPaused = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetButtonDown("Pause"))
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
        if (GameOverManager.Instance != null && GameOverManager.Instance.IsGameOverActive()) return;

        isPaused = true;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void ContinueGame()
    {
        isPaused = false;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    // =================================================================
    // 🛠️ FUNGSI BARU: TANPA PARAMETER (ANTI-BUG & LEBIH MUDAH DI-SETUP)
    // =================================================================

    // 1. PASANG FUNGSI INI DI "TOMBOL SETTING"
    public void OpenSettingsMenu()
    {
        Debug.Log("Tombol Setting Berhasil Diklik!"); // Cek di jendela Console Unity

        if (pauseMenuPanel != null && settingsPanel != null)
        {
            pauseMenuPanel.SetActive(false); // Sembunyikan menu pause utama
            settingsPanel.SetActive(true);   // Munculkan menu setting
        }
        else
        {
            Debug.LogError("Gagal! Ada panel yang belum Anda masukkan ke Inspector PauseMenuManager!");
        }
    }

    // 2. PASANG FUNGSI INI DI "TOMBOL BACK" (DI DALAM PANEL SETTING)
    public void CloseSettingsMenu()
    {
        Debug.Log("Tombol Back Berhasil Diklik!"); // Cek di jendela Console Unity

        if (pauseMenuPanel != null && settingsPanel != null)
        {
            pauseMenuPanel.SetActive(true);   // Munculkan kembali menu pause utama
            settingsPanel.SetActive(false);  // Sembunyikan menu setting
        }
    }

    // =================================================================

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}
using UnityEngine;
using UnityEngine.SceneManagement; // Diperlukan untuk berpindah scene

public class GameOverManager : MonoBehaviour
{
    // === SINGLETON ===
    public static GameOverManager Instance { get; private set; }

    [Header("UI Component")]
    [SerializeField] private GameObject gameOverPanel; // Tarik Panel Game Over dari Canvas ke sini

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Pastikan panel Game Over tidak muncul di awal permainan
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    // Fungsi yang akan dipanggil saat Player mati
    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            // Menghentikan waktu game (freeze) agar musuh/efek berhenti bergerak
            Time.timeScale = 0f;
        }
    }

    // Fungsi untuk Tombol RESTART
    public void RestartGame()
    {
        // Kembalikan waktu normal sebelum me-reload scene
        Time.timeScale = 1f;

        // Mengulang Scene Game yang sedang aktif saat ini
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Fungsi untuk Tombol QUIT TO MAIN MENU
    public void QuitToMainMenu()
    {
        // Kembalikan waktu normal sebelum kembali ke menu
        Time.timeScale = 1f;

        // PERUBAHAN: Diubah menjadi indeks 0 sesuai struktur di MainMenu.cs Anda
        SceneManager.LoadScene(0);
    }
}
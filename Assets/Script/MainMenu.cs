using UnityEngine;
using UnityEngine.SceneManagement; // Wajib diimport untuk mengatur pindah scene
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Buttons")]
    [SerializeField] private Button continueButton;

    private void Start()
    {
        // Pastikan saat game mulai, menu utama aktif dan menu setting mati
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // FITUR CONTINUE: Cek apakah ada data save-an sebelumnya
        // Di sini kita pakai contoh PlayerPrefs. Jika ada data "HasSaveData", tombol Continue aktif.
        if (continueButton != null)
        {
            if (PlayerPrefs.HasKey("HasSaveData"))
            {
                continueButton.interactable = true; // Tombol bisa diklik
            }
            else
            {
                continueButton.interactable = false; // Tombol buram & tidak bisa diklik
            }
        }
    }

    // 1. FUNGSI PLAY (Mulai Game Baru)
    public void PlayGame()
    {
        // Hapus data lama jika player memilih main dari awal lagi
        PlayerPrefs.DeleteKey("HasSaveData");

        // Load scene game utama (Index 1 di Build Settings)
        SceneManager.LoadScene(1);
    }

    // 2. FUNGSI CONTINUE (Melanjutkan Game)
    public void ContinueGame()
    {
        // Load scene game utama, nanti di scene tersebut taruh logika untuk memuat posisi terakhir
        SceneManager.LoadScene(1);
    }

    // 3. FUNGSI SETTING (Buka/Tutup Panel Setting)
    public void OpenSettings(bool open)
    {
        if (mainMenuPanel != null && settingsPanel != null)
        {
            mainMenuPanel.SetActive(!open); // Sembunyikan/munculkan menu utama
            settingsPanel.SetActive(open);    // Munculkan/sembunyikan menu setting
        }
    }

    // 4. FUNGSI EXIT (Keluar dari Game)
    public void ExitGame()
    {
        Debug.Log("Player keluar dari game!");
        Application.Quit(); // Fungsi ini bekerja setelah game di-build (.exe / .apk)
    }
}
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
        // === KODE BERSIH-BERSIH ASLI ANDA ===
        PlayerPrefs.DeleteKey("Unlocked_Support_Heider");
        PlayerPrefs.DeleteKey("NPC_Unlocked_Event_Heider");
        PlayerPrefs.DeleteKey("SavedSupportName");
        PlayerPrefs.DeleteKey("SavedSkillName");
        PlayerPrefs.DeleteKey("HasSaveData");

        // =====================================================================
        // TAMBAHAN BARU: BERSIHKAN DATA TRANSISI LEVEL & POSISI LAMA
        // =====================================================================
        PlayerPrefs.DeleteKey("SavedHP");
        PlayerPrefs.DeleteKey("SavedEnergy");
        PlayerPrefs.DeleteKey("PlayerPosX"); // Bersihkan sisa koordinat checkpoint lama
        PlayerPrefs.DeleteKey("PlayerPosY");
        // =====================================================================

        PlayerPrefs.Save();
        Debug.Log("[NEW GAME] Semua data Kameo, Event, HP, dan Energy berhasil dibersihkan!");

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
    public void QuitGame()
    {
        Debug.Log("Game Ditutup!"); // Hanya muncul di console Unity Editor
        Application.Quit();         // Menutup game saat sudah dicompile (.exe / .apk)
    }
}
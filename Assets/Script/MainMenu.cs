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
        // =====================================================================
        // PERBAIKAN UTAMA: RESET DATA LANGSUNG VIA PLAYERPREFS DI MAIN MENU
        // =====================================================================
        // 1. Hapus tanda bukti unlock karakter Heider agar terkunci kembali
        PlayerPrefs.DeleteKey("Unlocked_Support_Heider");

        // 2. Hapus flag event dialog NPC Heider agar dia muncul kembali di peta goa
        PlayerPrefs.DeleteKey("NPC_Unlocked_Event_Heider");

        // 3. Kosongkan slot kameo yang sedang dipakai agar tidak otomatis terpasang
        PlayerPrefs.DeleteKey("SavedSupportName");
        PlayerPrefs.DeleteKey("SavedSkillName");

        // 4. Hapus flag global data save utama
        PlayerPrefs.DeleteKey("HasSaveData");

        // Jika Anda memiliki karakter kameo selain Heider di masa depan, 
        // tinggal tambahkan baris DeleteKey untuk nama karakter tersebut di sini.
        // Atau jika ingin menghapus seluruh data game tanpa sisa, gunakan: PlayerPrefs.DeleteAll();

        PlayerPrefs.Save();
        Debug.Log("[NEW GAME] Semua data Kameo Heider dan Event NPC berhasil dibersihkan dari Main Menu!");

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
using UnityEngine;
using UnityEngine.UI;

public class ManaLogoController : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private Image manaLogoImage; // Masukkan Gambar Logo UI (tipe Filled) ke sini

    void Start()
    {
        // Berlangganan ke event mana milik player agar update otomatis
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.onManaChangedCallback += UpdateManaUI;
            UpdateManaUI(); // Update tampilan di awal game
        }
    }

    void UpdateManaUI()
    {
        if (PlayerController.Instance == null || manaLogoImage == null) return;

        // Hitung rasio (misal: 20/100 = 0.2f)
        float manaRatio = (float)PlayerController.Instance.currentEnergy / PlayerController.Instance.maxEnergy;

        // Mengubah porsi "isi" gambar agar naik seiring bertambahnya energi
        manaLogoImage.fillAmount = manaRatio;
    }

    private void OnDestroy()
    {
        // Bersihkan langganan saat UI/scene dihancurkan untuk mencegah error
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.onManaChangedCallback -= UpdateManaUI;
        }
    }
}
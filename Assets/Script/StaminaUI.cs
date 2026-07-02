using UnityEngine;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour
{
    [SerializeField] private Image staminaFillImage; // Tarik objek Image Fill berwarna Hijau ke sini

    void Start()
    {
        if (PlayerController.Instance != null)
        {
            // Berlangganan ke event callback di PlayerController
            PlayerController.Instance.onStaminaChangedCallback += UpdateStaminaBar;
            UpdateStaminaBar();
        }
    }

    private void UpdateStaminaBar()
    {
        if (PlayerController.Instance != null && staminaFillImage != null)
        { // <--- INI YANG TADI KURANG
            // Mengatur panjang bar berdasarkan persentase stamina saat ini (0.0f - 1.0f)
            staminaFillImage.fillAmount = PlayerController.Instance.currentStamina / PlayerController.Instance.maxStamina;
        }
    }

    private void OnDestroy()
    {
        // Bersihkan memori saat ganti scene
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.onStaminaChangedCallback -= UpdateStaminaBar;
        }
    }
}
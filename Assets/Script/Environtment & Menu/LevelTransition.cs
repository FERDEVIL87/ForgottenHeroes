using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransition : MonoBehaviour
{
    [Header("Pengaturan Scene")]
    [SerializeField] private string nextSceneName;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                PlayerPrefs.SetInt("SavedHP", player.health);
                PlayerPrefs.SetInt("SavedEnergy", player.currentEnergy);

                PlayerPrefs.DeleteKey("PlayerPosX");
                PlayerPrefs.DeleteKey("PlayerPosY");
                PlayerPrefs.Save();
            }

            // =========================================================
            // FIX BUG SLOW-MO: Kembalikan waktu normal sebelum pindah!
            // =========================================================
            Time.timeScale = 1f;

            SceneManager.LoadScene(nextSceneName);
        }
    }
}
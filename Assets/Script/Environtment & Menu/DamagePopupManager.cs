using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance { get; private set; }

    [Header("Prefab Settings")]
    [SerializeField] private GameObject damagePopupPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void CreatePopup(Vector3 spawnPosition, float damageAmount, Color textColor)
    {
        if (damagePopupPrefab == null)
        {
            Debug.LogError("Waduh! Prefab DamagePopup belum dimasukkan ke Inspector Manager!");
            return;
        }

        // === BARIS DIBAWAH INI UNTUK TES ===
        Debug.Log($"[POPUP SYSTEM] Memunculkan angka {damageAmount} di posisi {spawnPosition}");

        Vector3 randomOffset = new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(0.2f, 0.5f), 0f);
        GameObject popupObj = Instantiate(damagePopupPrefab, spawnPosition + randomOffset, Quaternion.identity);
        DamagePopup popupScript = popupObj.GetComponent<DamagePopup>();

        if (popupScript != null)
        {
            popupScript.Setup(damageAmount, textColor);
        }
    }
}
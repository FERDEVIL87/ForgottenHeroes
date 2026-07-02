using System.Collections;
using UnityEngine;

public class SupportManager : MonoBehaviour
{
    // === SINGLETON ===
    public static SupportManager Instance { get; private set; }

    [Header("Current Equipped")]
    // Tipe data disesuaikan dengan ScriptableObject aslimu (SupportCharacterSO & SupportSkillSO)
    public SupportCharacterSO equippedSupport;
    public SupportSkillSO equippedSkill;

    [Header("Cost Settings")]
    [Tooltip("Jumlah Mana/Energy yang dibutuhkan untuk summon")]
    public int summonEnergyCost = 25; // Variabel untuk mengatur harga mana

    private bool isCallingSupport = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // === FUNGSI BARU YANG SUDAH DISESUAIKAN DAN DIGABUNGKAN ===
    public void CallSupportWithPosition(Vector3 spawnPos, int faceDir)
    {
        // Mencegah spam jika Heider masih berada di arena / masih dalam cooldown
        if (isCallingSupport) return;

        // Cek apakah data Support dan Prefab-nya sudah dimasukkan di Inspector
        if (equippedSupport != null && equippedSupport.supportPrefab != null)
        {
            // =========================================================
            // === LOGIKA BARU: CEK DAN POTONG MANA SEBELUM MUNCUL ===
            // =========================================================
            if (PlayerController.Instance != null)
            {
                if (PlayerController.Instance.currentEnergy < summonEnergyCost)
                {
                    Debug.LogWarning("GAGAL: Mana/Energy tidak cukup!");
                    return; // Batalkan summon kalau mana tidak cukup
                }

                // Potong Mana/Energy Player
                PlayerController.Instance.currentEnergy -= summonEnergyCost;

                // Update UI Bar Mana langsung
                if (PlayerController.Instance.onManaChangedCallback != null)
                {
                    PlayerController.Instance.onManaChangedCallback.Invoke();
                }
            }
            // =========================================================

            isCallingSupport = true;

            // 1. Spawn prefab Heider di posisi modular yang sudah dihitung oleh Player
            GameObject spawnedSupport = Instantiate(equippedSupport.supportPrefab, spawnPos, Quaternion.identity);

            // 2. Ambil script HeiderSupport dari objek yang baru lahir tersebut
            HeiderSupport heiderScript = spawnedSupport.GetComponent<HeiderSupport>();

            if (heiderScript != null)
            {
                // 3. Kirim data arah hadap (0 = Kanan, 1 = Kiri) ke Heider agar animasinya pas
                heiderScript.faceDir = faceDir;
            }

            // Jalankan cooldown agar setelah Heider hilang, tombol baru bisa ditekan lagi
            StartCoroutine(ResetCooldownRoutine());
        }
        else
        {
            Debug.LogWarning("Gagal memanggil Kameo: Data Equipped Support atau Prefab-nya masih kosong!");
        }
    }

    // Coroutine untuk mereset status panggilan (Cooldown)
    private IEnumerator ResetCooldownRoutine()
    {
        // Beri jeda 1.2 detik (sesuai total durasi Heider muncul sampai hilang) sebelum bisa dipanggil lagi
        yield return new WaitForSeconds(1.2f);
        isCallingSupport = false;
    }
}
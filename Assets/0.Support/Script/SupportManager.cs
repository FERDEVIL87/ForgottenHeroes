using System.Collections;
using System.Collections.Generic; // Dibutuhkan untuk menggunakan List database
using UnityEngine;

public class SupportManager : MonoBehaviour
{
    // === SINGLETON ===
    public static SupportManager Instance { get; private set; }

    [Header("Current Equipped")]
    public SupportCharacterSO equippedSupport;
    public SupportSkillSO equippedSkill;

    [Header("Cost Settings")]
    [Tooltip("Jumlah Mana/Energy yang dibutuhkan untuk summon")]
    public int summonEnergyCost = 25;

    [Header("Database All Supports")]
    [Tooltip("Masukkan semua ScriptableObject SupportCharacterSO game Anda ke sini via Inspector")]
    public List<SupportCharacterSO> allSupportDatabase;

    private bool isCallingSupport = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Muat data di Awake agar siap SEBELUM UI (OnEnable) berjalan
        LoadEquippedSupport();
    }

    // Fungsi Start() dikosongkan karena logika load sudah pindah ke Awake
    private void Start()
    {
    }

    // Fungsi pendukung untuk unlock karakter dari NPC
    public void UnlockCharacter(string characterName)
    {
        PlayerPrefs.SetInt("Unlocked_Support_" + characterName, 1);
        PlayerPrefs.Save();
        Debug.Log($"[UNLOCK SUCCESS] Karakter {characterName} sekarang tersedia!");
    }

    // Fungsi pengecekan status unlock karakter
    public bool IsCharacterUnlocked(string characterName)
    {
        return PlayerPrefs.GetInt("Unlocked_Support_" + characterName, 0) == 1;
    }

    // Fungsi pembersihan data untuk Tombol New Game
    public void ResetAllSupportDataForNewGame()
    {
        foreach (SupportCharacterSO character in allSupportDatabase)
        {
            if (character != null)
            {
                PlayerPrefs.DeleteKey("Unlocked_Support_" + character.characterName);
                PlayerPrefs.DeleteKey("NPC_Unlocked_Event_" + character.characterName);
            }
        }

        PlayerPrefs.DeleteKey("SavedSupportName");
        PlayerPrefs.DeleteKey("SavedSkillName");
        PlayerPrefs.DeleteKey("HasSaveData");

        equippedSupport = null;
        equippedSkill = null;

        PlayerPrefs.Save();
        Debug.Log("[RESET SUCCESS] Semua data Kameo berhasil dibersihkan untuk New Game!");
    }

    public void CallSupportWithPosition(Vector3 spawnPos, int faceDir)
    {
        if (isCallingSupport) return;

        if (equippedSupport != null && equippedSupport.supportPrefab != null)
        {
            if (PlayerController.Instance != null)
            {
                if (!PlayerController.Instance.HasEnoughStamina(summonEnergyCost))
                {
                    Debug.LogWarning("Stamina/Mana tidak cukup untuk memanggil Kameo!");
                    return;
                }
                PlayerController.Instance.UseStamina(summonEnergyCost);

                if (PlayerController.Instance.onManaChangedCallback != null)
                {
                    PlayerController.Instance.onManaChangedCallback.Invoke();
                }
            }

            isCallingSupport = true;

            GameObject spawnedSupport = Instantiate(equippedSupport.supportPrefab, spawnPos, Quaternion.identity);
            SupportBase supportScript = spawnedSupport.GetComponent<SupportBase>();

            if (supportScript != null)
            {
                supportScript.Init(faceDir, equippedSkill);
            }

            StartCoroutine(ResetCooldownRoutine());
        }
        else
        {
            Debug.LogWarning("Gagal memanggil Kameo: Data Equipped Support atau Prefab-nya masih kosong!");
        }
    }

    private IEnumerator ResetCooldownRoutine()
    {
        yield return new WaitForSeconds(1.2f);
        isCallingSupport = false;
    }

    public void SaveEquippedSupport()
    {
        if (equippedSupport != null)
        {
            PlayerPrefs.SetString("SavedSupportName", equippedSupport.characterName);

            // PERBAIKAN: Buat flag HasSaveData di sini agar sistem tahu game ini sudah memiliki data aktif
            PlayerPrefs.SetInt("HasSaveData", 1);
        }
        else
        {
            PlayerPrefs.DeleteKey("SavedSupportName");
        }

        if (equippedSkill != null)
            PlayerPrefs.SetString("SavedSkillName", equippedSkill.skillName);
        else
            PlayerPrefs.DeleteKey("SavedSkillName");

        PlayerPrefs.Save();
        Debug.Log($"[SAVE SUCCESS] Karakter: {(equippedSupport != null ? equippedSupport.characterName : "Null")}, Skill: {(equippedSkill != null ? equippedSkill.skillName : "Null")}");
    }

    public void LoadEquippedSupport()
    {
        // =====================================================================
        // PERBAIKAN UTAMA: Baris 'if (!PlayerPrefs.HasKey("HasSaveData")) return;' 
        // TELAH DIHAPUS agar system tidak mengosongkan slot memory saat keluar menu api unggun.
        // =====================================================================

        string savedCharName = PlayerPrefs.GetString("SavedSupportName", "");
        string savedSkillName = PlayerPrefs.GetString("SavedSkillName", "");

        if (string.IsNullOrEmpty(savedCharName))
        {
            equippedSupport = null;
            equippedSkill = null;
            return;
        }

        SupportCharacterSO foundChar = allSupportDatabase.Find(c => c.characterName == savedCharName);
        if (foundChar != null)
        {
            equippedSupport = foundChar;

            if (foundChar.availableSkills != null && !string.IsNullOrEmpty(savedSkillName))
            {
                bool skillFound = false;
                foreach (SupportSkillSO sk in foundChar.availableSkills)
                {
                    if (sk.skillName == savedSkillName)
                    {
                        equippedSkill = sk;
                        skillFound = true;
                        break;
                    }
                }
                if (!skillFound) equippedSkill = null;
            }
            else
            {
                equippedSkill = null;
            }
        }
        else
        {
            equippedSupport = null;
            equippedSkill = null;
        }

        Debug.Log($"[LOAD SUCCESS] Berhasil memuat dari Save Data -> Karakter: {(equippedSupport != null ? equippedSupport.characterName : "Null")}, Skill: {(equippedSkill != null ? equippedSkill.skillName : "Null")}");
    }
}
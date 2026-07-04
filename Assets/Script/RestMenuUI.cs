using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RestMenuUI : MonoBehaviour
{
    [Header("Database")]
    [Tooltip("Masukkan semua data karakter (Heider, dll) ke dalam list ini")]
    public List<SupportCharacterSO> allCharacters;

    [Header("Character UI")]
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI charEquipBtnText;

    [Header("Skill UI")]
    public GameObject skillPanel;
    public TextMeshProUGUI skillNameText;
    public TextMeshProUGUI skillEquipBtnText;

    private int charIndex = 0;
    private int skillIndex = 0;

    private void OnEnable()
    {
        // =====================================================================
        // FILTER MODULAR: Hanya tampilkan karakter yang sudah di-unlock player
        // =====================================================================
        if (SupportManager.Instance != null)
        {
            // Ambil database mentah dari SupportManager, lalu filter ke list lokal UI
            // Catatan: Pastikan di Inspector RestMenuUI, list 'allCharacters' dikosongkan/biarkan diisi otomatis oleh kode ini
            allCharacters = SupportManager.Instance.allSupportDatabase.FindAll(
                c => SupportManager.Instance.IsCharacterUnlocked(c.characterName)
            );
        }

        SyncUIWithEquippedData();
        UpdateUI();
    }

    // Fungsi untuk memaksa UI membuka halaman karakter/skill yang sedang dipakai player
    private void SyncUIWithEquippedData()
    {
        if (SupportManager.Instance == null) return;

        // 1. Cari tahu indeks karakter yang sedang dipakai
        if (SupportManager.Instance.equippedSupport != null)
        {
            int foundCharIndex = allCharacters.FindIndex(c => c == SupportManager.Instance.equippedSupport);
            if (foundCharIndex != -1)
            {
                charIndex = foundCharIndex;
            }
        }
        else
        {
            charIndex = 0;
        }

        // 2. Cari tahu indeks skill dari karakter tersebut yang sedang dipakai
        if (allCharacters.Count > 0 && charIndex < allCharacters.Count)
        {
            SupportCharacterSO selectedChar = allCharacters[charIndex];
            if (SupportManager.Instance.equippedSkill != null && selectedChar.availableSkills != null)
            {
                int foundSkillIndex = System.Array.IndexOf(selectedChar.availableSkills, SupportManager.Instance.equippedSkill);
                if (foundSkillIndex != -1)
                {
                    skillIndex = foundSkillIndex;
                }
                else
                {
                    skillIndex = 0;
                }
            }
            else
            {
                skillIndex = 0;
            }
        }
    }

    // ==========================================
    // NAVIGASI KARAKTER (KIRI / KANAN)
    // ==========================================
    public void NextCharacter()
    {
        if (allCharacters.Count == 0) return;
        charIndex = (charIndex + 1) % allCharacters.Count;
        skillIndex = 0;
        UpdateUI();
    }

    public void PrevCharacter()
    {
        if (allCharacters.Count == 0) return;
        charIndex = charIndex - 1;
        if (charIndex < 0) charIndex = allCharacters.Count - 1;
        skillIndex = 0;
        UpdateUI();
    }

    // ==========================================
    // NAVIGASI SKILL (KIRI / KANAN)
    // ==========================================
    public void NextSkill()
    {
        if (allCharacters.Count == 0) return;
        SupportCharacterSO selectedChar = allCharacters[charIndex];

        if (selectedChar.availableSkills != null && selectedChar.availableSkills.Length > 0)
        {
            skillIndex = (skillIndex + 1) % selectedChar.availableSkills.Length;
            UpdateUI();
        }
    }

    public void PrevSkill()
    {
        if (allCharacters.Count == 0) return;
        SupportCharacterSO selectedChar = allCharacters[charIndex];

        if (selectedChar.availableSkills != null && selectedChar.availableSkills.Length > 0)
        {
            skillIndex = skillIndex - 1;
            if (skillIndex < 0) skillIndex = selectedChar.availableSkills.Length - 1;
            UpdateUI();
        }
    }

    // ==========================================
    // TOMBOL EQUIP / UNEQUIP
    // ==========================================
    public void ToggleEquipCharacter()
    {
        if (allCharacters.Count == 0) return;
        SupportCharacterSO selectedChar = allCharacters[charIndex];

        if (SupportManager.Instance.equippedSupport == selectedChar)
        {
            SupportManager.Instance.equippedSupport = null;
            SupportManager.Instance.equippedSkill = null;
        }
        else
        {
            SupportManager.Instance.equippedSupport = selectedChar;
            if (selectedChar.availableSkills != null && selectedChar.availableSkills.Length > 0)
            {
                SupportManager.Instance.equippedSkill = selectedChar.availableSkills[0];
                skillIndex = 0;
            }
            else
            {
                SupportManager.Instance.equippedSkill = null;
            }
        }

        SupportManager.Instance.SaveEquippedSupport();
        UpdateUI();
    }

    public void ToggleEquipSkill()
    {
        if (allCharacters.Count == 0) return;
        SupportCharacterSO selectedChar = allCharacters[charIndex];

        if (SupportManager.Instance.equippedSupport != selectedChar) return;

        if (selectedChar.availableSkills == null || selectedChar.availableSkills.Length == 0) return;
        SupportSkillSO selectedSkill = selectedChar.availableSkills[skillIndex];

        if (SupportManager.Instance.equippedSkill == selectedSkill)
        {
            SupportManager.Instance.equippedSkill = null;
        }
        else
        {
            SupportManager.Instance.equippedSkill = selectedSkill;
        }

        SupportManager.Instance.SaveEquippedSupport();
        UpdateUI();
    }

    // ==========================================
    // UPDATE TAMPILAN VISUAL (INTI LOGIKA)
    // ==========================================
    private void UpdateUI()
    {
        if (allCharacters.Count == 0) return;

        SupportCharacterSO selectedChar = allCharacters[charIndex];

        characterNameText.text = selectedChar.characterName;

        bool isCharEquipped = (SupportManager.Instance.equippedSupport == selectedChar);
        charEquipBtnText.text = isCharEquipped ? "Unequip" : "Equip";

        if (isCharEquipped)
        {
            skillPanel.SetActive(true);

            if (selectedChar.availableSkills != null && selectedChar.availableSkills.Length > 0)
            {
                if (skillIndex >= selectedChar.availableSkills.Length) skillIndex = 0;

                SupportSkillSO selectedSkill = selectedChar.availableSkills[skillIndex];
                skillNameText.text = selectedSkill.skillName;

                bool isSkillEquipped = (SupportManager.Instance.equippedSkill == selectedSkill);
                skillEquipBtnText.text = isSkillEquipped ? "Unequip" : "Equip";
            }
            else
            {
                skillNameText.text = "Tidak Ada Skill";
                skillEquipBtnText.text = "-";
            }
        }
        else
        {
            skillPanel.SetActive(false);
        }
    }
}
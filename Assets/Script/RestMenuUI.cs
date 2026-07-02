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
        UpdateUI();
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
        SupportCharacterSO selectedChar = allCharacters[charIndex];

        // MENGGUNAKAN .Length KARENA INI ADALAH ARRAY
        if (selectedChar.availableSkills == null || selectedChar.availableSkills.Length == 0) return;

        skillIndex = (skillIndex + 1) % selectedChar.availableSkills.Length;
        UpdateUI();
    }

    public void PrevSkill()
    {
        SupportCharacterSO selectedChar = allCharacters[charIndex];

        // MENGGUNAKAN .Length KARENA INI ADALAH ARRAY
        if (selectedChar.availableSkills == null || selectedChar.availableSkills.Length == 0) return;

        skillIndex = skillIndex - 1;
        if (skillIndex < 0) skillIndex = selectedChar.availableSkills.Length - 1;
        UpdateUI();
    }

    // ==========================================
    // TOMBOL EQUIP / UNEQUIP
    // ==========================================
    public void ToggleEquipCharacter()
    {
        SupportCharacterSO selectedChar = allCharacters[charIndex];

        if (SupportManager.Instance.equippedSupport == selectedChar)
        {
            SupportManager.Instance.equippedSupport = null;
            SupportManager.Instance.equippedSkill = null;
        }
        else
        {
            SupportManager.Instance.equippedSupport = selectedChar;
            SupportManager.Instance.equippedSkill = null;
        }

        UpdateUI();
    }

    public void ToggleEquipSkill()
    {
        SupportCharacterSO selectedChar = allCharacters[charIndex];

        // MENGGUNAKAN .Length KARENA INI ADALAH ARRAY
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

        UpdateUI();
    }

    // ==========================================
    // UPDATE TAMPILAN VISUAL (INTI LOGIKA)
    // ==========================================
    private void UpdateUI()
    {
        if (allCharacters.Count == 0) return;

        SupportCharacterSO selectedChar = allCharacters[charIndex];

        characterNameText.text = selectedChar.name;

        bool isCharEquipped = (SupportManager.Instance.equippedSupport == selectedChar);
        charEquipBtnText.text = isCharEquipped ? "Unequip" : "Equip";

        if (isCharEquipped)
        {
            skillPanel.SetActive(true);

            // MENGGUNAKAN .Length KARENA INI ADALAH ARRAY
            if (selectedChar.availableSkills != null && selectedChar.availableSkills.Length > 0)
            {
                SupportSkillSO selectedSkill = selectedChar.availableSkills[skillIndex];
                skillNameText.text = selectedSkill.name;

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
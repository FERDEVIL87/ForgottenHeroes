using UnityEngine;

[CreateAssetMenu(fileName = "NewSupportCharacter", menuName = "Support System/Character")]
public class SupportCharacterSO : ScriptableObject
{
    public string characterName;
    public GameObject supportPrefab; // Prefab visual karakter support yang akan muncul (Kameo)
    public SupportSkillSO[] availableSkills; // Daftar skill yang dia punya
}
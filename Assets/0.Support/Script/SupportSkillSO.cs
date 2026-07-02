using UnityEngine;

[CreateAssetMenu(fileName = "NewSupportSkill", menuName = "Support System/Skill")]
public class SupportSkillSO : ScriptableObject
{
    public string skillName;
    public float gaugeCost; // Jumlah mana/energy yang dibutuhkan
    public string animationTriggerName; // Nama trigger di Animator karakter support
    public GameObject effectPrefab; // Prefab proyektil/efek serangan dari skill ini

    [Header("Audio Settings")]
    [Tooltip("Suara yang keluar saat skill ini aktif/ditembakkan")]
    public AudioClip skillSFX; // <--- BARU: Tempat input file audio SFX
}
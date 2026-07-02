using UnityEngine;

// Menentukan tipe mekanik skill Kameo
public enum SupportSkillType
{
    Projectile, // Seperti Energy Ball (Menembak dari jauh)
    Dash        // Seperti Skill Baru Heider (Menerjang ke depan)
}

[CreateAssetMenu(fileName = "NewSupportSkill", menuName = "Support System/Skill")]
public class SupportSkillSO : ScriptableObject
{
    public string skillName;
    public float gaugeCost; // Jumlah mana/energy yang dibutuhkan
    public string animationTriggerName; // Nama trigger di Animator karakter support
    public GameObject effectPrefab; // Prefab proyektil (Hanya untuk tipe Projectile)

    [Header("Audio Settings")]
    [Tooltip("Suara yang keluar saat skill ini aktif")]
    public AudioClip skillSFX;

    [Header("Skill Type Configuration")]
    public SupportSkillType skillType = SupportSkillType.Projectile; // Default-nya menembak

    [Header("Dash Attack Settings (Khusus Tipe Dash)")]
    [Tooltip("Kecepatan terjang karakter saat melakukan dash")]
    public float dashSpeed = 15f;
    [Tooltip("Berapa lama durasi terjang/dash berlangsung")]
    public float dashDuration = 0.3f;
    [Tooltip("Jumlah damage yang diberikan ke musuh sepanjang jalur dash")]
    public int dashDamage = 25;
    [Tooltip("Layer yang digunakan oleh musuh agar bisa terkena hit")]
    public LayerMask enemyLayer;
}
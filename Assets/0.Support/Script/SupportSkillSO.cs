using UnityEngine;

public enum SupportSkillType { Projectile, Dash }

[CreateAssetMenu(fileName = "NewSupportSkill", menuName = "Support System/Skill")]
public class SupportSkillSO : ScriptableObject
{
    [Header("Identitas & Validasi")]
    public string skillName;
    public float gaugeCost;
    public SupportCharacterSO ownerCharacter;

    [Header("Skill Type Configuration")]
    public SupportSkillType skillType = SupportSkillType.Projectile;

    [Header("Animasi, Efek, & Suara")]
    public string animationTriggerName;
    public GameObject effectPrefab;
    public AudioClip skillSFX;

    [Header("Targeting Musuh")]
    public LayerMask enemyLayer;
}
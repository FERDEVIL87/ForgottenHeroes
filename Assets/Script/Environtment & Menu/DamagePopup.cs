using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    private TextMeshPro textMesh;
    private float disappearTimer = 0.4f;
    private Color textColor;
    private Vector3 moveVector;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    public void Setup(float damageAmount, Color color)
    {
        // Membulatkan angka damage agar tidak ada desimal panjang
        textMesh.SetText(Mathf.RoundToInt(damageAmount).ToString());
        textColor = color;
        textMesh.color = textColor;

        // Memberikan arah acak sedikit ke kiri/kanan saat melayang ke atas
        moveVector = new Vector3(Random.Range(-0.6f, 0.6f), 1f, 0f).normalized * 3.5f;
    }

    private void Update()
    {
        // Membuat teks bergerak melayang
        transform.position += moveVector * Time.deltaTime;

        // Efek perlambatan gesekan udara agar gerakan lebih halus
        moveVector -= moveVector * 3f * Time.deltaTime;

        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            // Mulai memudarkan text (Fade Out)
            float fadeSpeed = 5f;
            textColor.a -= fadeSpeed * Time.deltaTime;
            textMesh.color = textColor;

            if (textColor.a <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
using UnityEngine;

public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Cek apakah objek yang masuk ke jurang adalah Player
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();

            if (player != null)
            {
                Debug.Log("Player jatuh ke jurang!");

                // Berikan damage yang sangat besar (misal 999) agar Player langsung mati
                // Arah recoil dibuat Vector2.zero agar player tidak terpental aneh di dalam jurang
                player.TakeDamage(999f, Vector2.zero, 0f);
            }
        }
    }
}
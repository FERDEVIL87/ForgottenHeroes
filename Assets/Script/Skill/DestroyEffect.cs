using UnityEngine;

public class DestroyEffect : MonoBehaviour
{
    [SerializeField] private float destroyTime = 0.5f; // Sesuaikan dengan panjang/durasi animasimu

    void Start()
    {
        // Menghancurkan efek ledakan ini otomatis setelah beberapa detik (sesuai destroyTime)
        Destroy(gameObject, destroyTime);
    }
}
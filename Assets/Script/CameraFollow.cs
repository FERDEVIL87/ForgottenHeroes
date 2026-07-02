using System.Collections;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // === SINGLETON ===
    public static CameraFollow Instance { get; private set; }

    private Camera cam; // Referensi komponen kamera

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Ambil komponen kamera secara otomatis
        cam = GetComponent<Camera>();
    }

    [Header("Target & Smoothness")]
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);

    // Kotak hijau sekarang merepresentasikan BATAS DINDING UTAMA MAP
    [Header("Camera Bounds (Batas Luar Map)")]
    [SerializeField] private bool useBounds = true;
    [SerializeField] private float minX;
    [SerializeField] private float maxX;
    [SerializeField] private float minY;
    [SerializeField] private float maxY;

    private Vector3 shakeOffset = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Hitung posisi ideal kamera mengikuti player
        Vector3 desiredPosition = new Vector3(target.position.x + offset.x, target.position.y + offset.y, -10f);

        // 2. Batasi menggunakan pinggiran kamera (bukan titik tengah lagi)
        if (useBounds && cam != null)
        {
            // Hitung setengah tinggi & lebar jangkauan kamera di dalam game
            float camHeight = cam.orthographicSize;
            float camWidth = cam.orthographicSize * cam.aspect;

            // Batasi posisi tengah kamera agar pinggirannya tidak tembus kotak batas luar map
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX + camWidth, maxX - camWidth);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY + camHeight, maxY - camHeight);
        }

        // 3. Tambahkan efek getar
        desiredPosition += shakeOffset;

        // 4. Lakukan pergerakan halus (Lerp)
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
    }

    public void Shake(float duration, float magnitude)
    {
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            shakeOffset = new Vector3(x, y, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        shakeOffset = Vector3.zero;
    }

    private void OnDrawGizmos()
    {
        if (!useBounds) return;

        Gizmos.color = Color.green;

        Vector3 topLeft = new Vector3(minX, maxY, 0);
        Vector3 topRight = new Vector3(maxX, maxY, 0);
        Vector3 bottomLeft = new Vector3(minX, minY, 0);
        Vector3 bottomRight = new Vector3(maxX, minY, 0);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
}
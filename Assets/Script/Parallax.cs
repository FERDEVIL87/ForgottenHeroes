using UnityEngine;

public class Parallax : MonoBehaviour
{
    [SerializeField] private float parallaxEffect = 0.5f;  // Semakin kecil = lebih lambat

    private Transform cam;
    private Vector3 lastCamPos;

    void Start()
    {
        cam = Camera.main.transform;
        lastCamPos = cam.position;
    }

    void LateUpdate()
    {
        Vector3 deltaMovement = cam.position - lastCamPos;
        transform.position += new Vector3(deltaMovement.x * parallaxEffect, 0, 0);
        lastCamPos = cam.position;
    }
}
using UnityEngine;

// Экран пішіні қандай болса да, деңгейдің ені толық көрінеді.
[RequireComponent(typeof(Camera))]
public class CameraFit : MonoBehaviour
{
    public float halfWidth = 9.2f;

    void Start()
    {
        Camera c = GetComponent<Camera>();
        c.orthographicSize = Mathf.Max(5f, halfWidth / c.aspect);
    }
}

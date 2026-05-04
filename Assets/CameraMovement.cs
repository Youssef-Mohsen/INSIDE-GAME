using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(19f, 6f, -0.3f);
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        Vector3 targetPosition = player.position + offset;
        
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );

        transform.rotation = Quaternion.Euler(10f, 180f, 0f);
    }
}
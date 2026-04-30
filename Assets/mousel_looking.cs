using UnityEngine;

public class mouse_looking : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(-8f, 3f, 0f);
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        Vector3 targetPosition = player.position + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );

        transform.rotation = Quaternion.Euler(10f, 90f, 0f);
    }
}
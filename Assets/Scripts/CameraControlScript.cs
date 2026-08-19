using UnityEngine;

public class CameraControlScript : MonoBehaviour
{
    public GameObject player;

    public float offsetX = -5f;
    public float offsetZ = 0f;
    public float offsetY = 3f;

    public float smoothTime = 0.3f;
        
    public float xRotation = 30f;
    public float yRotation = 30f;
    public float zRotation = 30f;
    
    [Header("Aim Look Ahead")]
    [SerializeField] private float aimOffsetDistance = 2.5f;
    [SerializeField] private float aimOffsetSmoothTime = 0.15f;

    private Vector3 velocity = Vector3.zero;
    private Vector3 currentAimOffset;
    private Vector3 aimOffsetVelocity;
    
    [Header("Shake")]
    [SerializeField] private float shakeDecay = 5f;
    private float shakeIntensity;

    void LateUpdate()
    {
        if (player is null) return;
            
        Vector3 targetPosition = new Vector3(player.transform.position.x + offsetX, player.transform.position.y + offsetY, player.transform.position.z + offsetZ);
        
        Vector3 aimDirection = player.transform.forward;
        aimDirection.y = 0f;

        if (aimDirection.sqrMagnitude > 0.01f)
        {
            aimDirection.Normalize();
        }

        Vector3 targetAimOffset = aimDirection * aimOffsetDistance;

        currentAimOffset = Vector3.SmoothDamp(currentAimOffset, targetAimOffset, ref aimOffsetVelocity, aimOffsetSmoothTime);

        targetPosition += currentAimOffset;

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        
        if (shakeIntensity > 0f)
        {
            Vector3 shakeOffset = Random.insideUnitSphere * shakeIntensity;
            shakeOffset.y = 0f; 
            transform.position += shakeOffset;

            shakeIntensity -= shakeDecay * Time.unscaledDeltaTime;
            if (shakeIntensity < 0f) shakeIntensity = 0f;
        }
            
        transform.rotation = Quaternion.Euler(xRotation, yRotation, zRotation);
    }
    
    public void Shake(float intensity)
    {
        if (intensity > shakeIntensity)
            shakeIntensity = intensity;
    }

    public void SnapToPlayer()
    {
        if (player == null) return;

        transform.position = new Vector3(
            player.transform.position.x + offsetX,
            player.transform.position.y + offsetY,
            player.transform.position.z + offsetZ);

        velocity = Vector3.zero;
    }
}

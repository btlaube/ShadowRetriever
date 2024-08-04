using UnityEngine;

public class RoomRespawnManager : MonoBehaviour
{
    public Transform player; // The player's Transform
    public Vector3 playerSpawnLocation;

    private Camera cameraComponent;
    private Vector3 playerPosition;

    void Start()
    {
        // Initialize camera component
        cameraComponent = GameObject.Find("Main Camera").GetComponent<Camera>();
    }

    void Update()
    {
        if (cameraComponent == null)
        {
            return;
        }

        // Update player position
        playerPosition = player.position;

        // Calculate the camera's pixel rect
        Rect pixelRect = cameraComponent.pixelRect;

        // Convert pixelRect to world space coordinates
        Vector3[] corners = new Vector3[4];
        corners[0] = cameraComponent.ScreenToWorldPoint(new Vector3(pixelRect.xMin, pixelRect.yMin, cameraComponent.nearClipPlane)); // Bottom-left
        corners[1] = cameraComponent.ScreenToWorldPoint(new Vector3(pixelRect.xMax, pixelRect.yMin, cameraComponent.nearClipPlane)); // Bottom-right
        corners[2] = cameraComponent.ScreenToWorldPoint(new Vector3(pixelRect.xMax, pixelRect.yMax, cameraComponent.nearClipPlane)); // Top-right
        corners[3] = cameraComponent.ScreenToWorldPoint(new Vector3(pixelRect.xMin, pixelRect.yMax, cameraComponent.nearClipPlane)); // Top-left

        // Define bounds
        float leftBorder = corners[0].x;
        float rightBorder = corners[1].x;
        float topBorder = corners[2].y;
        float bottomBorder = corners[0].y;

        // Check if the player is outside the camera view bounds
        if (playerPosition.x < leftBorder || playerPosition.x > rightBorder ||
            playerPosition.y < bottomBorder || playerPosition.y > topBorder)
        {
            // Player is outside the camera bounds, you can use playerPosition here
            Debug.Log("Player is outside the camera bounds: " + playerPosition);
            playerSpawnLocation = playerPosition;
        }
    }

    void OnDrawGizmos()
    {
        if (cameraComponent == null)
        {
            cameraComponent = GetComponent<Camera>();
        }

        if (cameraComponent == null)
        {
            return;
        }

        // Calculate the camera's pixel rect
        Rect pixelRect = cameraComponent.pixelRect;

        // Convert pixelRect to world space coordinates
        Vector3[] corners = new Vector3[4];
        corners[0] = cameraComponent.ScreenToWorldPoint(new Vector3(pixelRect.xMin, pixelRect.yMin, cameraComponent.nearClipPlane)); // Bottom-left
        corners[1] = cameraComponent.ScreenToWorldPoint(new Vector3(pixelRect.xMax, pixelRect.yMin, cameraComponent.nearClipPlane)); // Bottom-right
        corners[2] = cameraComponent.ScreenToWorldPoint(new Vector3(pixelRect.xMax, pixelRect.yMax, cameraComponent.nearClipPlane)); // Top-right
        corners[3] = cameraComponent.ScreenToWorldPoint(new Vector3(pixelRect.xMin, pixelRect.yMax, cameraComponent.nearClipPlane)); // Top-left

        // Set Gizmo color
        Gizmos.color = Color.red;

        // Draw rectangle in world space
        Gizmos.DrawLine(corners[0], corners[1]);
        Gizmos.DrawLine(corners[1], corners[2]);
        Gizmos.DrawLine(corners[2], corners[3]);
        Gizmos.DrawLine(corners[3], corners[0]);
    }
}

using UnityEngine;

public class MapPlayerMarker : MonoBehaviour
{
    public Transform player;  // XR Origin or rig root

    void LateUpdate()
    {
        if (player == null) return;

        // Copy XZ of the player; keep marker at its own Y (height above city)
        Vector3 pos = transform.position;
        pos.x = player.position.x;
        pos.z = player.position.z;
        transform.position = pos;

        // Optional: point arrow in same direction as player
        transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
    }
}

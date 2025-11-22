using UnityEngine;

public class LineFlow : MonoBehaviour
{
    public Material flowMat;
    public float scrollSpeed = 0.5f;
    private float offset;

    void Update()
    {
        if (flowMat == null) return;
        offset += Time.deltaTime * scrollSpeed;
        flowMat.SetTextureOffset("_MainTex", new Vector2(offset, 0));
    }
}

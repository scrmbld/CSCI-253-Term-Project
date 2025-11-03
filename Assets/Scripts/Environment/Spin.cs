using UnityEngine;

public class Spin : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float speed = 60f;
    void Update() => transform.Rotate(Vector3.up * speed * Time.deltaTime);
}

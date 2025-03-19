using UnityEngine;

public class Spin : MonoBehaviour
{
    public float rotationSpeed = 100f;
    public Transform rotatee;

    void Update()
    {
        rotatee.Rotate(Vector3.forward * -rotationSpeed * Time.deltaTime);
    }
}

using UnityEngine;

public class MouseMove : MonoBehaviour
{    
    public float sensitivity = 15f;

    void Update()
    {
        float rightLeftInput = Input.GetAxis("Horizontal"); // Valor de entrada horizontal (A y D)
        float upDownInput = Input.GetAxis("Vertical"); // Valor de entrada vertical (W y S)
        float zoomInput = Input.GetAxis("Mouse ScrollWheel") * 10f; // Valor de entrada del zoom (rueda del raton)

        Vector3 movement = new Vector3(rightLeftInput, upDownInput, zoomInput) * 10f;
        transform.Translate(movement);
    }
}

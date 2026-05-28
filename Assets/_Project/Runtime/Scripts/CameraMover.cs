using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMover : MonoBehaviour
{
    [Header("Sensibilité souris")]
    public float sensitivity = 0.08f;

    [Header("Limites verticales")]
    public float minPitch = -60f;
    public float maxPitch = 60f;

    [Header("Contrôle")]
    public bool holdRightClick = true;

    private Quaternion baseLocalRotation;

    private float yawOffset = 0f;
    private float pitchOffset = 0f;

    void Start()
    {
        // On garde exactement l'orientation de départ de la caméra
        baseLocalRotation = transform.localRotation;
    }

    void LateUpdate()
    {
        if (Mouse.current == null)
            return;

        if (holdRightClick && !Mouse.current.rightButton.isPressed)
            return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        yawOffset += mouseDelta.x * sensitivity;
        pitchOffset -= mouseDelta.y * sensitivity;

        pitchOffset = Mathf.Clamp(pitchOffset, minPitch, maxPitch);

        Quaternion mouseRotation = Quaternion.Euler(pitchOffset, yawOffset, 0f);

        // On ajoute le mouvement souris à la rotation de départ
        transform.localRotation = baseLocalRotation * mouseRotation;
    }
}
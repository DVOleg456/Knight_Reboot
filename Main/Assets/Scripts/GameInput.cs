using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance {  get; private set; }

    private PlayerInputAction playerinputaction;

    private void Awake()
    {
        Instance = this;
        playerinputaction = new PlayerInputAction();
        playerinputaction.Enable();
    }

    public Vector2 GetMovementVector()
    {
        Vector2 inputVector = playerinputaction.Player.Move.ReadValue<Vector2>();
        return inputVector;
    }

    public Vector3 GetMousePosition()
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        return mousePos;
    }
}

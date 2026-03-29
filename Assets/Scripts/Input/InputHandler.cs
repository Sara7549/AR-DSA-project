using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public static event Action<Vector2> OnTap;

    private UserInput _userInput;

    private void Awake()
    {
        _userInput = new UserInput();
    }

    private void OnEnable()
    {
        _userInput.Enable();
        _userInput.MobileTouch.Tap.performed += OnTapPerformed;
    }

    private void OnDisable()
    {
        _userInput.MobileTouch.Tap.performed -= OnTapPerformed;
        _userInput.Disable();
    }

    private void OnTapPerformed(InputAction.CallbackContext context)
    {
        Vector2 touchPos = Touchscreen.current.primaryTouch.position.ReadValue();
        OnTap?.Invoke(touchPos);
    }
}
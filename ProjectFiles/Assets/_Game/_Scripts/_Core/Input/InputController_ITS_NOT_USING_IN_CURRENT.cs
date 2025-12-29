using UnityEngine;
using UnityEngine.InputSystem;

public class InputController_ITS_NOT_USING_IN_CURRENT : MonoBehaviour
{
    IControllableInput_ITS_NOT_USING_IN_CURRENT controllable;
    GameInput gameInput;

    private void Awake()
    {
        gameInput = new GameInput();
        gameInput.Enable();

        controllable = GetComponent<IControllableInput_ITS_NOT_USING_IN_CURRENT>();
        if (controllable == null)
        {
            throw new System.Exception($"There is haven`t IControllableInput. ObjectName: {gameObject.name}");
        }
    }

    void OnEnable()
    {
        gameInput.Gameplay.Use.performed += UsePerformed;
    }

    private void OnDisable()
    {
        gameInput.Gameplay.Use.performed -= UsePerformed;
    }

    private void UsePerformed(InputAction.CallbackContext context)
    {
        controllable.Use();
    }

    void Update()
    {
        ReadMovement();
    }

    void ReadMovement()
    {
        var inputDirection = gameInput.Gameplay.Movement.ReadValue<Vector2>();
        controllable.Move((Vector3)inputDirection);
    }
}

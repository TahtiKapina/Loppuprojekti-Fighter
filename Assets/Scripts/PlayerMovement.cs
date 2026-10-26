
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    void Update()
    {
        float moveX = 0f;
        float moveY = 0f;

        if (Keyboard.current.aKey.isPressed)
            moveX = -1f;

        if (Keyboard.current.dKey.isPressed)
            moveX = 1f;

        if (Keyboard.current.wKey.isPressed)
            moveY = 1f;

        if (Keyboard.current.sKey.isPressed)
            moveY = -1f;

        Vector3 movement = new Vector3(moveX, moveY, 0f).normalized;

        transform.position += movement * moveSpeed * Time.deltaTime;
    }
}

using UnityEngine;

namespace Starter.Shooter
{
    public struct GameplayInput
    {
        public Vector2 LookRotation;
        public Vector2 MoveDirection;
        public bool Jump;
        public bool Fire;
    }

    public sealed class PlayerInput : MonoBehaviour
    {
        public GameplayInput CurrentInput => _input;
        private GameplayInput _input;

        public void ResetInput()
        {
            _input.MoveDirection = default;
            _input.Jump = false;
            _input.Fire = false;
        }

        private void Update()
        {
            // Nếu đang chat thì ngừng nhận input
            if (ChatUI.IsChatting)
            {
                ResetInput();
                return;
            }

            if (Cursor.lockState != CursorLockMode.Locked)
                return;

            _input.LookRotation += new Vector2(-Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X"));

            var moveDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            _input.MoveDirection = moveDirection.normalized;

            _input.Fire |= Input.GetButtonDown("Fire1");
            _input.Jump |= Input.GetButtonDown("Jump");
        }
    }
}

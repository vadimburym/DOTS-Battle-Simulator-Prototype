using UnityEngine;

namespace _Project._Code.Infrastructure
{
    public sealed class InputService : IInputService
    {
        public bool IsMainActionDown => Input.GetMouseButtonDown(0);
        public bool IsMainActionUp => Input.GetMouseButtonUp(0);
        public bool IsSecondActionDown => Input.GetMouseButtonDown(1);
        public Vector2 MousePosition => Input.mousePosition;
        public float Scroll => Input.mouseScrollDelta.y;
        
        public bool TryGetMouseToWorldPosition(out Vector3 worldPosition)
        {
            worldPosition = Vector3.zero;
            if (Camera.main == null)
                return false;
            var mouseCameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            var plane = new Plane(Vector3.up, Vector3.zero);
            if (plane.Raycast(mouseCameraRay, out float distance))
            {
                worldPosition = mouseCameraRay.GetPoint(distance);
                return true;
            }
            return false;
        }
    }
}
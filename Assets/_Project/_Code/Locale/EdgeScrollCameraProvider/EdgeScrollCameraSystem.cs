using _Project._Code.Core.Contracts;
using _Project._Code.Infrastructure;
using UnityEngine;

namespace _Project._Code.Locale.EdgeScrollCamera
{
    public sealed class EdgeScrollCameraSystem : ILateTick
    {
        private readonly IEdgeScrollCameraProvider _edgeScrollCameraProvider;
        private readonly IMainCameraService _mainCameraService;
        private readonly IInputService _inputService;
        
        public EdgeScrollCameraSystem(
            IEdgeScrollCameraProvider edgeScrollCameraProvider,
            IInputService inputService,
            IMainCameraService mainCameraService)
        {
            _edgeScrollCameraProvider = edgeScrollCameraProvider;
            _inputService = inputService;
            _mainCameraService = mainCameraService;
        }
        
        public void LateTick()
        {
            var transform = _mainCameraService.MainCameraTransform;
            
            var edgeSize = _edgeScrollCameraProvider.EdgeSize;
            var moveSpeed = _edgeScrollCameraProvider.MoveSpeed;
            var xLimits = _edgeScrollCameraProvider.XLimits;
            var zLimits = _edgeScrollCameraProvider.ZLimits;
            var zoomSpeed = _edgeScrollCameraProvider.ZoomSpeed;
            
            var pos = transform.position;
            var mousePos = _inputService.MousePosition;
            
            if (mousePos.x <= edgeSize)
                pos.x -= moveSpeed * Time.deltaTime;
            else if (mousePos.x >= Screen.width - edgeSize)
                pos.x += moveSpeed * Time.deltaTime;
            
            if (mousePos.y <= edgeSize)
                pos.z -= moveSpeed * Time.deltaTime;
            else if (mousePos.y >= Screen.height - edgeSize)
                pos.z += moveSpeed * Time.deltaTime;
            
            float scroll = _inputService.Scroll;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                pos.y -= scroll * zoomSpeed * Time.deltaTime;
                pos.y = Mathf.Clamp(pos.y,
                    _edgeScrollCameraProvider.MinY,
                    _edgeScrollCameraProvider.MaxY);
            }
            
            pos.x = Mathf.Clamp(pos.x, xLimits.x, xLimits.y);
            pos.z = Mathf.Clamp(pos.z, zLimits.x, zLimits.y);
            transform.position = pos;
        }
    }
}
using UnityEngine;

namespace _Project._Code.Locale.EdgeScrollCamera
{
    public sealed class EdgeScrollCameraProvider : MonoBehaviour, IEdgeScrollCameraProvider
    {
        public float ZoomSpeed => _zoomSpeed;
        public float MinY => _minY;
        public float MaxY => _maxY;
        public Transform Root => transform;
        public float MoveSpeed => _moveSpeed;
        public float EdgeSize => _edgeSize;
        public Vector2 XLimits => _xLimits;
        public Vector2 ZLimits => _zLimits;
        
        [SerializeField] private float _moveSpeed = 25f;
        [SerializeField] private float _edgeSize = 10f;
        private Vector2 _xLimits;
        private  Vector2 _zLimits;
        [SerializeField] private Transform LimitsMin; 
        [SerializeField] private Transform LimitsMax;
        
        [SerializeField] private float _zoomSpeed = 200f;
        [SerializeField] private float _minY = 10f;
        [SerializeField] private float _maxY = 80f;

        private void Awake()
        {
            _xLimits = new Vector2(LimitsMin.position.x, LimitsMax.position.x);
            _zLimits = new Vector2(LimitsMin.position.z, LimitsMax.position.z);
        }
    }
}
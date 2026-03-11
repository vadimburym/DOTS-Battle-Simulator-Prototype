using _Project._Code.Gameplay.CoreFeatures.Entities.Behaviours;
using _Project._Code.Gameplay.CoreFeatures.Entities.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using VContainer;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.Systems
{
    [DisableAutoCreation]
    public partial class SelectionSystem : SystemBase
    {
        private ISelectionAreaProvider _selectionAreaProvider;
        
        private SelectionResult _selectionResult;
        private bool _isSelectionDirty;
        
        [Inject]
        private void Construct(ISelectionAreaProvider selectionAreaProvider)
        {
            _selectionAreaProvider = selectionAreaProvider;
            _selectionAreaProvider.OnSelectionResult += OnSelectionResult;
        }

        private void OnSelectionResult(SelectionResult result)
        {
            _selectionResult = result;
            _isSelectionDirty = true;
        }
        
        protected override void OnUpdate()
        {
            if (!_isSelectionDirty)
                return;
            var camera = Camera.main;
            float2 min = _selectionResult.ScreenMin;
            float2 max = _selectionResult.ScreenMax;
            
            foreach (var (localTransform, entity)
                     in SystemAPI.Query<RefRO<LocalTransform>>().WithPresent<Selected>().WithEntityAccess())
            {
                float2 unitScreenPosition = (Vector2)camera.WorldToScreenPoint(localTransform.ValueRO.Position);
                bool inside = math.all(unitScreenPosition >= min & unitScreenPosition <= max);
                EntityManager.SetComponentEnabled<Selected>(entity, inside);
            }
            
            _isSelectionDirty = false;
        }

        protected override void OnDestroy()
        {
            if (_selectionAreaProvider != null)
                _selectionAreaProvider.OnSelectionResult -= OnSelectionResult;
        }
    }
}
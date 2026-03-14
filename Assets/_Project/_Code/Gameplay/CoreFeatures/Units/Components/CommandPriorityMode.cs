using Unity.Entities;

namespace _Project._Code.Gameplay.CoreFeatures
{
    public struct CommandPriorityMode : IComponentData
    {
        public CommandPriorityModeId Value;
    }
    
    public enum CommandPriorityModeId
    {
        None = 0,
        Reactive = 1,
        Strict = 2
    }
}
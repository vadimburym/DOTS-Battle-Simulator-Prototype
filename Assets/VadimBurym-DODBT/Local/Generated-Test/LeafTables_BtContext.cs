using _Project._Code.Gameplay.CoreFeatures.AI.BtLeafs;

namespace VadimBurym.DodBehaviourTree.Generated
{
    public struct LeafTables_BtContext
    {
        public static NodeStatus TickLeaf(byte leafId, ref RunnerState_BtContext state)
        {
            return leafId switch
            {
                0 => DebugLeaf.OnTick(ref state),
                1 => IsEnemyDetectedLeaf.OnTick(ref state),
                2 => DistanceToEnemyThresholdLeaf.OnTick(ref state),
                3 => InAttackRangeLeaf.OnTick(ref state),
                4 => ChaseEnemyLeaf.OnTick(ref state),
                5 => IsMovingLeaf.OnTick(ref state),
                6 => AttackEnemyLeaf.OnTick(ref state),
                7 => IdleLeaf.OnTick(ref state),
                _ => NodeStatus.None
            };
        }

        public static void EnterLeaf(byte leafId, ref RunnerState_BtContext state)
        {
            switch (leafId)
            {
                case 0: DebugLeaf.OnEnter(ref state); break;
                case 1: IsEnemyDetectedLeaf.OnEnter(ref state); break;
                case 2: DistanceToEnemyThresholdLeaf.OnEnter(ref state); break;
                case 3: InAttackRangeLeaf.OnEnter(ref state); break;
                case 4: ChaseEnemyLeaf.OnEnter(ref state); break;
                case 5: IsMovingLeaf.OnEnter(ref state); break;
                case 6: AttackEnemyLeaf.OnEnter(ref state); break;
                case 7: IdleLeaf.OnEnter(ref state); break;
            }
        }

        public static void ExitLeaf(byte leafId, ref RunnerState_BtContext state)
        {
            switch (leafId)
            {
                case 0: DebugLeaf.OnExit(ref state); break;
                case 1: IsEnemyDetectedLeaf.OnExit(ref state); break;
                case 2: DistanceToEnemyThresholdLeaf.OnExit(ref state); break;
                case 3: InAttackRangeLeaf.OnExit(ref state); break;
                case 4: ChaseEnemyLeaf.OnExit(ref state); break;
                case 5: IsMovingLeaf.OnExit(ref state); break;
                case 6: AttackEnemyLeaf.OnExit(ref state); break;
                case 7: IdleLeaf.OnExit(ref state); break;
            }
        }

        public static void AbortLeaf(byte leafId, ref RunnerState_BtContext state)
        {
            switch (leafId)
            {
                case 0: DebugLeaf.OnAbort(ref state); break;
                case 1: IsEnemyDetectedLeaf.OnAbort(ref state); break;
                case 2: DistanceToEnemyThresholdLeaf.OnAbort(ref state); break;
                case 3: InAttackRangeLeaf.OnAbort(ref state); break;
                case 4: ChaseEnemyLeaf.OnAbort(ref state); break;
                case 5: IsMovingLeaf.OnAbort(ref state); break;
                case 6: AttackEnemyLeaf.OnAbort(ref state); break;
            }
        }
    }
}

using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace VATDots
{
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct VATAnimationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VATAnimator>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var job = new VATAnimationUpdateJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
            };
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        private partial struct VATAnimationUpdateJob : IJobEntity
        {
            public float DeltaTime;

            private void Execute(ref VATAnimator animator, ref VATAnimationCommand command, in VATLibraryBlobRef libraryRef)
            {
                ref var clips = ref libraryRef.Value.Value.Clips;
                int clipCount = clips.Length;

                if (command.RequestedClipIndex >= 0)
                {
                    int requestedClip = command.RequestedClipIndex;
                    bool validClip = requestedClip < clipCount && requestedClip >= 0 && clips[requestedClip].MeshIndex == animator.MeshIndex;
                    if (validClip)
                    {
                        bool sameClip = animator.CurrentClipIndex == requestedClip;
                        if (!sameClip || command.RestartIfSame != 0)
                        {
                            animator.PreviousClipIndex = animator.CurrentClipIndex;
                            animator.PreviousNormalizedTime = animator.CurrentNormalizedTime;
                            animator.CurrentClipIndex = requestedClip;
                            animator.CurrentNormalizedTime = math.saturate(command.StartNormalizedTime);
                            animator.BlendElapsed = 0f;
                            animator.BlendDuration = command.TransitionDuration >= 0f ? command.TransitionDuration : animator.DefaultTransitionDuration;
                            animator.BlendDuration = math.max(0f, animator.BlendDuration);
                            animator.Blend01 = animator.BlendDuration <= 0f ? 1f : 0f;
                        }
                    }

                    command.RequestedClipIndex = -1;
                    command.TransitionDuration = -1f;
                    command.StartNormalizedTime = 0f;
                    command.RestartIfSame = 0;
                }

                if (animator.Playing != 0)
                {
                    if ((uint)animator.CurrentClipIndex < (uint)clipCount)
                    {
                        float currentLength = math.max(clips[animator.CurrentClipIndex].Length, 1e-5f);
                        animator.CurrentNormalizedTime = AdvanceTime(animator.CurrentNormalizedTime, currentLength, DeltaTime, animator.Speed, animator.Loop != 0);
                    }

                    if (animator.PreviousClipIndex >= 0 && (uint)animator.PreviousClipIndex < (uint)clipCount)
                    {
                        float previousLength = math.max(clips[animator.PreviousClipIndex].Length, 1e-5f);
                        animator.PreviousNormalizedTime = AdvanceTime(animator.PreviousNormalizedTime, previousLength, DeltaTime, animator.Speed, animator.Loop != 0);
                    }
                }

                if (animator.PreviousClipIndex >= 0)
                {
                    if (animator.BlendDuration <= 0f)
                    {
                        animator.Blend01 = 1f;
                        animator.PreviousClipIndex = -1;
                        animator.PreviousNormalizedTime = 0f;
                    }
                    else
                    {
                        animator.BlendElapsed += math.max(0f, DeltaTime);
                        animator.Blend01 = math.saturate(animator.BlendElapsed / animator.BlendDuration);
                        if (animator.Blend01 >= 0.9999f)
                        {
                            animator.Blend01 = 1f;
                            animator.PreviousClipIndex = -1;
                            animator.PreviousNormalizedTime = 0f;
                        }
                    }
                }
                else
                {
                    animator.Blend01 = 1f;
                }
            }

            private static float AdvanceTime(float current01, float clipLength, float deltaTime, float speed, bool loop)
            {
                if (clipLength <= 0f)
                    return current01;

                float next = current01 + (deltaTime * speed) / clipLength;
                return loop ? math.frac(next) : math.saturate(next);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct VATAnimStateGPU
    {
        public float AnimIndex;
        public float AnimTime;
        public float PrevAnimIndex;
        public float PrevAnimTime;
        public float Blend;
        public float Pad0;
        public float Pad1;
        public float Pad2;
    }

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(VATAnimationSystem))]
    public partial class VATAnimationUploadSystem : SystemBase
    {
        private static readonly int AnimStateBufferId = Shader.PropertyToID("_VATAnimStateBuffer");

        private EntityQuery _query;
        private GraphicsBuffer _gpuBuffer;
        private int _gpuCapacity;

        protected override void OnCreate()
        {
            _query = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<VATAnimator>(),
                    ComponentType.ReadWrite<VATRendererUserValue>(),
                }
            });

            RequireForUpdate(_query);
            EnsureBufferCapacity(1);
            Shader.SetGlobalBuffer(AnimStateBufferId, _gpuBuffer);
        }

        protected override void OnDestroy()
        {
            _gpuBuffer?.Dispose();
            _gpuBuffer = null;
            _gpuCapacity = 0;
        }

        protected override void OnUpdate()
        {
            int entityCount = _query.CalculateEntityCount();
            if (entityCount <= 0)
                return;

            using var states = new NativeArray<VATAnimStateGPU>(entityCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var job = new BuildAnimStateBufferJob
            {
                Output = states,
            };

            Dependency = job.ScheduleParallel(_query, Dependency);
            Dependency.Complete();

            EnsureBufferCapacity(entityCount);
            _gpuBuffer.SetData(states);
            Shader.SetGlobalBuffer(AnimStateBufferId, _gpuBuffer);
        }

        private void EnsureBufferCapacity(int requiredCount)
        {
            if (_gpuBuffer != null && _gpuCapacity >= requiredCount)
                return;

            int newCapacity = math.max(1, math.ceilpow2(requiredCount));
            _gpuBuffer?.Dispose();
            _gpuBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, newCapacity, Marshal.SizeOf<VATAnimStateGPU>());
            _gpuCapacity = newCapacity;
        }

        [BurstCompile]
        private partial struct BuildAnimStateBufferJob : IJobEntity
        {
            public NativeArray<VATAnimStateGPU> Output;

            private void Execute([EntityIndexInQuery] int entityIndexInQuery, ref VATRendererUserValue rendererUserValue, in VATAnimator animator)
            {
                rendererUserValue.Value = (uint)entityIndexInQuery;
                Output[entityIndexInQuery] = new VATAnimStateGPU
                {
                    AnimIndex = animator.CurrentClipIndex,
                    AnimTime = animator.CurrentNormalizedTime,
                    PrevAnimIndex = animator.PreviousClipIndex,
                    PrevAnimTime = animator.PreviousNormalizedTime,
                    Blend = animator.Blend01,
                    Pad0 = 0f,
                    Pad1 = 0f,
                    Pad2 = 0f,
                };
            }
        }
    }
}

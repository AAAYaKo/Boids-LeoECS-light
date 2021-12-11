using Leopotam.EcsLite;
using Leopotam.EcsLite.Threads.Unity;
using Unity.Collections;

namespace Client
{
	public sealed class RunAffectToBoidVelocitySystem : EcsUnityJobSystem<RunAffectToBoidVelocitySystem.Job, Velocity, BoidImpact>
	{
		protected override int GetChunkSize(EcsSystems systems)
		{
			return 64;
		}

		protected override EcsFilter GetFilter(EcsWorld world)
		{
			return world
				.Filter<Velocity>()
				.Inc<BoidImpact>()
				.End();
		}

		protected override EcsWorld GetWorld(EcsSystems systems)
		{
			return systems.GetWorld();
		}

		public struct Job : IEcsUnityJob<Velocity, BoidImpact>
		{
			NativeArray<int> _entities;
			[NativeDisableParallelForRestriction]
			NativeArray<Velocity> _velocityPool;
			[NativeDisableParallelForRestriction]
			NativeArray<int> _velocityIndices;
			[NativeDisableParallelForRestriction]
			NativeArray<BoidImpact> _impactPool;
			[NativeDisableParallelForRestriction]
			NativeArray<int> _impactIndices;


			public void Execute(int index)
			{
				int entity = _entities[index];

				int pool1Idx = _velocityIndices[entity];
				var velocity = _velocityPool[pool1Idx];

				var impact = _impactPool[_impactIndices[entity]];

				velocity.Vector = impact.FirstAffection + impact.SecondAffection + impact.ThirdAffection + impact.FourthAffection;
				_velocityPool[pool1Idx] = velocity;
			}

			public void Init(NativeArray<int> entities, NativeArray<Velocity> pool1, NativeArray<int> indices1, NativeArray<BoidImpact> pool2, NativeArray<int> indices2)
			{
				_entities = entities;
				_velocityPool = pool1;
				_impactPool = pool2;
				_velocityIndices = indices1;
				_impactIndices = indices2;
			}
		}
	}
}
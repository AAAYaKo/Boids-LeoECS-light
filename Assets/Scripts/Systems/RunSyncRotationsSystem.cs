using AAAAYaKo.EcsTransformJobs;
using Leopotam.EcsLite;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace Client
{
	public sealed class RunSyncRotationsSystem : EcsUnityTransformJobSystem<RunSyncRotationsSystem.Job, Rotation, RealTransform>
	{
		protected override EcsFilter GetFilter(EcsWorld world)
		{
			return world.Filter<Rotation>().Inc<RealTransform>().End();
		}

		protected override Transform GetTransform(RealTransform transform)
		{
			return transform.Transform;
		}

		protected override EcsWorld GetWorld(EcsSystems systems)
		{
			return systems.GetWorld();
		}

		public struct Job : IEcsUnityTransformsJob<Rotation>
		{
			NativeArray<int> _entities;
			[NativeDisableParallelForRestriction]
			NativeArray<Rotation> _pool1;
			[NativeDisableParallelForRestriction]
			NativeArray<int> _indices1;


			public void Execute(int index, TransformAccess transform)
			{
				var entity = _entities[index];
				var pool1Idx = _indices1[entity];
				var position = _pool1[pool1Idx];
				position.Value = transform.rotation;
				_pool1[pool1Idx] = position;
			}

			public void Init(NativeArray<int> entities, NativeArray<Rotation> pool1, NativeArray<int> indices1)
			{
				_entities = entities;
				_pool1 = pool1;
				_indices1 = indices1;
			}
		}
	}
}
using AAAAYaKo.EcsTransformJobs;
using Leopotam.EcsLite;
using Unity.Collections;
using UnityEngine.Jobs;

namespace Client
{
	public sealed class RunSyncPositionsSystem : EcsUnityTransformJobSystem<RunSyncPositionsSystem.Job, Position, RealTransform>
	{
		protected override EcsFilter GetFilter(EcsWorld world)
		{
			return world.Filter<Position>().Inc<RealTransform>().End();
		}

		protected override EcsWorld GetWorld(EcsSystems systems)
		{
			return systems.GetWorld();
		}

		public override void OnEntityTransformsPoolCanged()
		{
			_transforms.Clear();
			foreach (var transform in _poolTransform.GetRawDenseItems())
				_transforms.Add(transform.Transform);
			RebuildTransformAccessArray(_transforms.ToArray());
		}
		public struct Job : IEcsUnityTransformsJob<Position>
		{
			NativeArray<int> _entities;
			[NativeDisableParallelForRestriction]
			NativeArray<Position> _pool1;
			[NativeDisableParallelForRestriction]
			NativeArray<int> _indices1;


			public void Execute(int index, TransformAccess transform)
			{
				var entity = _entities[index];
				var pool1Idx = _indices1[entity];
				var position = _pool1[pool1Idx];
				position.Value = transform.position;
				_pool1[pool1Idx] = position;
			}

			public void Init(NativeArray<int> entities, NativeArray<Position> pool1, NativeArray<int> indices1)
			{
				_entities = entities;
				_pool1 = pool1;
				_indices1 = indices1;
			}
		}
	}

}
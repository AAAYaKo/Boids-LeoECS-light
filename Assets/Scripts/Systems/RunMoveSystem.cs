using AAAAYaKo.EcsTransformJobs;
using Leopotam.EcsLite;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace Client
{
	public sealed class RunMoveSystem : EcsUnityTransformJobSystem<RunMoveSystem.Job, Velocity, RealTransform>
	{
		protected override EcsFilter GetFilter(EcsWorld world)
		{
			return world.Filter<Velocity>().Inc<RealTransform>().End();
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

		protected override void SetData(EcsSystems systems, ref Job job)
		{
			job.DeltaTime = Time.deltaTime;
		}

		public struct Job : IEcsUnityTransformsJob<Velocity>
		{
			public float DeltaTime;
			NativeArray<int> _entities;
			[NativeDisableParallelForRestriction]
			NativeArray<Velocity> _pool1;
			[NativeDisableParallelForRestriction]
			NativeArray<int> _indices1;


			public void Execute(int index, TransformAccess transform)
			{
				if (index >= _entities.Length || index < 0) return;
				if (index >= _indices1.Length || index < 0) return;
				if (index >= _pool1.Length || index < 0) return;

				var entity = _entities[index];
				var pool1Idx = _indices1[entity];
				var velocity = _pool1[pool1Idx];
				transform.position += (Vector3)(velocity.Vector * DeltaTime);
			}

			public void Init(NativeArray<int> entities, NativeArray<Velocity> pool1, NativeArray<int> indices1)
			{
				_entities = entities;
				_pool1 = pool1;
				_indices1 = indices1;
			}
		}
	}
}
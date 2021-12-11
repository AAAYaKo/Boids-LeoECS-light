using Leopotam.EcsLite;
using Leopotam.EcsLite.Threads;
using Unity.Mathematics;

namespace Client
{
	public sealed class RunBoidThirdRuleSystem : EcsThreadSystem<RunBoidThirdRuleSystem.Thread, Velocity, Boid, BoidImpact>
	{
		protected override int GetChunkSize(EcsSystems systems)
		{
			return 64;
		}

		protected override EcsFilter GetFilter(EcsWorld world)
		{
			return world
				.Filter<Velocity>()
				.Inc<Boid>()
				.Inc<BoidImpact>()
				.End();
		}

		protected override EcsWorld GetWorld(EcsSystems systems)
		{
			return systems.GetWorld();
		}

		protected override void SetData(EcsSystems systems, ref Thread thread)
		{
			thread.World = GetWorld(systems);
		}

		public struct Thread : IEcsThread<Velocity, Boid, BoidImpact>
		{
			public EcsWorld World;
			private int[] _entities;
			private Boid[] _boidPool;
			private Velocity[] _velocityPool;
			private BoidImpact[] _impactPool;
			private int[] _boidIndices;
			private int[] _velocityIndices;
			private int[] _impactIndices;


			public void Execute(int fromIndex, int beforeIndex)
			{
				for (int i = fromIndex; i < beforeIndex; i++)
				{
					var entity = _entities[i];
					var closest = _boidPool[_boidIndices[entity]];
					var velocity = _velocityPool[_velocityIndices[entity]].Vector;
					ref var impact = ref _impactPool[_impactIndices[entity]];

					float3 average = velocity;
					int count = 1;
					foreach (var other in closest.Closest)
					{
						if (other.Unpack(World, out int otherEntity))
						{
							average += _velocityPool[_velocityIndices[otherEntity]].Vector;
							count++;
						}
					}

					impact.ThirdAffection = average / (count * 8);
				}
			}

			public void Init(int[] entities, Velocity[] pool1, int[] indices1, Boid[] pool2, int[] indices2, BoidImpact[] pool3, int[] indices3)
			{
				_entities = entities;
				_velocityPool = pool1;
				_boidPool = pool2;
				_impactPool = pool3;
				_boidIndices = indices1;
				_velocityIndices = indices2;
				_impactIndices = indices3;
			}
		}
	}
}
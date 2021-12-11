using Leopotam.EcsLite;
using Leopotam.EcsLite.Threads;
using Unity.Mathematics;

namespace Client
{
	public sealed class RunBoidSecondRuleSystem : EcsThreadSystem<RunBoidSecondRuleSystem.Thread, Position, Boid, BoidImpact>
	{
		protected override int GetChunkSize(EcsSystems systems)
		{
			return 64;
		}

		protected override EcsFilter GetFilter(EcsWorld world)
		{
			return world
				.Filter<Position>()
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

		public struct Thread : IEcsThread<Position, Boid, BoidImpact>
		{
			public EcsWorld World;
			private int[] _entities;
			private Boid[] _boidPool;
			private Position[] _positionPool;
			private BoidImpact[] _impactPool;
			private int[] _boidIndices;
			private int[] _positionIndices;
			private int[] _impactIndices;


			public void Execute(int fromIndex, int beforeIndex)
			{
				for (int i = fromIndex; i < beforeIndex; i++)
				{
					var entity = _entities[i];
					var closest = _boidPool[_boidIndices[entity]];
					var position = _positionPool[_positionIndices[entity]].Value;
					ref var impact = ref _impactPool[_impactIndices[entity]];
					float3 velocity = 0;
					foreach (var other in closest.Closest)
					{
						if (other.Unpack(World, out int otherEntity))
						{
							float3 otherPosition = _positionPool[_positionIndices[otherEntity]].Value;
							float distance = math.distance(otherPosition, position);
							if (distance <= 3)
							{
								velocity += math.normalize(position - otherPosition) * (4 - distance);
							}
						}
					}

					impact.SecondAffection = velocity / 16;
				}
			}

			public void Init(int[] entities, Position[] pool1, int[] indices1, Boid[] pool2, int[] indices2, BoidImpact[] pool3, int[] indices3)
			{
				_entities = entities;
				_positionPool = pool1;
				_boidPool = pool2;
				_impactPool = pool3;
				_boidIndices = indices1;
				_positionIndices = indices2;
				_impactIndices = indices3;
			}
		}
	}
}
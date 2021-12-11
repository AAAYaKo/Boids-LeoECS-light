using Leopotam.EcsLite;
using Leopotam.EcsLite.Threads;
using Unity.Mathematics;

namespace Client
{
	public sealed class RunBoidFirstRuleSystem : EcsThreadSystem<RunBoidFirstRuleSystem.Thread, Position, Boid, BoidImpact>
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
					float3 massCenter = position;
					int count = 1;
					foreach(var other in closest.Closest)
					{
						if (other.Unpack(World, out int otherEntity))
						{
							massCenter += _positionPool[_positionIndices[otherEntity]].Value;
							count++;
						}
					}
					massCenter /= count;

					impact.FirstAffection = (massCenter - position) / 8;
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
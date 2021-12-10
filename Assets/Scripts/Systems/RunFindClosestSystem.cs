using DataStructures.ViliWonka.KDTree;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.Threads;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Client
{
	public sealed class RunFindClosestSystem : EcsThreadSystem<TestThread, Boid, Position>
	{
		[EcsInject] private readonly KDTree _tree;
		[EcsWorld] private readonly EcsWorld _world;


		protected override int GetChunkSize(EcsSystems systems)
		{
			return 32;
		}

		protected override EcsFilter GetFilter(EcsWorld world)
		{
			return world.Filter<Boid>().Inc<Position>().End();
		}

		protected override EcsWorld GetWorld(EcsSystems systems)
		{
			return systems.GetWorld();
		}

		protected override void SetData(EcsSystems systems, ref TestThread thread)
		{
			thread.Tree = _tree;
			thread.World = _world;
		}
	}

	public struct TestThread : IEcsThread<Boid, Position>
	{
		public KDTree Tree;
		public EcsWorld World;
		private int[] _entities;
		private Boid[] _boidPool;
		private Position[] _positionPool;
		private int[] _boidIndices;
		private int[] _positionIndices;
		private Pool<KDQuery> queriesPool;
		private Pool<List<int>> resultIdsPool;
		private Pool<List<float3>> resultsPool;


		public void Execute(int fromIndex, int beforeIndex)
		{
			var query = queriesPool.Get();
			var resultIds = resultIdsPool.Get();
			var results = resultsPool.Get();
			for (int i = fromIndex; i < beforeIndex; i++)
			{
				resultIds.Clear();
				results.Clear();

				var entity = _entities[i];
				ref var boid = ref _boidPool[_boidIndices[entity]];
				ref var position = ref _positionPool[_positionIndices[entity]];

				if (boid.Closest == null) boid.Closest = new List<EcsPackedEntity>();
				var point = position;
				query.Radius(Tree, point.Value, 7, resultIds);
				foreach (var result in resultIds)
					results.Add(Tree.Points[result]);
				boid.Closest.Clear();
				foreach (var other in _entities)
				{
					if (other == entity) continue;
					if (results.Contains(_positionPool[other].Value))
						boid.Closest.Add(World.PackEntity(other));
				}
			}
			queriesPool.Return(query);
			resultsPool.Return(results);
			resultIdsPool.Return(resultIds);
		}

		public void Init(int[] entities, Boid[] pool1, int[] indices1, Position[] pool2, int[] indices2)
		{
			_entities = entities;
			_boidIndices = indices1;
			_positionPool = pool2;
			_boidPool = pool1;
			_positionIndices = indices2;
			queriesPool = new Pool<KDQuery>();
			resultIdsPool = new Pool<List<int>>();
			resultsPool = new Pool<List<float3>>();
		}

		private class Pool<T> where T : new()
		{
			private readonly Stack<T> _pool = new Stack<T>();


			public T Get()
			{
				lock (_pool)
				{
					if (_pool.Count == 0)
						return new T();
					else
						return _pool.Pop();
				}
			}

			public void Return(T pooled)
			{
				lock (_pool)
				{
					_pool.Push(pooled);
				}
			}
		}
	}
}
using DataStructures.ViliWonka.KDTree;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.Threads;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Client
{
	public sealed class RunFindClosestSystem : EcsThreadSystem<TestThread, Boid, Position>
	{
		[EcsInject] private readonly KDTree _tree;
		[EcsWorld] private readonly EcsWorld _world;
		private readonly Pool<KDQuery> queriesPool = new Pool<KDQuery>();
		private readonly Pool<List<int>> resultIdsPool = new Pool<List<int>>();
		private readonly Dictionary<float3, int> dictionary = new Dictionary<float3, int>();


		protected override int GetChunkSize(EcsSystems systems)
		{
			return 64;
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
			thread.QueriesPool = queriesPool;
			thread.ResultIdsPool = resultIdsPool;
			thread.Dictionary = dictionary;
			thread.FillDictionary();
		}
	}

	public struct TestThread : IEcsThread<Boid, Position>
	{
		public KDTree Tree;
		public EcsWorld World;
		public Pool<KDQuery> QueriesPool;
		public Pool<List<int>> ResultIdsPool;
		public Dictionary<float3, int> Dictionary;
		private int[] _entities;
		private Boid[] _boidPool;
		private Position[] _positionPool;
		private int[] _boidIndices;
		private int[] _positionIndices;


		public void Execute(int fromIndex, int beforeIndex)
		{
			var query = QueriesPool.Get();
			var resultIds = ResultIdsPool.Get();
			for (int i = fromIndex; i < beforeIndex; i++)
			{
				resultIds.Clear();
				var entity = _entities[i];
				ref var boid = ref _boidPool[_boidIndices[entity]];
				ref var position = ref _positionPool[_positionIndices[entity]];

				if (boid.Closest == null) boid.Closest = new List<EcsPackedEntity>();
				var point = position.Value;
				query.Radius(Tree, point, 8, resultIds);
				boid.Closest.Clear();
				foreach (var result in resultIds)
				{
					var other = Tree.Points[result];
					if ((Vector3)point == other) continue;
					boid.Closest.Add(World.PackEntity(Dictionary[other]));
				}
			}
			QueriesPool.Return(query);
			ResultIdsPool.Return(resultIds);
		}

		public void Init(int[] entities, Boid[] pool1, int[] indices1, Position[] pool2, int[] indices2)
		{
			_entities = entities;
			_boidIndices = indices1;
			_positionPool = pool2;
			_boidPool = pool1;
			_positionIndices = indices2;
		}

		public void FillDictionary()
		{
			Dictionary.Clear();
			foreach (var entity in _entities)
				Dictionary[_positionPool[_positionIndices[entity]].Value] = entity;
		}
	}
	public class Pool<T> where T : new()
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
using DataStructures.ViliWonka.KDTree;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;
using UnityEngine;

namespace Client
{
	public sealed class RunCloudSystem : IEcsRunSystem
	{
		[EcsInject] private readonly KDTree _tree;
		[EcsFilter(typeof(Position))] private readonly EcsFilter _filter;
		[EcsPool] private readonly EcsPool<Position> _pool;
		private Vector3[] points = new Vector3[32];


		public void Run(EcsSystems systems)
		{
			int count = _filter.GetEntitiesCount();
			if (count != points.Length)
				Array.Resize(ref points, count);
			int arrayID = 0;
			foreach (var entity in _filter)
			{
				ref var position = ref _pool.Get(entity);
				points[arrayID] = position.Value;
				arrayID++;
			}

			_tree.Build(points, 8);
		}
	}
}
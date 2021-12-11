using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Client
{
	public sealed class RunAddBoidImpactSystem : IEcsRunSystem
	{
		[EcsWorld] private readonly EcsWorld _world;
		[EcsPool] private readonly EcsPool<BoidImpact> _pool;


		public void Run(EcsSystems systems)
		{
			var filter = _world
				.Filter<Boid>()
				.Exc<BoidImpact>()
				.End();
			foreach (var entity in filter)
				_pool.Add(entity);
		}
	}
}
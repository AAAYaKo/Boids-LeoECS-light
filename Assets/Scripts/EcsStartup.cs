using DataStructures.ViliWonka.KDTree;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;
using Voody.UniLeo.Lite;

namespace Client
{
	public sealed class EcsStartup : MonoBehaviour
	{
		[SerializeField] private GameObject prefab;
		private EcsSystems _systems;

		private void Start()
		{
			//for (int i = 0; i < 512; i++)
			//	Instantiate(prefab, Random.insideUnitSphere * 16, Random.rotation);
			// register your shared data here, for example:
			// var shared = new Shared ();
			// systems = new EcsSystems (new EcsWorld (), shared);
			_systems = new EcsSystems(new EcsWorld());
			_systems
				.Add(new RunSyncPositionsSystem())
				.Add(new RunSyncRotationsSystem())
				.Add(new RunCloudSystem())
				.Add(new RunFindClosestSystem())
				.Add(new RunMoveSystem())

				// register additional worlds here, for example:
				// .AddWorld (new EcsWorld (), "events")
#if UNITY_EDITOR
				// add debug systems for custom worlds here, for example:
				// .Add (new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem ("events"))
				.Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem())
#endif
				.ConvertScene()
				.Inject(new KDTree())
				.Init();
		}

		private void Update()
		{
			_systems?.Run();
		}

		private void OnDestroy()
		{
			if (_systems != null)
			{
				_systems.Destroy();
				// add here cleanup for custom worlds, for example:
				// _systems.GetWorld ("events").Destroy ();
				_systems.GetWorld().Destroy();
				_systems = null;
			}
		}
	}
}
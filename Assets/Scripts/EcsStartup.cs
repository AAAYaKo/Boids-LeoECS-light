using DataStructures.ViliWonka.KDTree;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System.Collections;
using UnityEngine;
using Voody.UniLeo.Lite;

namespace Client
{
	public sealed class EcsStartup : MonoBehaviour
	{
		[SerializeField] private GameObject prefab;
		private EcsSystems _updateSystems;


		private void Start()
		{
			StartCoroutine(Spawn());

			var defaultWorld = new EcsWorld();
			_updateSystems = new EcsSystems(defaultWorld);

			_updateSystems
				.Add(new RunSyncPositionsSystem())
				.Add(new RunSyncRotationsSystem())
				.Add(new RunCloudSystem())
				.Add(new RunFindClosestSystem())
				.Add(new RunBoidFirstRuleSystem())
				.Add(new RunBoidSecondRuleSystem())
				.Add(new RunBoidThirdRuleSystem())
				.Add(new RunBoidFourthRuleSystem())
				.Add(new RunAffectToBoidVelocitySystem())
				.Add(new RunMoveSystem())
				.Add(new RunAddBoidImpactSystem());
#if UNITY_EDITOR
			// add debug systems for custom worlds here, for example:
			// .Add (new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem ("events"))
			_updateSystems.Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem());
#endif
			_updateSystems
				.ConvertScene()
				.Inject(new KDTree())
				.Init();

		}

		private void Update()
		{
			_updateSystems?.Run();
		}

		private void OnDestroy()
		{
			if (_updateSystems != null)
			{
				_updateSystems.Destroy();
				_updateSystems.GetWorld().Destroy();
				_updateSystems = null;
			}
		}

		private IEnumerator Spawn()
		{
			for (int i = 0; i < 512; i++)
			{
				Instantiate(prefab, Random.insideUnitSphere * 16, Random.rotation);
				yield return null;
			}
		}
	}
}
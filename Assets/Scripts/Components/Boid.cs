using Leopotam.EcsLite;
using System;
using System.Collections.Generic;

namespace Client
{
	[Serializable]
	public struct Boid
	{
		public List<EcsPackedEntity> Closest;
	}
}
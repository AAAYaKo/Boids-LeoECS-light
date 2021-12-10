using Voody.UniLeo.Lite;

namespace Client
{
	public class RealTransformProvider : MonoProvider<RealTransform>
	{
		private void Awake()
		{
			value.Transform = transform;
		}
	}
}
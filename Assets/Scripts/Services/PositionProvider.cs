using Voody.UniLeo.Lite;

namespace Client
{
	public class PositionProvider : MonoProvider<Position>
	{
		private void Awake()
		{
			value.Value = transform.position;
		}
	}
}
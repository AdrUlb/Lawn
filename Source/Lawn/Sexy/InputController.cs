using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Sexy;

internal class InputController
{
	public static Vector2 touchPos = Vector2.Zero;

	public static void TestTouchCaps()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		TouchPanelCapabilities capabilities = TouchPanel.GetCapabilities();
		if (((TouchPanelCapabilities)(ref capabilities)).IsConnected)
		{
			int maximumTouchCount = ((TouchPanelCapabilities)(ref capabilities)).MaximumTouchCount;
			Debug.OutputDebug("maxPoints", maximumTouchCount);
		}
	}

	public static void HandleTouchInput()
	{
	}
}

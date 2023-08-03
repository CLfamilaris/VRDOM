using Godot;

namespace VRDOM;

public enum TouchEvents
{
	TOUCH_DOWN = 0,
	TOUCH_UP = 1,
	TOUCH_MOVE = 2,
}

public class TouchCommand
{
	public int sensorId;
	public int touchEvent;
	public Vector2 touchPosition;
	public Vector2 touchSize;

	public TouchCommand(int sensorId, int touchEvent, Vector2 touchPosition, Vector2 touchSize)
	{
		this.sensorId = sensorId;
		this.touchEvent = touchEvent;
		this.touchPosition = touchPosition;
		this.touchSize = touchSize;
	}
}
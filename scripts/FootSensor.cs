using System.Collections.Generic;
using Godot;
using VRDOM;

public partial class FootSensor : CsgMesh3D
{
    [Export]
    private GameConnection _gameConnection;

    private Area3D _sensor;
    private Marker3D MinMarker;
    private Marker3D MaxMarker;
    private Dictionary<int, bool> _activeSensors = new Dictionary<int, bool>();
    public Vector3[] gpu_led_data = new Vector3[1862];
    private Vector2 defaultTouchSize = new Vector2(0.15f, 0.15f);

    public override void _PhysicsProcess(double delta)
    {
        List<TouchCommand> touchCommands = new List<TouchCommand>();

        foreach (var pair in _activeSensors)
            _activeSensors[pair.Key] = false;

        foreach (Area3D area in _sensor.GetOverlappingAreas())
        {
            var sensorTrigger = area as SensorTrigger;
            RayCast3D raycast = area.GetNode("RayCast3D") as RayCast3D;
            if (raycast.IsColliding())
            {
                if (_activeSensors.ContainsKey(sensorTrigger.sensorId))
                {
                    _activeSensors[sensorTrigger.sensorId] = true;
                    //GD.Print($"move event on {sensorTrigger.sensorId}!");
                    //move event!
                    touchCommands.Add(new TouchCommand(sensorTrigger.sensorId, (int)TouchEvents.TOUCH_MOVE, GetLocalTouchPosition(new Vector2(raycast.GetCollisionPoint().X, raycast.GetCollisionPoint().Z)), defaultTouchSize));
    
                }
                else
                {
                    _activeSensors.Add(sensorTrigger.sensorId, true);
                    //GD.Print($"down event on {sensorTrigger.sensorId}!");
                    //down event!
                    touchCommands.Add(new TouchCommand(sensorTrigger.sensorId, (int)TouchEvents.TOUCH_DOWN, GetLocalTouchPosition(new Vector2(raycast.GetCollisionPoint().X, raycast.GetCollisionPoint().Z)), defaultTouchSize));
                }
            }
        }

        foreach (var pair in _activeSensors)
        {
            if (pair.Value == false)
            {
                _activeSensors.Remove(pair.Key);
                //GD.Print($"up event on {pair.Key}!");
                //up event!
                touchCommands.Add(new TouchCommand(pair.Key, (int)TouchEvents.TOUCH_UP, new Vector2(0, 0), defaultTouchSize));
            }
        }
        _gameConnection.setTouch(touchCommands);
    }

    public Vector2 GetLocalTouchPosition(Vector2 vec)
    {
        float x = 1 - (vec.X - MinMarker.GlobalPosition.X) / (MaxMarker.GlobalPosition.X - MinMarker.GlobalPosition.X);
        float y = 1 - (vec.Y - MinMarker.GlobalPosition.Z) / (MaxMarker.GlobalPosition.Z - MinMarker.GlobalPosition.Z);
        return new Vector2(1 - x, 1 - y);
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _sensor = GetNode("Sensor") as Area3D;
        MinMarker = GetNode("Sensor/MinMarker") as Marker3D;
        MaxMarker = GetNode("Sensor/MaxMarker") as Marker3D;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        (Material as ShaderMaterial).SetShaderParameter("led_data", gpu_led_data);
    }
}

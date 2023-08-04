using System.Collections.Generic;
using Godot;
using VRDOM;

public partial class FootSensor : CsgMesh3D
{
    [Export]
    private GameConnection _gameConnection;

    private Area3D _sensor;
    private Marker3D _minMarker;
    private Marker3D _maxMarker;
    private Dictionary<int, bool> _activeSensors = new();
    public Vector3[] gpu_led_data = new Vector3[1862];
    private Vector2 _defaultTouchSize = new(0.15f, 0.15f);

    public override void _PhysicsProcess(double delta)
    {
        //"collect" touch commands so we can all send them in one WebSocket message
        //needed so the multitouch doesn't brutally murder the WebSocket connection
        List<TouchCommand> touchCommands = new();

        //Reset the "touching" bool,
        //if it's still touching it'll be set to true again later
        foreach (var pair in _activeSensors)
            _activeSensors[pair.Key] = false;

        //See if any SensorTrigger is touching the foot sensor area
        foreach (Area3D area in _sensor.GetOverlappingAreas())
        {
            var sensorTrigger = area as SensorTrigger;
            RayCast3D raycast = area.GetNode("RayCast3D") as RayCast3D;
            if (raycast.IsColliding())
            {
                //If the sensor is already in the dict, it's already touched before -> MOVE
                //If it hasn't touched before -> DOWN
                if (_activeSensors.ContainsKey(sensorTrigger.sensorId))
                {
                    _activeSensors[sensorTrigger.sensorId] = true;
                    touchCommands.Add(new TouchCommand(sensorTrigger.sensorId, (int)TouchEvents.TOUCH_MOVE, GetLocalTouchPosition(new Vector2(raycast.GetCollisionPoint().X, raycast.GetCollisionPoint().Z)), _defaultTouchSize));
                }
                else
                {
                    _activeSensors.Add(sensorTrigger.sensorId, true);
                    touchCommands.Add(new TouchCommand(sensorTrigger.sensorId, (int)TouchEvents.TOUCH_DOWN, GetLocalTouchPosition(new Vector2(raycast.GetCollisionPoint().X, raycast.GetCollisionPoint().Z)), _defaultTouchSize));
                }
            }
        }

        //If touching bool is still false -> UP
        foreach (var pair in _activeSensors)
        {
            if (pair.Value == false)
            {
                _activeSensors.Remove(pair.Key);
                touchCommands.Add(new TouchCommand(pair.Key, (int)TouchEvents.TOUCH_UP, new Vector2(0, 0), _defaultTouchSize));
            }
        }
        _gameConnection.SetTouch(touchCommands);
    }

    //Turns the world position into X and Y values from 0 to 1 on the pad that we can use for Spice
    public Vector2 GetLocalTouchPosition(Vector2 vec)
    {
        float x = 1 - (vec.X - _minMarker.GlobalPosition.X) / (_maxMarker.GlobalPosition.X - _minMarker.GlobalPosition.X);
        float y = 1 - (vec.Y - _minMarker.GlobalPosition.Z) / (_maxMarker.GlobalPosition.Z - _minMarker.GlobalPosition.Z);
        return new Vector2(1 - x, 1 - y);
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _sensor = GetNode("Sensor") as Area3D;
        _minMarker = GetNode("Sensor/MinMarker") as Marker3D;
        _maxMarker = GetNode("Sensor/MaxMarker") as Marker3D;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        (Material as ShaderMaterial).SetShaderParameter("led_data", gpu_led_data);
    }
}

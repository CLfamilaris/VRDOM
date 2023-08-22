using Godot;

public partial class SensorTrigger : Area3D
{
    [Export]
    public int sensorId;
    [Export]
    public RayCast3D rayCast;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        rayCast.GlobalRotation = Vector3.Zero;
    }
}

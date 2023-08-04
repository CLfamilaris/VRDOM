using Godot;
using System;

public partial class ActionHandler : Node
{
	[Export]
	private Node3D _leftFoot;
	[Export]
	private Node3D _rightFoot;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void OnXRButtonPress(string name)
	{
		GD.Print("Action pressed: " + name);
		if (name == "recalibrate")
		{
			Recalibrate();
		}
	}

	private void Recalibrate()
	{
		GD.Print("Recalibrating feet");
		const float defaultFootHeight = 0.075f;
		_leftFoot.GlobalRotation = Vector3.Zero;
		_rightFoot.GlobalRotation = Vector3.Zero;

		_leftFoot.GlobalPosition = new Vector3(_leftFoot.GlobalPosition.X, defaultFootHeight, _leftFoot.GlobalPosition.Z);
		_rightFoot.GlobalPosition = new Vector3(_rightFoot.GlobalPosition.X, defaultFootHeight, _rightFoot.GlobalPosition.Z);
	}
}

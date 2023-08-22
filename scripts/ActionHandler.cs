using Godot;
using System;

public partial class ActionHandler : Node
{
	[Export]
	private Node3D _leftHand;
	[Export]
	private Node3D _rightHand;

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
		GD.Print("Recalibrating hands");
		const float defaultFootHeight = 0.038f;
		_leftHand.GlobalRotation = Vector3.Zero;
		_rightHand.GlobalRotation = Vector3.Zero;

		_leftHand.GlobalPosition = new Vector3(_leftHand.GlobalPosition.X, defaultFootHeight, _leftHand.GlobalPosition.Z);
		_rightHand.GlobalPosition = new Vector3(_rightHand.GlobalPosition.X, defaultFootHeight, _rightHand.GlobalPosition.Z);
	}
}

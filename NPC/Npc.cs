using Godot;

public partial class Npc : CharacterBody2D
{
    [Export]
	public float Speed = 300.0f;

	private AnimationTree _animationTree;
	private Vector2 _lastGlobalPosition;
	private Vector2 _lastDirection = Vector2.Down;

	public bool CanMove { get; set; } = true;

    public override void _Ready()
	{
		_animationTree = GetNode<AnimationTree>("AnimationTree");
		_animationTree.Active = true;
		_lastGlobalPosition = GlobalPosition;

        var dialogController = GetNode<DialogController>("/root/Game2D/HBoxContainer/LeftViewportContainer/LeftSubViewport/LeftUI/BottomCenter/Dialog");
        dialogController.DialogEnded += (who) => CanMove = true;
    }

	public override void _PhysicsProcess(double delta)
	{
		var movement = GlobalPosition - _lastGlobalPosition;

		if (movement.LengthSquared() > 0.0001f)
		{
			_lastDirection = movement.Normalized();
			_animationTree.Set("parameters/Walking/blend_position", _lastDirection);
		}

		_lastGlobalPosition = GlobalPosition;
	}
}

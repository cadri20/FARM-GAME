using Godot;
using System;

public partial class PlayerController : CharacterBody2D
{
	[Export]
	public int Speed = 400;

	private AnimationTree _animationTree;
	private Vector2 _moveDirection;

    public override void _Ready()
    {
        _animationTree = GetNode<AnimationTree>("AnimationTree");
		_animationTree.Active = true;
    }

	public override void _PhysicsProcess(double delta)
	{
		ValidateInput();
		_animationTree.Set("parameters/Walking/blend_position", _moveDirection);
		MoveAndSlide();
    }

	private void ValidateInput()
	{
		_moveDirection = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		Velocity = _moveDirection * Speed;
    }

}

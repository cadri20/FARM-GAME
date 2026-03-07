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
		AnimatePlayer();
		MoveAndSlide();
    }

	private void ValidateInput()
	{
		_moveDirection = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		Velocity = _moveDirection * Speed;
    }

	private void AnimatePlayer()
	{
		if (Velocity.Length() == 0) 
		{ 
			_animationTree.Set("parameters/conditions/Idle", true);
			_animationTree.Set("parameters/conditions/Walk", false);
		}
		else
		{
            _animationTree.Set("parameters/Walking/blend_position", _moveDirection);
            _animationTree.Set("parameters/Idle/blend_position", _moveDirection);

            _animationTree.Set("parameters/conditions/Idle", false);
            _animationTree.Set("parameters/conditions/Walk", true);
        }
    }

}

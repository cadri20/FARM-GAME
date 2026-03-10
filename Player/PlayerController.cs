using Godot;
using System;
using System.Threading.Tasks;

public partial class PlayerController : CharacterBody2D
{
	[Export]
	public int Speed = 400;

	private AnimationTree _animationTree;
	private Vector2 _moveDirection;
	private Node2D _farmTool;
	private AnimationTree _farmToolAnim;

	private bool _canMove = true;
	private bool _canAction = true;

    public override void _Ready()
    {
        _animationTree = GetNode<AnimationTree>("AnimationTree");
		_animationTree.Active = true;
		_farmTool = GetNode<Node2D>("FarmTool");
		_farmToolAnim = _farmTool.GetNode<AnimationTree>("AnimationTree");
    }

	public override void _PhysicsProcess(double delta)
	{
		if (_canMove)
		{
            ValidateInput();
            AnimatePlayer();
            MoveAndSlide();
        }
		
    }

    public override async void _UnhandledInput(InputEvent @event)
    {
        if(_canMove && _canAction)
		{
			if (@event.IsActionPressed("ui_select"))
			{
				_canMove = false;
				_canAction = false;
				_animationTree.Set("parameters/conditions/UsingTool", true);
				UsingTool(true);
				_animationTree.Set("parameters/conditions/Idle", false);
				_animationTree.Set("parameters/conditions/Walk", false);
                await ToSignal(GetTree().CreateTimer(0.6f), SceneTreeTimer.SignalName.Timeout);
				_animationTree.Set("parameters/conditions/Idle", true);
				_animationTree.Set("parameters/conditions/UsingTool", false);
				UsingTool(false);
                _canMove = true;
				await EnableAction();
            }
		}
    }

	private async Task EnableAction()
	{
		await ToSignal(GetTree().CreateTimer(0.2f), SceneTreeTimer.SignalName.Timeout);
		_canAction = true;
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
            _animationTree.Set("parameters/UsingTool/blend_position", _moveDirection);
            _farmToolAnim.Set("parameters/Tool/blend_position", _moveDirection);
            _animationTree.Set("parameters/conditions/Idle", false);
            _animationTree.Set("parameters/conditions/Walk", true);
        }
    }

	private void UsingTool(bool isActive)
	{
		_farmTool.Position = this.Position;
		_farmTool.Visible = isActive;
		_farmToolAnim.Active = isActive;
	}

}

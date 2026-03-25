using Godot;
using System;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;

public partial class PlayerController : CharacterBody2D
{
	[Export]
	public int Speed = 400;

	[Export]
	public int PlayerIndex = 1;

	private AnimationTree _animationTree;
	private Vector2 _moveDirection;
	private Node2D _farmTool;
	private AnimationTree _farmToolAnim;
	private Marker2D _viewDirectionMarker;
	private Area2D _viewDirectionArea;
	private CollisionShape2D _viewDirectionCollision;
	private PackedScene _dirtHolePreload;
	private Area2D _actionables;
	private Sprite2D _notify;
	private MainRoomController _mainGame;

	private bool _canMove = true;
	private bool _canAction = true;
	private string _basePosition = "DOWN";

	private string ActionLeft => PlayerIndex == 1 ? "p1_left" : "p2_left";
	private string ActionRight => PlayerIndex == 1 ? "p1_right" : "p2_right";
	private string ActionUp => PlayerIndex == 1 ? "p1_up" : "p2_up";
	private string ActionDown => PlayerIndex == 1 ? "p1_down" : "p2_down";
	private string ActionSelect => PlayerIndex == 1 ? "p1_action" : "p2_action";

    public override void _Ready()
	{
		_animationTree = GetNode<AnimationTree>("AnimationTree");
		_farmTool = GetNode<Node2D>("FarmTool");
		_farmToolAnim = _farmTool.GetNode<AnimationTree>("AnimationTree");
		_viewDirectionMarker = GetNode<Marker2D>("ViewDirection");
		_viewDirectionArea = _viewDirectionMarker.GetNode<Area2D>("Area2D");
		_viewDirectionCollision = _viewDirectionArea.GetNode<CollisionShape2D>("CollisionShape2D");
		_actionables = GetNode<Area2D>("Actionables");
		_notify = GetNode<Sprite2D>("Notify");
		_mainGame = GetParent<MainRoomController>();

        _animationTree.Active = true;
		_viewDirectionArea.BodyEntered += CheckGround;
		_dirtHolePreload = GD.Load<PackedScene>("res://Crops/hole.tscn");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_canMove)
		{
			ValidateInput();
			AnimatePlayer();
			MoveAndSlide();
			CheckPositionView();
			CheckActionables();
		}

	}

	public override async void _UnhandledInput(InputEvent @event)
	{
		if (_canMove && _canAction && _mainGame.SlotInUse != null)
		{
			if (@event.IsActionPressed(ActionSelect) && _mainGame.SlotInUse.TextureName == "FarmSeeds")
			{
				_viewDirectionCollision.Disabled = false;
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
				_viewDirectionCollision.Disabled = true;
			}
		}
	}

	private void CheckPositionView()
	{
		_viewDirectionCollision.Position = new Vector2(0, 16);

		var resultX = Mathf.RoundToInt(_viewDirectionCollision.GlobalPosition.X) % 16;
		var resultY = Mathf.RoundToInt(_viewDirectionCollision.GlobalPosition.Y) % 16;

		if (resultX > 0)
			resultX = Mathf.RoundToInt(_viewDirectionCollision.GlobalPosition.X) - resultX + 8;
		else if (resultX < 0)
			resultX = Mathf.RoundToInt(_viewDirectionCollision.GlobalPosition.X) - resultX - 8;

		if (resultY > 0)
			resultY = Mathf.RoundToInt(_viewDirectionCollision.GlobalPosition.Y) - resultY + 8;
		else if (resultY < 0)
			resultY = Mathf.RoundToInt(_viewDirectionCollision.GlobalPosition.Y) - resultY - 8;

		_viewDirectionCollision.GlobalPosition = new Vector2(resultX, resultY);
	}

	private async Task EnableAction()
	{
		await ToSignal(GetTree().CreateTimer(0.2f), SceneTreeTimer.SignalName.Timeout);
		_canAction = true;
	}

	private void ValidateInput()
	{
		_moveDirection = Input.GetVector(ActionLeft, ActionRight, ActionUp, ActionDown);
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
			_viewDirectionMarker.Rotation = Mathf.DegToRad(0);
			_basePosition = "DOWN";

			if (Velocity.X < 0)
			{
				_viewDirectionMarker.Rotation = Mathf.DegToRad(90);
				_basePosition = "LEFT";
			}
			else if (Velocity.X > 0)
			{
				_viewDirectionMarker.Rotation = Mathf.DegToRad(-90);
				_basePosition = "RIGHT";
			}
			else if (Velocity.Y < 0)
			{
				_viewDirectionMarker.Rotation = Mathf.DegToRad(180);
				_basePosition = "UP";
			}

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

	private void CheckGround(Node2D body)
	{
		var dirtHoleInst = _dirtHolePreload.Instantiate<DefineDirtHole>();
		dirtHoleInst.Position = _viewDirectionCollision.GlobalPosition;
		dirtHoleInst.RandomCrop = int.Parse(_mainGame.SlotInUse.idTexture);
		GetParent().CallDeferred("add_child", dirtHoleInst);
	}

	private void CheckActionables()
	{
		var areas = _actionables.GetOverlappingAreas();
        
		if(areas.Count <= 0)
		{
			_notify.Visible = false; 
			return;
        }

		foreach(var area in areas)
		{
			if (area.GetParent<DefineDirtHole>().FlagReady)
			{
				_notify.Visible = true;
				area.GetParent<DefineDirtHole>().HarvestCrop();
            }
		}

	}
}

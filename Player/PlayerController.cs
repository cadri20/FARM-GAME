using FarmGame.Inventory;
using Godot;
using System;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;

public partial class PlayerController : CharacterBody2D
{
    [Signal]
    public delegate void ShowDialogEventHandler(string who, string dialogId);

    [Export]
	public int Speed = 400;

	[Export]
	public int PlayerIndex = 1;

	public Inventory Inventory { get; set; }

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
	private Sprite2D _farmToolSprite;

	private bool _canMove = true;
	private bool _canAction = true;
	private string _basePosition = "DOWN";

	private string ActionLeft => PlayerIndex == 1 ? "p1_left" : "p2_left";
	private string ActionRight => PlayerIndex == 1 ? "p1_right" : "p2_right";
	private string ActionUp => PlayerIndex == 1 ? "p1_up" : "p2_up";
	private string ActionDown => PlayerIndex == 1 ? "p1_down" : "p2_down";
	private string ActionSelect => PlayerIndex == 1 ? "p1_action" : "p2_action";
	private string ActionTalk => PlayerIndex == 1 ? "p1_talk" : "p2_talk";

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
		_farmToolSprite = _farmTool.GetNode<Sprite2D>("Sprite2D");
        _mainGame = GetParent<MainRoomController>();

        _animationTree.Active = true;
		_viewDirectionArea.BodyEntered += CheckGround;
		_dirtHolePreload = GD.Load<PackedScene>("res://Crops/hole.tscn");

		var uiRoute = $"/root/Game2D/HBoxContainer/{(PlayerIndex == 1 ? "Left" : "Right")}ViewportContainer/{(PlayerIndex == 1 ? "Left" : "Right")}SubViewport/{(PlayerIndex == 1 ? "Left" : "Right")}UI/BottomCenter/Dialog";
        var dialogController = GetNode<DialogController>(uiRoute);
		dialogController.DialogStarted += (who, text) => 
		{
			_canMove = who != $"player{PlayerIndex}"; 
		};
		dialogController.DialogEnded += (who) => { _canMove = true; };
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
			CheckViewActionables();
        }

	}

	public override async void _UnhandledInput(InputEvent @event)
	{
		if (_canMove && _canAction && _mainGame.SlotInUse != null)
		{
			if (@event.IsActionPressed(ActionSelect))
			{
                await TimerCollision();
                if (_mainGame.SlotInUse.TextureName == "PlayerTools")
                {
                    _canMove = false;
                    _canAction = false;
                    _farmToolSprite.Texture = GD.Load<Texture2D>($"res://Tools/PlayerTool_{_mainGame.SlotInUse.idTexture}.png");
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

			if (@event.IsActionPressed(ActionTalk))
			{
				await TimerCollision();
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

	private async Task TimerCollision()
	{
		_viewDirectionCollision.Disabled = false;
		await ToSignal(GetTree().CreateTimer(0.025f), SceneTreeTimer.SignalName.Timeout);
        _viewDirectionCollision.Disabled = true;
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
		if(body is TileMap)
		{
            if (_mainGame.SlotInUse.TextureName == "PlayerTools" && _mainGame.SlotInUse.idTexture == "0")
            {
                if (ValidateIfHole() == null)
                {
                    var dirtHoleInst = _dirtHolePreload.Instantiate<DefineDirtHole>();
                    dirtHoleInst.GlobalPosition = new Vector2(float.Round(_viewDirectionCollision.GlobalPosition.X), float.Round(_viewDirectionCollision.GlobalPosition.Y));
                    dirtHoleInst.Name = $"DirtHole_{float.Round(_viewDirectionCollision.GlobalPosition.X)}_{float.Round(_viewDirectionCollision.GlobalPosition.Y)}";
                    GetParent().CallDeferred("add_child", dirtHoleInst);
                }
            }

            if (_mainGame.SlotInUse.TextureName == "FarmSeeds")
            {
                var dirtHole = ValidateIfHole();
                if (dirtHole != null)
                {
                    dirtHole.RandomCrop = int.Parse(_mainGame.SlotInUse.idTexture);
                    dirtHole.SetupCrop();
                    Inventory.RemoveItem(new Item(_mainGame.SlotInUse.TextureName, _mainGame.SlotInUse.idTexture), 1);
                }
            }
        }

		
		if(_mainGame.SlotInUse.TextureName == "SimpleItem" && _mainGame.SlotInUse.idTexture == "1")
		{
			var pot = ValidateIfPot();
			if(pot != null)
			{
				bool opened = pot.OpenCookingUI(Inventory, PlayerIndex);
				if (opened)
				{
					_canMove = false;
					_canAction = false;
					pot.CookingUIClosed += OnCookingUIClosed;
				}
			}
        }
    }

	private void OnCookingUIClosed()
	{
		_canMove = true;
		_canAction = true;
		// Disconnect so it doesn't accumulate on repeated uses
		var pot = ValidateIfPot();
		if (pot != null)
			pot.CookingUIClosed -= OnCookingUIClosed;
	}

    private DefineDirtHole ValidateIfHole()
	{
		var holeName = $"DirtHole_{float.Round(_viewDirectionCollision.GlobalPosition.X)}_{float.Round(_viewDirectionCollision.GlobalPosition.Y)}";
		return GetParent().GetNodeOrNull<DefineDirtHole>(holeName);
	}

	private Pot ValidateIfPot()
	{
		var areas = _actionables.GetOverlappingAreas();
		
		foreach(var area in areas)
		{
			if (area.GetParent() is Pot pot)
			{
				return pot;
			}
		}
		return null;
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
			if ((area.GetParent() is DefineDirtHole dirtHole) && dirtHole.FlagReady)
			{
				_notify.Visible = true;
				var cropItem = dirtHole.HarvestCrop();
				Inventory.AddItem(cropItem, 1); 
            }

			if(area.GetParent() is Npc npc)
			{
				npc.CanMove = false;
			}
		}

	}

	private void CheckViewActionables()
	{
		if (!Input.IsActionPressed(ActionTalk))
		{
			return;
		}
		var areas = _viewDirectionArea.GetOverlappingAreas();
		foreach(var area in areas)
		{
			if(area.GetParent() is Npc npc)
			{
				npc.CanMove = false;
				EmitSignal(SignalName.ShowDialog, $"player{PlayerIndex}", npc.DialogId);
            }
        }
    }
}

using Godot;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

public partial class InventoryHighlight : Node2D
{
	private TextureRect _inventoryNode;
	private MainRoomController _mainGame;

	public bool FlagWheel = false;
	public int IdSlot = 1;

	private string _nameSlot = "InventorySlot";



	public override async void _Ready()
	{
		//if(OS.IsDebugBuild())
		//	Debugger.Launch();
		_mainGame = GetNode<MainRoomController>("/root/Game2D/HBoxContainer/LeftViewportContainer/LeftSubViewport/Level2D");
		_inventoryNode = GetParent().GetNode<TextureRect>("Inventory");
		await ChangePosition();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _Input(InputEvent @event)
	{
		if (FlagWheel && @event is InputEventMouseButton buttonEvent)
		{
			if (buttonEvent.ButtonIndex == MouseButton.WheelUp)
			{
				FlagWheel = false;
				MoveSlotDown();
			}
			else if(buttonEvent.ButtonIndex == MouseButton.WheelDown)
            {
				FlagWheel = false;
				MoveSlotUp();
			}
		}
	}

	private async void MoveSlotUp()
	{
		if (IdSlot == 10)
			IdSlot = 1;
		else
		{
			IdSlot += 1;
		}
		await ChangePosition();
	}
    private async void MoveSlotDown()
    {
        if (IdSlot == 1)
            IdSlot = 10;
        else
        {
            IdSlot -= 1;
        }
        await ChangePosition();
    }

    private async Task ChangePosition()
	{
		var slot = _inventoryNode.GetNode<PrefabInventorySlot>(_nameSlot + IdSlot);
		GlobalPosition = new Vector2(slot.GlobalPosition.X + 5, slot.GlobalPosition.Y + 5);

		_mainGame.SetupSlotInUse(slot.InUse ? slot : null);
        await ToSignal(GetTree().CreateTimer(0.025), SceneTreeTimer.SignalName.Timeout);
        FlagWheel = true;
    }
}

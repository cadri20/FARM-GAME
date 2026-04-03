using Godot;
using System;
using System.Linq;

public partial class UiController : CanvasLayer
{
	[Export]
	public double SpeedMultiplier { get; set; }

	[Export]
	public bool IsPrimary { get; set; }

	public double TimeInSeconds => _timeInSeconds;
	public bool IsNight => _isNight;

	private RichTextLabel _hourText;
	private RichTextLabel _dayText;
	private RichTextLabel _monthText;
	private MainRoomController _mainGame;
	public Control SlotsInventory;

	private bool _isNight = false;
	private double _timeInSeconds = 0.0;
	private string _center = "[center]";

	public override void _Ready()
	{
		_hourText = GetNode<RichTextLabel>("TopRight/Hour");
		_dayText = GetNode<RichTextLabel>("TopRight/Day");
		_monthText = GetNode<RichTextLabel>("TopRight/Month");
		SlotsInventory = GetNode<Control>("BottomCenter/Inventory");

        _mainGame = GetNode<MainRoomController>("%Level2D");
		_mainGame.Connect("DayChanged", new Callable(this, nameof(OnDayChanged)));

		var inventoryHighlight = GetNode<InventoryHighlight>("BottomCenter/InventoryHighlight");

		inventoryHighlight.PlayerId = IsPrimary ? 1 : 2;
    }

	public override void _Process(double delta)
	{
		if (!IsPrimary)
			return;

		_timeInSeconds += delta * SpeedMultiplier;
		var hours = (int)(_timeInSeconds / 3600) % 24;
		var minutes = (int)((_timeInSeconds / 60) % 60);

		if (hours == 18 && !_isNight)
		{
			_isNight = true;
			_mainGame.StartNight();
		}
		if (_timeInSeconds >= 24 * 3600)
		{
			_timeInSeconds = 0.0;
			_isNight = false;
			_mainGame.FinishNight();
		}

		_hourText.Text = _center + $"{hours:00}:{minutes:00}";
	}

	public void SyncTime(double timeInSeconds, bool isNight)
	{
		_timeInSeconds = timeInSeconds;
		_isNight = isNight;

		var hours = (int)(_timeInSeconds / 3600) % 24;
		var minutes = (int)((_timeInSeconds / 60) % 60);
		_hourText.Text = _center + $"{hours:00}:{minutes:00}";
	}

	private void OnDayChanged()
	{
		_dayText.Text = _center + $"{_mainGame.Day:00}";
		_monthText.Text = _center + $"{_mainGame.Month:00}";
	}

	public void CropRecolected(string idObjectName, string idObject, int value)
	{
		if(SlotsInventory == null)
		{
            SlotsInventory = GetNode<Control>("BottomCenter/Inventory");
        }
		var slots = SlotsInventory.GetChildren().Select(s => s as PrefabInventorySlot);
		foreach(var slot in slots)
		{
            if (slot.InUse)
			{
				if(slot.TextureName == idObjectName && slot.idTexture == idObject)
				{
                    slot.UpdateText(value);
					break;
                }
            }
			else
			{
                slot.SetupSlot(idObjectName, idObject, value);
				break;
            }
        }


        
    }

	public void SetupSlot(int slotNumber, string textureGroup, string idObject, int value)
	{
		var slot = GetNode<PrefabInventorySlot>("BottomCenter/Inventory/InventorySlot" + slotNumber);
		slot.SetupSlot(textureGroup, idObject, value);
    }
}

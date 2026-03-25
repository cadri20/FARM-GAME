using Godot;
using System;

public partial class UiController : CanvasLayer
{
	[Export]
	public double SpeedMultiplier { get; set; }

	[Export]
	public bool IsPrimary { get; set; } = false;

	public double TimeInSeconds => _timeInSeconds;
	public bool IsNight => _isNight;

	private RichTextLabel _hourText;
	private RichTextLabel _dayText;
	private RichTextLabel _monthText;
	private MainRoomController _mainGame;
	public PrefabInventorySlot Slot1;
	public PrefabInventorySlot Slot2;
	public PrefabInventorySlot Slot3;
	public PrefabInventorySlot Slot4;

	private bool _isNight = false;
	private double _timeInSeconds = 0.0;
	private string _center = "[center]";

	public override void _Ready()
	{
		_hourText = GetNode<RichTextLabel>("TopRight/Hour");
		_dayText = GetNode<RichTextLabel>("TopRight/Day");
		_monthText = GetNode<RichTextLabel>("TopRight/Month");
		Slot1 = GetNode<PrefabInventorySlot>("BottomCenter/Inventory/InventorySlot1");
		Slot2 = GetNode<PrefabInventorySlot>("BottomCenter/Inventory/InventorySlot2");
		Slot3 = GetNode<PrefabInventorySlot>("BottomCenter/Inventory/InventorySlot3");
		Slot4 = GetNode<PrefabInventorySlot>("BottomCenter/Inventory/InventorySlot4");

        _mainGame = GetNode<MainRoomController>("%Level2D");
		_mainGame.Connect("DayChanged", new Callable(this, nameof(OnDayChanged)));
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
		var slot = GetNode<PrefabInventorySlot>("BottomCenter/Inventory/InventorySlot" + idObject);

        if (slot.InUse)
            slot.UpdateText(value);
        else
            slot.SetupSlot(idObjectName, idObject, value);
    }

	public void SetupSlot(int slotNumber, string textureGroup, string idObject, int value)
	{
		var slot = GetNode<PrefabInventorySlot>("BottomCenter/Inventory/InventorySlot" + slotNumber);
		slot.SetupSlot(textureGroup, idObject, value);
    }
}

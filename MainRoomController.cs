using Godot;
using System;

public partial class MainRoomController : Node2D
{
	[Signal]
	public delegate void DayChangedEventHandler();

	private CanvasModulate _nightModulate;
	private UiController _uiController;

	private bool _fadeNight = false;
	private int _fadeDuration = 5;
	private double _timeElapsed = 0.0;

	public int Day = 1;
	public int Month = 1;
    public PrefabInventorySlot SlotInUse;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		_nightModulate = GetNode<CanvasModulate>("NightModulate");
		_uiController = GetNode<UiController>("%LeftUI");
		SetInventoryTemporal();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (_fadeNight)
		{
			if(_timeElapsed < _fadeDuration)
			{
				_timeElapsed += 0.05;
				var newColor = (float)Mathf.Lerp(1.0, 0.5, _timeElapsed / _fadeDuration);
				_nightModulate.Color = new Color(newColor, newColor, newColor);
			}
			else
			{
				_fadeNight = false;
				_timeElapsed = 0;

			}
		}
	}

	public void StartNight()
	{
		_fadeNight = true;
	}

	public void FinishNight()
	{
		Day++;
		if(Day > 28)
		{
			Day = 1;
			Month++;
		}

		_nightModulate.Color = new Color(1, 1, 1);
		EmitSignal(SignalName.DayChanged);
    }

	public void CropRecoleted(string idObjectName, string idObject, int value)
	{
		_uiController.CropRecolected(idObjectName, idObject, value);
    }

	public void SetupSlotInUse(PrefabInventorySlot slot)
	{
		SlotInUse = slot;
    }

	private void SetInventoryTemporal()
	{
		_uiController.SetupSlot(3, "FarmSeeds", "0", 1);
		_uiController.SetupSlot(4, "FarmSeeds", "1", 1);
    }
}

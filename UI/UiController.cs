using Godot;
using System;

public partial class UiController : CanvasLayer
{
	[Export]
	public double SpeedMultiplier { get; set; } = 1000;

	private RichTextLabel _hourText;
	private RichTextLabel _dayText;
	private RichTextLabel _monthText;
	private MainRoomController _mainGame;

	private bool _isNight = false;
	private double _timeInSeconds = 0.0f;
	private string _center = "[center]";




	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_hourText = GetNode<RichTextLabel>("Control/Hour");
		_dayText = GetNode<RichTextLabel>("Control/Day");
		_monthText = GetNode<RichTextLabel>("Control/Month");

		_mainGame = GetNode<MainRoomController>("/root/Main");
		_mainGame.Connect("DayChanged", new Callable(this, nameof(OnDayChanged)));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		_timeInSeconds += delta * SpeedMultiplier;
		var hours = (int)(_timeInSeconds / 3600) % 24;
		var minutes = (int)((_timeInSeconds / 60) % 60);

		if(hours == 18 && !_isNight)
		{
			_isNight = true;
			_mainGame.StartNight();
        }
		if(_timeInSeconds >= 24 * 3600)
		{
			_timeInSeconds = 0.0;
			_isNight = false;
			_mainGame.FinishNight();
		}

		_hourText.Text = _center + $"{hours:00}:{minutes:00}";
    }

	private void OnDayChanged()
	{
		_dayText.Text = _center + $"{_mainGame.Day:00}";
		_monthText.Text = _center + $"{_mainGame.Month:00}";
    }


}

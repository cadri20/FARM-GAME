using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class Game2d : Control
{
	private UiController _primaryUI;
	private UiController _secondaryUI;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var players =
		   new[]
		   {
				new
				{
					SubViewport = GetNode<SubViewport>("%LeftSubViewport"),
					Camera = GetNode<Camera2D>("%LeftCamera2D"),
					Player = GetNode<PlayerController>("%Level2D/Player1"),
					UI = GetNode<UiController>("%LeftUI"),
				},
			   new
			   {
				   SubViewport = GetNode<SubViewport>("%RightSubViewport"),
				   Camera = GetNode<Camera2D>("%RightCamera2D"),
				   Player = GetNode<PlayerController>("%Level2D/Player2"),
				   UI = GetNode<UiController>("%RightUI"),
			   }
		   };

		players[1].SubViewport.World2D = players[0].SubViewport.World2D;

		foreach (var info in players)
		{
			var remoteTransform = new RemoteTransform2D();
			info.Player.AddChild(remoteTransform);
			remoteTransform.RemotePath = info.Camera.GetPath();
			info.Camera.Enabled = true;
        }

		_primaryUI = players[0].UI;
		_primaryUI.IsPrimary = true;

		_secondaryUI = players[1].UI;
		_secondaryUI.IsPrimary = false;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (_primaryUI != null && _secondaryUI != null)
			_secondaryUI.SyncTime(_primaryUI.TimeInSeconds, _primaryUI.IsNight);
	}
}

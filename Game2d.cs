using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class Game2d : Control
{
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
				},
			   new
			   {
				   SubViewport = GetNode<SubViewport>("%RightSubViewport"),
				   Camera = GetNode<Camera2D>("%RightCamera2D"),
				   Player = GetNode<PlayerController>("%Level2D/Player2"),
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


    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	
}

using Godot;
using System;
using System.Collections.Generic;

public partial class Pot : Node2D
{
	private List<string> ingredients = new List<string>();
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void AddIngredient(string objectId)
	{
		ingredients.Add(objectId);
		GD.Print($"Added ingredient: {objectId}");
    }


}

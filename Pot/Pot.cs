using FarmGame.Inventory;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Pot : Node2D
{
	private List<Item> ingredients = new ();
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void AddIngredient(Item item)
	{
		ingredients.Add(item);
		GD.Print($"Added ingredient: {item.TextureGroup} {item.Id}");
    }

	public Item Cook()
	{
		if (ingredients.Count == 0)
		{
			GD.Print("No ingredients to cook!");
			return new Item("SimpleItem", "2");
		}

		return null;
		
    }


}

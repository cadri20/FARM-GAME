using Godot;
using System;

public partial class PrefabInventorySlot : TextureRect
{
	private Label _textAmount;

	public bool InUse = false;
 	public int Amount = 0;

    // Called when the node enters the scene tree for the first time.

    public override void _Ready()
	{
		_textAmount = GetNode<Label>("Label");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public void SetupSlot(string textureGroup, int value)
	{
		InUse = true;
		Texture = GD.Load<Texture2D>($"res://Inventory/Icons/IcoInv_{textureGroup}.png");
		UpdateText(value);
		Visible = true;

	}

	public void UpdateText(int value)
	{
		Amount += value;
		_textAmount.Text = value.ToString();
    }
}

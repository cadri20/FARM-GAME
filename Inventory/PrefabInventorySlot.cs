using Godot;
using System;

public partial class PrefabInventorySlot : TextureRect
{
	private Label _textAmount;

	public bool InUse = false;
 	public int Amount = 0;
	public string TextureName = "";
	public string idTexture = "0";

    // Called when the node enters the scene tree for the first time.

    public override void _Ready()
	{
		_textAmount = GetNode<Label>("Label");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public void SetupSlot(string textureGroup, string idObject, int value)
	{
		GD.Print("Setting up slot with texture group: " + textureGroup + ", id: " + idObject + ", value: " + value);
		InUse = true;
		TextureName = textureGroup;
		idTexture = idObject;
        Texture = GD.Load<Texture2D>($"res://Inventory/Icons/IcoInv_{textureGroup}_{idObject}.png");
		if(textureGroup == "PlayerTools")
		{
			_textAmount ??= GetNode<Label>("Label");
            _textAmount.Visible = false;
		}else
			UpdateText(value);
		Visible = true;

	}

	public void UpdateText(int value)
	{
		_textAmount ??= GetNode<Label>("Label");

		Amount += value;
		_textAmount.Text = value.ToString();
    }

	public void ClearSlot()
	{
		InUse = false;
		TextureName = "";
		idTexture = "0";
		Texture = null;
		_textAmount ??= GetNode<Label>("Label");
		_textAmount.Text = "";
		_textAmount.Visible = true;
		Visible = false;
    }
}

using FarmGame.Inventory;
using Godot;
using System;
using System.Collections.Generic;

public partial class Inventory : TextureRect
{
	private Dictionary<Item, int> items = new();

    [Signal]
    public delegate void InventoryFullEventHandler();

    private const int MaxSlots = 10;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{

    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

	public void AddItem(Item item, int amount)
	{
		if(items.Count == MaxSlots && !items.ContainsKey(item))
		{
			//Send signal to player that inventory is full
			EmitSignal(SignalName.InventoryFull);
			return;
        }

        if (items.ContainsKey(item))
			items[item] += amount;
		else
			items[item] = amount;
		GD.Print($"Added {amount} of {item.Id} to inventory. Total: {items[item]}");
		UpdateInventoryUI();
    }

	public void RemoveItem(Item item, int amount)
	{
		if (!items.ContainsKey(item))
		{
			GD.Print($"Cannot remove {item.Id} - not in inventory.");
			return;
		}
		items[item] -= amount;
		if (items[item] <= 0)
			items.Remove(item);
		GD.Print($"Removed {amount} of {item.Id} from inventory. Remaining: {items.GetValueOrDefault(item, 0)}");
		UpdateInventoryUI();
    }

    private void UpdateInventoryUI()
	{
		int index = 1;
		foreach (var kvp in items)
		{
			if (index > MaxSlots)
				break; // No more slots available
			var item = kvp.Key;
			var amount = kvp.Value;
			GetNode<PrefabInventorySlot>($"InventorySlot{index}").SetupSlot(item.TextureGroup, item.Id, amount);
			index++;
		}
    }
}

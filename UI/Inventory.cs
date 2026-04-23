using FarmGame.Inventory;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Inventory : TextureRect
{
    private const int MaxSlots = 10;

	private InventorySlot[] _inventorySlots = new InventorySlot[MaxSlots];

    [Signal]
    public delegate void InventoryFullEventHandler();

	public int SlotsUsed => _inventorySlots.Count(slot => slot != null);

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
		if(SlotsUsed == MaxSlots && !_inventorySlots.Any(i => i.Item == item))
		{
			//Send signal to player that inventory is full
			EmitSignal(SignalName.InventoryFull);
			return;
        }

		if (_inventorySlots.Any(i => i?.Item == item))
			_inventorySlots.First(i => i?.Item == item).Amount += amount;

		else
			_inventorySlots[Array.IndexOf(_inventorySlots, null)] = new InventorySlot { Item = item, Amount = amount };
        GD.Print($"Added {amount} of {item.Id} to inventory. Total: {_inventorySlots.First(i => i.Item == item).Amount}");
		UpdateInventoryUI();
    }

	public void RemoveItem(Item item, int amount)
	{
		if (!_inventorySlots.Any(i => i?.Item == item))
		{
			GD.Print($"Cannot remove {item.Id} - not in inventory.");
			return;
		}
		var inventorySlot = _inventorySlots.First(i => i?.Item == item);
        inventorySlot.Amount -= amount;
		if (inventorySlot.Amount <= 0)
			_inventorySlots[Array.IndexOf(_inventorySlots, inventorySlot)] = null;
		GD.Print($"Removed {amount} of {item.Id} from inventory.");
		UpdateInventoryUI();
    }

    /// <summary>Returns a copy of all inventory slots (nulls included for empty positions).</summary>
    public IEnumerable<InventorySlot> GetAllSlots() => _inventorySlots.ToArray();

    private void UpdateInventoryUI()
	{
		int index = 1;
		foreach (var inventorySlot in _inventorySlots)
		{
			if (index > MaxSlots)
				break; // No more slots available

            if (inventorySlot == null)
            {
				GetNode<PrefabInventorySlot>($"InventorySlot{index}").ClearSlot();
				index++;
				continue;
            }
            var item = inventorySlot.Item;
			var amount = inventorySlot.Amount;
			GetNode<PrefabInventorySlot>($"InventorySlot{index}").SetupSlot(item.TextureGroup, item.Id, amount);
			index++;
		}
    }
}

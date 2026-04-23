using FarmGame.Inventory;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Pot : Node2D
{
    private static readonly PackedScene CookingUIScene =
        GD.Load<PackedScene>("res://Pot/cooking_ui.tscn");

    private CookingUI _activeCookingUI = null;

    // Fired when the UI closes so the player controller can re-enable movement
    [Signal]
    public delegate void CookingUIClosedEventHandler();

    public override void _Ready()
    {
    }

    public override void _Process(double delta)
    {
    }

    // ------------------------------------------------------------------
    // Called by PlayerController when the player uses SimpleItem/1 near the pot
    // ------------------------------------------------------------------
    public bool OpenCookingUI(Inventory playerInventory, int playerIndex)
    {
        if (_activeCookingUI != null)
        {
            GD.Print("[Pot] Cooking UI already open.");
            return false;
        }

        var ui = CookingUIScene.Instantiate<CookingUI>();
        // Position the UI in world space above the pot sprite
        ui.GlobalPosition = GlobalPosition + new Vector2(0, -50);
        GetParent().AddChild(ui);
        ui.Init(playerInventory, playerIndex);
        ui.CookingFinished += OnCookingFinished;
        _activeCookingUI = ui;

        GD.Print("[Pot] Cooking UI opened.");
        return true;
    }

    private void OnCookingFinished(bool success)
    {
        _activeCookingUI = null;
        GD.Print($"[Pot] Cooking finished. Success: {success}");
        EmitSignal(SignalName.CookingUIClosed);
    }

    // ------------------------------------------------------------------
    // Legacy helpers kept for compatibility
    // ------------------------------------------------------------------
    public void AddIngredient(Item item)
    {
        GD.Print($"[Pot] AddIngredient: {item.TextureGroup} {item.Id}");
    }

    /// <summary>
    /// Evaluates a list of ingredients and returns the cooked item, or null if no recipe matches.
    /// Recipe: FarmCrops/0 + FarmCrops/1 → Food/1
    /// </summary>
    public Item Cook(List<Item> ingredients = null)
    {
        if (ingredients == null || ingredients.Count == 0)
        {
            GD.Print("[Pot] No ingredients to cook!");
            return null;
        }

        bool hasCrop0 = ingredients.Any(i => i.TextureGroup == "FarmCrops" && i.Id == "0");
        bool hasCrop1 = ingredients.Any(i => i.TextureGroup == "FarmCrops" && i.Id == "1");

        if (hasCrop0 && hasCrop1)
        {
            GD.Print("[Pot] Recipe matched: FarmCrops/0 + FarmCrops/1 → Food/1");
            return new Item("Food", "1");
        }

        GD.Print("[Pot] No matching recipe for provided ingredients.");
        return null;
    }
}

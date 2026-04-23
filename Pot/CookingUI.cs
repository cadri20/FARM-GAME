using FarmGame.Inventory;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// World-space cooking UI that floats above the Pot.
/// 
/// Layout (rendered in world space, above the pot):
///   Row 0 — "Inventory" label
///   Row 1 — Player inventory slots (up to MaxDisplaySlots)  ← player picks from here
///   Row 2 — "Ingredients" label
///   Row 3 — Ingredient slots (MaxIngredientSlots)           ← items added to pot
///   Row 4 — Hint label ("Action=Add  Talk=Cook  Esc=Close")
/// 
/// Controls (uses the same player-index action strings):
///   Left / Right  → cycle selection highlight in inventory row
///   p_action      → add highlighted inventory item to next ingredient slot
///   p_talk        → attempt to cook (checks recipe)
///   p_cancel / ui_cancel → close without cooking
/// </summary>
public partial class CookingUI : Node2D
{
    // ------------------------------------------------------------------
    // Constants
    // ------------------------------------------------------------------
    private const int MaxDisplaySlots = 8;   // inventory items shown
    private const int MaxIngredientSlots = 4; // ingredient slots
    private const int SlotSize = 18;          // px per slot icon
    private const int SlotPadding = 2;        // px gap between slots
    private const int RowHeight = 22;         // px between rows

    // ------------------------------------------------------------------
    // Signals
    // ------------------------------------------------------------------
    [Signal] public delegate void CookingFinishedEventHandler(bool success);

    // ------------------------------------------------------------------
    // State
    // ------------------------------------------------------------------
    private Inventory _playerInventory;
    private int _playerIndex = 1;
    private int _selectedInvSlot = 0;        // which inventory slot is highlighted
    private List<Item> _ingredients = new();

    // UI nodes created at runtime
    private List<Sprite2D> _invIcons = new();
    private List<Sprite2D> _ingIcons = new();
    private Sprite2D _highlight;
    private Label _hintLabel;
    private Label _invLabel;
    private Label _ingLabel;

    // Snapshot of player inventory at open time (refreshed on change)
    private List<InventorySlot> _invSnapshot = new();

    // ------------------------------------------------------------------
    // Action strings (mirror PlayerController convention)
    // ------------------------------------------------------------------
    private string ActionLeft   => _playerIndex == 1 ? "p1_left"   : "p2_left";
    private string ActionRight  => _playerIndex == 1 ? "p1_right"  : "p2_right";
    private string ActionSelect => _playerIndex == 1 ? "p1_action" : "p2_action";
    private string ActionTalk   => _playerIndex == 1 ? "p1_talk"   : "p2_talk";
    private string ActionClose  => _playerIndex == 1 ? "p1_close"  : "p2_close";

    // ------------------------------------------------------------------
    // Initialization
    // ------------------------------------------------------------------

    public void Init(Inventory playerInventory, int playerIndex)
    {
        _playerInventory = playerInventory;
        _playerIndex = playerIndex;
        BuildUI();
        RefreshInventoryDisplay();
        RefreshIngredientDisplay();
        UpdateHighlight();
    }

    // ------------------------------------------------------------------
    // Input
    // ------------------------------------------------------------------

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed(ActionLeft))
        {
            _selectedInvSlot = Mathf.Max(0, _selectedInvSlot - 1);
            UpdateHighlight();
            GetViewport().SetInputAsHandled();
        }
        else if (@event.IsActionPressed(ActionRight))
        {
            _selectedInvSlot = Mathf.Min(_invSnapshot.Count - 1, _selectedInvSlot + 1);
            UpdateHighlight();
            GetViewport().SetInputAsHandled();
        }
        else if (@event.IsActionPressed(ActionSelect))
        {
            AddIngredientFromSelection();
            GetViewport().SetInputAsHandled();
        }
        else if (@event.IsActionPressed(ActionTalk))
        {
            TryCook();
            GetViewport().SetInputAsHandled();
        }
        else if (@event.IsActionPressed(ActionClose))
        {
            Close(false);
            GetViewport().SetInputAsHandled();
        }
    }

    // ------------------------------------------------------------------
    // Actions
    // ------------------------------------------------------------------

    private void AddIngredientFromSelection()
    {
        if (_ingredients.Count >= MaxIngredientSlots) return;
        if (_invSnapshot.Count == 0) return;
        if (_selectedInvSlot >= _invSnapshot.Count) return;

        var slot = _invSnapshot[_selectedInvSlot];
        if (slot == null) return;

        _ingredients.Add(slot.Item);
        _playerInventory.RemoveItem(slot.Item, 1);

        RefreshInventoryDisplay();

        // Clamp selection in case the item was the last one
        _selectedInvSlot = Mathf.Min(_selectedInvSlot, Mathf.Max(0, _invSnapshot.Count - 1));
        UpdateHighlight();
        RefreshIngredientDisplay();

        GD.Print($"[CookingUI] Added ingredient: {slot.Item.TextureGroup}/{slot.Item.Id}. Total ingredients: {_ingredients.Count}");
    }

    private void TryCook()
    {
        var result = EvaluateRecipe();
        if (result != null)
        {
            GD.Print($"[CookingUI] Cooked: {result.TextureGroup}/{result.Id}");
            _playerInventory.AddItem(result, 1);
            Close(true);
        }
        else
        {
            GD.Print("[CookingUI] No valid recipe. Returning ingredients and closing.");
            Close(false);
        }
    }

    /// <summary>Returns the cooked item if the current ingredient list matches a recipe, else null.</summary>
    private Item EvaluateRecipe()
    {
        // Recipe: FarmCrops/0 + FarmCrops/1 → Food/1
        bool hasCrop0 = _ingredients.Any(i => i.TextureGroup == "FarmCrops" && i.Id == "0");
        bool hasCrop1 = _ingredients.Any(i => i.TextureGroup == "FarmCrops" && i.Id == "1");
        if (hasCrop0 && hasCrop1)
            return new Item("Food", "1");

        return null;
    }

    private void Close(bool success)
    {
        // Return any unused ingredients
        foreach (var ing in _ingredients)
            _playerInventory.AddItem(ing, 1);
        _ingredients.Clear();

        EmitSignal(SignalName.CookingFinished, success);
        QueueFree();
    }

    // ------------------------------------------------------------------
    // UI Construction
    // ------------------------------------------------------------------

    private void BuildUI()
    {
        int totalWidth = MaxDisplaySlots * (SlotSize + SlotPadding);

        // Background panel
        var bg = new ColorRect();
        bg.Color = new Color(0, 0, 0, 0.6f);
        bg.Size = new Vector2(totalWidth + 10, RowHeight * 5 + 10);
        bg.Position = new Vector2(-5, -RowHeight * 5 - 15);
        AddChild(bg);

        // -- Inventory label --
        _invLabel = new Label();
        _invLabel.Text = "Inventory";
        _invLabel.Position = new Vector2(0, -RowHeight * 5 - 10);
        _invLabel.AddThemeFontSizeOverride("font_size", 6);
        AddChild(_invLabel);

        // -- Inventory icon row --
        for (int i = 0; i < MaxDisplaySlots; i++)
        {
            var icon = new Sprite2D();
            icon.Position = new Vector2(i * (SlotSize + SlotPadding) + SlotSize / 2, -RowHeight * 4);
            icon.Visible = false;
            AddChild(icon);
            _invIcons.Add(icon);

            // slot border
            var border = new ColorRect();
            border.Color = new Color(1, 1, 1, 0.15f);
            border.Size = new Vector2(SlotSize, SlotSize);
            border.Position = new Vector2(i * (SlotSize + SlotPadding), -RowHeight * 4 - SlotSize / 2);
            AddChild(border);
        }

        // -- Highlight sprite (drawn over selected inventory slot) --
        _highlight = new Sprite2D();
        var highlightTex = GD.Load<Texture2D>("res://Inventory/InventoryHighlight.png");
        if (highlightTex != null)
        {
            _highlight.Texture = highlightTex;
            _highlight.Scale = new Vector2(
                (float)SlotSize / highlightTex.GetWidth(),
                (float)SlotSize / highlightTex.GetHeight());
        }
        else
        {
            // Fallback: yellow rect
            var img = Image.Create(SlotSize, SlotSize, false, Image.Format.Rgba8);
            img.Fill(new Color(1, 1, 0, 0.5f));
            _highlight.Texture = ImageTexture.CreateFromImage(img);
        }
        _highlight.Position = new Vector2(SlotSize / 2, -RowHeight * 4);
        _highlight.ZIndex = 1;
        AddChild(_highlight);

        // -- Ingredients label --
        _ingLabel = new Label();
        _ingLabel.Text = "Ingredients";
        _ingLabel.Position = new Vector2(0, -RowHeight * 2 - 10);
        _ingLabel.AddThemeFontSizeOverride("font_size", 6);
        AddChild(_ingLabel);

        // -- Ingredient icon row --
        for (int i = 0; i < MaxIngredientSlots; i++)
        {
            // empty slot border
            var border = new ColorRect();
            border.Color = new Color(0.6f, 0.4f, 0.1f, 0.4f);
            border.Size = new Vector2(SlotSize, SlotSize);
            border.Position = new Vector2(i * (SlotSize + SlotPadding), -RowHeight * 1 - SlotSize / 2 - 8);
            AddChild(border);

            var icon = new Sprite2D();
            icon.Position = new Vector2(i * (SlotSize + SlotPadding) + SlotSize / 2, -RowHeight * 1 - 8);
            icon.Visible = false;
            AddChild(icon);
            _ingIcons.Add(icon);
        }

        // -- Hint label --
        _hintLabel = new Label();
        _hintLabel.Text = "←/→ Select  Action=Add  Talk=Cook  Esc/B=Close";
        _hintLabel.Position = new Vector2(0, -6);
        _hintLabel.AddThemeFontSizeOverride("font_size", 5);
        AddChild(_hintLabel);
    }

    // ------------------------------------------------------------------
    // Display refresh helpers
    // ------------------------------------------------------------------

    private void RefreshInventoryDisplay()
    {
        _invSnapshot = _playerInventory.GetAllSlots()
            .Where(s => s != null)
            .Take(MaxDisplaySlots)
            .ToList();

        for (int i = 0; i < MaxDisplaySlots; i++)
        {
            if (i < _invSnapshot.Count)
            {
                var slot = _invSnapshot[i];
                var tex = GD.Load<Texture2D>($"res://Inventory/Icons/IcoInv_{slot.Item.TextureGroup}_{slot.Item.Id}.png");
                _invIcons[i].Texture = tex;
                _invIcons[i].Scale = tex != null
                    ? new Vector2((float)(SlotSize - 2) / tex.GetWidth(), (float)(SlotSize - 2) / tex.GetHeight())
                    : Vector2.One;
                _invIcons[i].Visible = tex != null;
            }
            else
            {
                _invIcons[i].Texture = null;
                _invIcons[i].Visible = false;
            }
        }
    }

    private void RefreshIngredientDisplay()
    {
        for (int i = 0; i < MaxIngredientSlots; i++)
        {
            if (i < _ingredients.Count)
            {
                var item = _ingredients[i];
                var tex = GD.Load<Texture2D>($"res://Inventory/Icons/IcoInv_{item.TextureGroup}_{item.Id}.png");
                _ingIcons[i].Texture = tex;
                _ingIcons[i].Scale = tex != null
                    ? new Vector2((float)(SlotSize - 2) / tex.GetWidth(), (float)(SlotSize - 2) / tex.GetHeight())
                    : Vector2.One;
                _ingIcons[i].Visible = tex != null;
            }
            else
            {
                _ingIcons[i].Texture = null;
                _ingIcons[i].Visible = false;
            }
        }
    }

    private void UpdateHighlight()
    {
        if (_invSnapshot.Count == 0)
        {
            _highlight.Visible = false;
            return;
        }
        _selectedInvSlot = Mathf.Clamp(_selectedInvSlot, 0, _invSnapshot.Count - 1);
        _highlight.Position = new Vector2(
            _selectedInvSlot * (SlotSize + SlotPadding) + SlotSize / 2,
            -RowHeight * 4);
        _highlight.Visible = true;
    }
}

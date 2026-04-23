using FarmGame.Dialog;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class DialogController : Control
{
	// Called when the node enters the scene tree for the first time.
	private RichTextLabel _text;
	private Dialog _currentDialog;
	private int _currentOptionIndex = 0;
	private bool _isActive = false;

    private string _downOptionAction = "p1_down";
	private string _upOptionAction = "p1_up";
	private string _selectOptionAction = "p1_select";

    private List<TextureRect> _optionDialogs = new List<TextureRect>();

    [Signal]
    public delegate void DialogStartedEventHandler(string who, string text);

	[Signal]
	public delegate void DialogEndedEventHandler(string who);

	private UiController _uiController;

    public override void _Ready()
	{
		_text = GetNode<RichTextLabel>("Text/RichTextLabel");
		Visible = false;
        _uiController = GetParent().GetParent<UiController>();
		var playerIndex = _uiController.IsPrimary ? 1 : 2;

		_downOptionAction = $"p{playerIndex}_down";
		_upOptionAction = $"p{playerIndex}_up";
		_selectOptionAction = $"p{playerIndex}_select";

        GetNode<PlayerController>($"/root/Game2D/HBoxContainer/LeftViewportContainer/LeftSubViewport/Level2D/Player{playerIndex}").ShowDialog += ShowInitDialog;
		_optionDialogs = GetNode("Options").GetChildren().OfType<TextureRect>().ToList();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(!_isActive) return;

		if(Input.IsActionJustPressed(_downOptionAction))
		{
			_currentOptionIndex = (_currentOptionIndex + 1) % _optionDialogs.Count;
			ChangeOptionSelected();
		}
		else if(Input.IsActionJustPressed(_upOptionAction))
		{
			_currentOptionIndex = (_currentOptionIndex - 1 + _optionDialogs.Count) % _optionDialogs.Count;
			ChangeOptionSelected();
		}
		else if(Input.IsActionJustPressed(_selectOptionAction) && Visible)
		{
			OnOptionSelected(_currentOptionIndex);
        }
    }

	private void ShowInitDialog(string who, string dialogId)
	{
		var dialog = DialogManager.Instance.GetDialogById(dialogId);
        ShowDialog(dialog, who);
		_isActive = true;
    }

    private void ShowDialog(Dialog dialog, string who)
	{
		_currentDialog = dialog;
		Visible = true;
		_text.Text = dialog.Text;

		for(int i = 0; i < dialog.Options.Count; i++)
		{
			var optionText = GetNode<RichTextLabel>($"Options/Option{i + 1}/RichTextLabel");
			optionText.Text = dialog.Options[i].Text;
			optionText.GetParent<TextureRect>().Visible = true;
        }

        //Hide unused option slots
		for(int i = dialog.Options.Count; i < _optionDialogs.Count; i++)
		{
			_optionDialogs[i].Visible = false;
        }

        EmitSignal(SignalName.DialogStarted, who, dialog.Text);
    }

	private void OnOptionSelected(int optionIndex)
	{
		var dialogOptionSelected = _currentDialog.Options[optionIndex];
        if (dialogOptionSelected.IsEnd)
        {
            _isActive = false;
            Visible = false;
            EmitSignal(SignalName.DialogEnded, "player1");
            return;
        }

        var nextDialog = DialogManager.Instance.GetNextDialog(dialogOptionSelected);
		ShowDialog(nextDialog, $"player{(_uiController.IsPrimary ? 1 : 2)}");
	}

	private void ChangeOptionSelected()
	{
		var selectedOption = 
			_optionDialogs
			.Where(o => o.Name == $"Option{_currentOptionIndex + 1}")
			.FirstOrDefault();

		if (selectedOption != null)
		{
			selectedOption.Modulate = new Color(1, 1, 0); // Highlight selected option
			foreach (var optionDialog in _optionDialogs)
			{
				if (optionDialog != selectedOption)
				{
					optionDialog.Modulate = new Color(1, 1, 1); // Reset other options
                }
            }
        }

    }
}

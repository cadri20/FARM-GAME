using FarmGame.Dialog;
using Godot;
using System;

public partial class DialogController : Control
{
	// Called when the node enters the scene tree for the first time.
	private RichTextLabel _text;

    [Signal]
    public delegate void DialogStartedEventHandler(string who, string text);

    public override void _Ready()
	{
		_text = GetNode<RichTextLabel>("Text/RichTextLabel");
		Visible = false;
        GetNode<PlayerController>("/root/Game2D/HBoxContainer/LeftViewportContainer/LeftSubViewport/Level2D/Player1").ShowDialog += ShowInitDialog;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void ShowInitDialog(string who)
	{
		var dialog = DialogManager.Instance.GetDialogById("greeting");
        ShowDialog(dialog, who);
    }

    private void ShowDialog(Dialog dialog, string who)
	{
		Visible = true;
		_text.Text = dialog.Text;

		for(int i = 0; i < dialog.Options.Count; i++)
		{
			var optionText = GetNode<RichTextLabel>($"Options/Option{i + 1}/RichTextLabel");
			optionText.Text = dialog.Options[i].Text;
			optionText.GetParent<TextureRect>().Visible = true;
        }

		EmitSignal(SignalName.DialogStarted, who, dialog.Text);
    }
}

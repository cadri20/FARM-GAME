using FarmGame.Dialog;
using Godot;
using System;
using System.Collections.Generic;

public partial class DialogManager : Node
{
    public string Language { get; set; } = "es";

	public static DialogManager Instance { get; private set; }

	private List<Dialog> _dialogs = new List<Dialog>();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		Instance = this;
		LoadDialogs();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void LoadDialogs()
	{
		string path = $"res://Dialog/dialogs-{Language}.json";
		using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
		if (file == null)
		{
			_dialogs = new List<Dialog>();
			return;
		}

		var jsonString = file.GetAsText();
		var json = new Json();
		var error = json.Parse(jsonString);

		if (error == Error.Ok)
		{
			var data = json.Data;
			var dialogArray = (Godot.Collections.Array)data;
			_dialogs = new List<Dialog>();

			foreach (var item in dialogArray)
			{
				var dict = (Godot.Collections.Dictionary)item;
				var id = (string)dict["id"];
				var text = (string)dict["text"];
				var optionsList = new List<DialogOption>();

				if (dict.ContainsKey("options"))
				{
					var optionsArray = (Godot.Collections.Array)dict["options"];
					foreach (var opt in optionsArray)
					{
						var optDict = (Godot.Collections.Dictionary)opt;
						var optText = (string)optDict["text"];
						var nextId = (string)optDict["nextId"];
						var isEnd = optDict.ContainsKey("isEnd") ? (bool)optDict["isEnd"] : false;
                        optionsList.Add(new DialogOption(optText, nextId, isEnd));
					}
				}

				_dialogs.Add(new Dialog(id, text, optionsList));
			}
		}
    }

	public Dialog GetDialogById(string id)
	{
		return _dialogs.Find(d => d.Id == id);
    }

	public Dialog GetNextDialog(DialogOption option)
	{
		return GetDialogById(option.NextId);
    }
}

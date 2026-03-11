using Godot;
using System;
using System.Threading.Tasks;

public partial class DefineDirtHole : Node2D
{
	[Export]
	public double GrowSpeed = 1.5;
    
    public bool FlagReady = false;

    private int _growCropId = 0;
    private int _randomCrop = 0;
    private RandomNumberGenerator rng = new RandomNumberGenerator();
    private Sprite2D _crops;
    // Called when the node enters the scene tree for the first time.
    public override async void _Ready()
	{
        _crops = GetNode<Sprite2D>("Crops");
        _randomCrop = rng.RandiRange(0, _crops.Vframes - 1);
        _crops.Visible = true;
        _crops.FrameCoords = new Vector2I(_growCropId, _randomCrop);
        await GrowCrop();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    private async Task GrowCrop()
    {
        if (_growCropId < _crops.Hframes - 1)
        {
            await ToSignal(GetTree().CreateTimer(GrowSpeed), "timeout");
            _growCropId++;
            _crops.FrameCoords = new Vector2I(_growCropId, _randomCrop);
            await GrowCrop();
        }
        else
            FlagReady = true;
    }
}

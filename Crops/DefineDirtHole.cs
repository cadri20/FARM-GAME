using Godot;
using System;
using System.Threading.Tasks;

public partial class DefineDirtHole : Node2D
{
	[Export]
	public double GrowSpeed = 1.5;
    
    public bool FlagReady = false;
    public bool CropDisabled = false;

    private int _growCropId = 0;
    private int _randomCrop = 0;
    private RandomNumberGenerator rng = new RandomNumberGenerator();
    private Sprite2D _crops;
    private MainRoomController _mainGame;
    private CollisionShape2D _areaCollision;
    

    // Called when the node enters the scene tree for the first time.
    public override async void _Ready()
	{
        _crops = GetNode<Sprite2D>("Crops");
        _mainGame = GetParent<MainRoomController>();
        _mainGame.Connect("DayChanged", new Callable(this, nameof(GrowCrop)));

        _randomCrop = rng.RandiRange(0, _crops.Vframes - 1);
        _crops.Visible = true;
        _crops.FrameCoords = new Vector2I(_growCropId, _randomCrop);
        _areaCollision = GetNode<CollisionShape2D>("Area2D/CollisionShape2D");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    private void GrowCrop()
    {
        if (_growCropId < _crops.Hframes - 1)
        {
            _growCropId++;
            _crops.FrameCoords = new Vector2I(_growCropId, _randomCrop);
        }
        else
            FlagReady = true;
    }

    public void HarvestCrop()
    {
        if (!CropDisabled)
        {
            CropDisabled = true;
            _areaCollision.Disabled = true;
            _mainGame.CropRecoleted("FarmCrops_" + _randomCrop, 1);
            CallDeferred("queue_free");
        }
    }
}

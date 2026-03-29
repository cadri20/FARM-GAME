using Godot;
using System;
using System.Threading.Tasks;

public partial class DefineDirtHole : Node2D
{
	[Export]
	public double GrowSpeed = 1.5;
    
    public bool FlagReady = false;
    public bool CropDisabled = false;
    public int RandomCrop = 0;

    private int _growCropId = 0;
    private RandomNumberGenerator rng = new RandomNumberGenerator();
    private Sprite2D _crops;
    private MainRoomController _mainGame;
    private CollisionShape2D _areaCollision;
    

    // Called when the node enters the scene tree for the first time.
    public override async void _Ready()
	{
        _crops = GetNode<Sprite2D>("Crops");
        _mainGame = GetParent<MainRoomController>();

        //_randomCrop = rng.RandiRange(0, _crops.Vframes - 1);
        _areaCollision = GetNode<CollisionShape2D>("Area2D/CollisionShape2D");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public void SetupCrop()
    {
        _crops.Visible = true;
        _crops.FrameCoords = new Vector2I(RandomCrop, _growCropId);
        _mainGame.DayChanged += GrowCrop;
    }

    private void GrowCrop()
    {
        if (_growCropId < _crops.Hframes - 1)
        {
            _growCropId++;
            _crops.FrameCoords = new Vector2I(_growCropId, RandomCrop);
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
            _mainGame.CropRecoleted("FarmCrops", (RandomCrop).ToString(), 1);
            CallDeferred("queue_free");
        }
    }
}

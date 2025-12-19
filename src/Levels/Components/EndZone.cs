using Godot;

public partial class EndZone : Area2D
{
	[Export(PropertyHint.File, "*.tscn")] public string NextLevelPath;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is PlayerController)
		{
			GD.Print("Level Complete!");
			// If we have a next level set, load it. Otherwise just print win.
			if (!string.IsNullOrEmpty(NextLevelPath))
			{
				GetTree().ChangeSceneToFile(NextLevelPath);
			}
		}
	}
}

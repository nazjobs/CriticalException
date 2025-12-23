using Godot;

public partial class MainMenu : Control
{
	// Drag your Level 1 scene here in the Inspector
	[Export(PropertyHint.File, "*.tscn")] public string FirstLevelPath;

	public override void _Ready()
	{
		// Connect buttons automatically
		GetNode<Button>("VBoxContainer/PlayButton").Pressed += OnPlayPressed;
		GetNode<Button>("VBoxContainer/QuitButton").Pressed += OnQuitPressed;
	}

	private void OnPlayPressed()
	{
		if (!string.IsNullOrEmpty(FirstLevelPath))
		{
			GetTree().ChangeSceneToFile(FirstLevelPath);
		}
		else
		{
			GD.Print("ERROR: No Level 1 path assigned in Inspector!");
		}
	}

	private void OnQuitPressed()
	{
		GD.Print("Quitting...");
		GetTree().Quit();
	}
}

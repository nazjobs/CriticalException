using Godot;

public partial class MainMenu : Control
{
	// Drag your Level 1 scene here in the Inspector
	[Export(PropertyHint.File, "*.tscn")] public string FirstLevelPath;

	private AudioStreamPlayer _blipSound; 
	
	public override void _Ready()
	{
		// Connect buttons automatically
		GetNode<Button>("VBoxContainer/PlayButton").Pressed += OnPlayPressed;
		GetNode<Button>("VBoxContainer/QuitButton").Pressed += OnQuitPressed;
		
		// 1. Get the Sound Node
		_blipSound = GetNode<AudioStreamPlayer>("SFX_Blip");

		// 2. Connect Buttons
		GetNode<Button>("VBoxContainer/PlayButton").Pressed += OnPlayPressed;
		GetNode<Button>("VBoxContainer/QuitButton").Pressed += OnQuitPressed;
	}

	private void OnPlayPressed()
	{
		_blipSound.Play(); // Play sound
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
		_blipSound.Play(); // Play sound
		GD.Print("Quitting...");
		GetTree().Quit();
	}
}

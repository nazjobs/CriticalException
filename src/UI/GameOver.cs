using Godot;

public partial class GameOver : CanvasLayer
{
	public override void _Ready()
	{
		// Connect buttons
		GetNode<Button>("VBoxContainer/RetryButton").Pressed += OnRetryPressed;
		GetNode<Button>("VBoxContainer/MenuButton").Pressed += OnMenuPressed;
	}

	private void OnRetryPressed()
	{
		// Reload the scene we died in
		GetTree().ReloadCurrentScene();
		// Since we are adding this as a child, self-destructing isn't strictly needed 
		// as the reload wipes everything, but it's good practice.
		QueueFree();
	}

	private void OnMenuPressed()
	{
		// Go back to Main Menu
		GetTree().ChangeSceneToFile("res://src/UI/MainMenu.tscn");
	}
}

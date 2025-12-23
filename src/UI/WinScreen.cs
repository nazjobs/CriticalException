using Godot;

public partial class WinScreen : Control
{
	public override void _Ready()
	{
		// Wait 5 seconds, then Quit (or go to Main Menu later)
		GetTree().CreateTimer(5.0).Timeout += () => GetTree().Quit();
	}
}

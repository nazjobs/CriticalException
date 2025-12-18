using Godot;

public partial class KillFloor : Area2D
{
	public override void _Ready()
	{
		// Godot Signal: Fires when a physics body (like the Player) enters
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		// Check if the body is the Player
		if (body is PlayerController)
		{
			GD.Print("Player fell into the void!");
			CallDeferred("ReloadLevel");
		}
		// Optional: Delete falling enemies
		else if (body is CharacterBody2D)
		{
			body.QueueFree();
		}
	}

	private void ReloadLevel()
	{
		GetTree().ReloadCurrentScene();
	}
}

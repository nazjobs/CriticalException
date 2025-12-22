using Godot;
public partial class Projectile : Area2D
{
	[Export] public float Speed = 600f;
	[Export] public float Lifetime = 2.0f;
	public Vector2 Direction = Vector2.Right;

	public override void _Ready()
	{
		GetTree().CreateTimer(Lifetime).Timeout += QueueFree;
		BodyEntered += OnBodyEntered;
	}

	public override void _PhysicsProcess(double delta)
	{
		GlobalPosition += Direction * Speed * (float)delta;
	}

	private void OnBodyEntered(Node2D body)
	{
		// IGNORE THE PLAYER
		// If the thing we hit is the Player, do nothing (let the bullet pass through)
		if (body is PlayerController) return;

		// If we hit anything else (Walls, Floor), destroy the bullet
		QueueFree();
	}
}

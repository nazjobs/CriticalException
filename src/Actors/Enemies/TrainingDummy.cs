using Godot;

public partial class TrainingDummy : CharacterBody2D, IDamageable
{
	[Export] public int Health = 10;

	public void TakeDamage(int amount, Vector2 knockback)
	{
		Health -= amount;
		
		// 1. Apply Knockback (Simple physics impulse)
		Velocity = knockback;
		MoveAndSlide(); // Apply the velocity immediately

		// 2. Visual Feedback (Flash White)
		Modulate = Colors.Red; // Flash red
		GetTree().CreateTimer(0.1).Timeout += () => Modulate = Colors.White; // Reset

		GD.Print($"Ouch! HP Left: {Health}");

		if (Health <= 0)
		{
			Die();
		}
	}

	private void Die()
	{
		GD.Print("Dummy Destroyed!");
		QueueFree(); // Delete object
	}
	
	// Add simple friction so he stops sliding after being hit
	public override void _PhysicsProcess(double delta)
	{
		Velocity = Velocity.MoveToward(Vector2.Zero, 200 * (float)delta);
		MoveAndSlide();
	}
}

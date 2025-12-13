using Godot;

public partial class NullPointer : CharacterBody2D, IDamageable
{
	[Export] public float Speed = 150.0f;
	[Export] public int Health = 2; // Takes 2 hits to kill
	
	// Direction: 1 = Right, -1 = Left
	private int _direction = 1; 

	public override void _PhysicsProcess(double delta)
	{
		// 1. Move
		Vector2 velocity = Velocity;
		velocity.X = Speed * _direction;
		Velocity = velocity;

		MoveAndSlide();

		// 2. AI Logic: Wall Detection
		// If we hit a wall, turn around
		if (IsOnWall())
		{
			FlipDirection();
		}
	}

	private void FlipDirection()
	{
		_direction *= -1;
		
		// Visual Flip (Flip the sprite container if you have one, or just scale)
		// Note: Using Scale.X on the root can mess up physics sometimes, 
		// but for simple symmetric drones it's fine.
		Vector2 scale = Scale;
		scale.X *= -1; 
		// We actually want to flip the VISUALS, not the root usually, 
		// but let's keep it simple for now. 
		// Better: Flip the Sprite2D.
		GetNode<Sprite2D>("Sprite2D").FlipH = (_direction < 0);
	}

	// --- COMBAT INTERFACE ---
	public void TakeDamage(int amount, Vector2 knockback)
	{
		Health -= amount;
		
		// Knockback (optional for drones, maybe they just stutter)
		Velocity += knockback; 

		// Flash White
		Modulate = Colors.Red;
		GetTree().CreateTimer(0.1).Timeout += () => Modulate = Colors.Green;

		if (Health <= 0)
		{
			Die();
		}
	}

	private void Die()
	{
		// Poof!
		QueueFree();
	}
}

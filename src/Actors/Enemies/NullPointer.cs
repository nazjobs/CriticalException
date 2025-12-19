using Godot;

public partial class NullPointer : CharacterBody2D, IDamageable
{
	[Export] public float Speed = 100.0f;
	[Export] public int Health = 2;

	private int _direction = 1;
	private bool _isInvincible = false; // <--- THIS PREVENTS ONE-SHOTS

	public override void _PhysicsProcess(double delta)
	{
		Velocity = new Vector2(Speed * _direction, 0);
		MoveAndSlide();

		if (IsOnWall())
		{
			FlipDirection();
		}
	}

	private void FlipDirection()
	{
		_direction *= -1;
		// Flip the sprite visual
		GetNode<Sprite2D>("Sprite2D").FlipH = (_direction < 0);
	}

	public void TakeDamage(int amount, Vector2 knockback)
	{
		if (_isInvincible) return; // <--- IGNORE DAMAGE IF ALREADY HIT

		GD.Print($"Enemy took {amount} damage. Health: {Health} -> {Health - amount}"); // <--- ADD THIS
		Health -= amount;
		
		// Visual Feedback
		var sprite = GetNode<Sprite2D>("Sprite2D");
		sprite.Modulate = Colors.Red;
		
		// Turn on Invincibility for 0.4 seconds
		_isInvincible = true;
		
		GetTree().CreateTimer(0.4).Timeout += () => 
		{
			if (IsInstanceValid(sprite)) sprite.Modulate = Colors.Green; // Reset color
			_isInvincible = false; // Reset invincibility
		};

		if (Health <= 0)
		{
			Die();
		}
	}

	private void Die()
	{
		QueueFree();
	}
}

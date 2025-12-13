using Godot;

public interface IDamageable
{
	// Amount: How much GPA/Health to lose
	// Knockback: Which direction and how hard to fly backward
	void TakeDamage(int amount, Vector2 knockback);
}

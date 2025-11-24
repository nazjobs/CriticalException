using Godot;

public partial class Hitbox : Area2D
{
	[Export] public int Damage = 1;
	[Export] public float KnockbackForce = 300f;

	public override void _Ready()
	{
		AreaEntered += OnAreaEntered;
	}

	private void OnAreaEntered(Area2D area)
	{
		if (area is Hurtbox hurtbox)
		{
			if (hurtbox.HealthOwner is IDamageable target)
			{
				// Clean combat is silent combat
				DealDamage(target, GlobalPosition);
			}
		}
	}

	public void DealDamage(IDamageable target, Vector2 ownerPosition)
	{
		Vector2 direction = (target is Node2D node) 
			? (node.GlobalPosition - ownerPosition).Normalized() 
			: Vector2.Right;

		target.TakeDamage(Damage, direction * KnockbackForce);
	}
}

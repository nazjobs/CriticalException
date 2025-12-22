using Godot;

public partial class SpamBot : CharacterBody2D, IDamageable
{
	[Export] public int Health = 3;
	[Export] public float FireRate = 2.0f; // Time between shots
	[Export] public float DetectionRange = 400f; // How far it can see
	[Export] public PackedScene ProjectileScene; // Drag 'SpamProjectile.tscn' here

	private Node2D _player;
	private Timer _shootTimer;
	private Marker2D _muzzle;

	public override void _Ready()
	{
		// 1. Find Player (Make sure Player is in group "Player")
		_player = GetTree().GetFirstNodeInGroup("Player") as Node2D;
		
		// 2. Get Muzzle
		_muzzle = GetNode<Marker2D>("Muzzle");

		// 3. Setup Timer
		_shootTimer = new Timer();
		_shootTimer.WaitTime = FireRate;
		_shootTimer.Timeout += AttemptShoot;
		AddChild(_shootTimer);
		_shootTimer.Start();
	}

	private void AttemptShoot()
	{
		if (_player == null) return;

		float dist = GlobalPosition.DistanceTo(_player.GlobalPosition);
		if (dist > DetectionRange) return;

		if (ProjectileScene != null)
		{
			var bullet = ProjectileScene.Instantiate<Projectile>();
			bullet.GlobalPosition = _muzzle.GlobalPosition;
			
			// --- FIX 1: Aim at Chest, not Feet ---
			// Assuming player is ~32 pixels tall, offset Y by -12 to -16
			Vector2 playerCenter = _player.GlobalPosition + new Vector2(0, -12);

			// --- FIX 2: Calculate Direction from MUZZLE, not Body ---
			Vector2 direction = (playerCenter - _muzzle.GlobalPosition).Normalized();
			
			bullet.Direction = direction;
			
			// --- FIX 3: Rotate the sprite to face the player ---
			bullet.Rotation = direction.Angle(); 
			
			GetTree().Root.AddChild(bullet);
		}
	}

	public void TakeDamage(int amount, Vector2 knockback)
	{
		Health -= amount;
		Modulate = Colors.Red;
		GetTree().CreateTimer(0.1).Timeout += () => Modulate = Colors.White;
		
		if (Health <= 0) Die();
	}
	
	public void Die() 
	{ 
		QueueFree(); 
	}
}

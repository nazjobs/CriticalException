using Godot;
using System;

public partial class PlayerController : CharacterBody2D, IDamageable
{
	[ExportCategory("Movement Stats")]
	[Export] public float Speed = 300.0f;
	[Export] public float JumpVelocity = -500.0f;
	[Export] public float DoubleJumpVelocity = -400.0f;
	[Export] public float Gravity = 1200.0f;
	[Export] public float Acceleration = 1500.0f;
	[Export] public float Friction = 1200.0f;

	[ExportCategory("Game Feel")]
	[Export] public float JumpCutValue = 0.5f;
	[Export] public double CoyoteTime = 0.1;
	[Export] public double JumpBufferTime = 0.1;
	[Export] public int MaxJumps = 2;

	[ExportCategory("Combat")]
	[Export] public int MaxHealth = 4;
	[Export] public float AttackDuration = 0.2f;

	// --- Components ---
	private AnimationPlayer _animPlayer;
	private Sprite2D _sprite;
	private Area2D _hitboxArea;
	private Area2D _weaponPivot;

	// --- State Variables ---
	private int _currentHealth;
	private double _coyoteTimer = 0;
	private double _jumpBufferTimer = 0;
	private int _jumpCount = 0;
	private bool _isAttacking = false;

	public override void _Ready()
	{
		_currentHealth = MaxHealth;
		// Grab references
		_animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_sprite = GetNode<Sprite2D>("Sprite2D");
		_hitboxArea = GetNode<Area2D>("WeaponPivot/MeleeHitbox");
		_weaponPivot = GetNode<Area2D>("WeaponPivot");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_currentHealth <= 0) return; // Dead players don't move

		Vector2 velocity = Velocity;

		// 1. Timers
		if (_coyoteTimer > 0) _coyoteTimer -= delta;
		if (_jumpBufferTimer > 0) _jumpBufferTimer -= delta;

		// 2. Gravity
		if (!IsOnFloor())
		{
			velocity.Y += Gravity * (float)delta;
		}
		else
		{
			_coyoteTimer = CoyoteTime;
			_jumpCount = 0;
		}

		// 3. Jump
		if (Input.IsActionJustPressed("ui_accept")) _jumpBufferTimer = JumpBufferTime;

		if (_jumpBufferTimer > 0 && _coyoteTimer > 0)
		{
			velocity.Y = JumpVelocity;
			_jumpBufferTimer = 0;
			_coyoteTimer = 0;
			_jumpCount = 1;
		}
		else if (Input.IsActionJustPressed("ui_accept") && _jumpCount < MaxJumps && _jumpCount > 0)
		{
			velocity.Y = DoubleJumpVelocity;
			_jumpCount++;
		}

		if (Input.IsActionJustReleased("ui_accept") && velocity.Y < 0)
		{
			velocity.Y *= JumpCutValue;
		}

		// 4. Movement
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		if (direction.X != 0)
		{
			velocity.X = Mathf.MoveToward(velocity.X, direction.X * Speed, Acceleration * (float)delta);
			
			// FLIP VISUALS
			_sprite.FlipH = (direction.X < 0);
			_weaponPivot.Scale = new Vector2(direction.X > 0 ? 1 : -1, 1);
		}
		else
		{
			velocity.X = Mathf.MoveToward(velocity.X, 0, Friction * (float)delta);
		}

		// 5. Attack
		if (Input.IsActionJustPressed("attack") && !_isAttacking)
		{
			PerformAttack();
		}

		// 6. ANIMATION LOGIC (New!)
		UpdateAnimations(velocity);

		Velocity = velocity;
		MoveAndSlide();
	}

	private void UpdateAnimations(Vector2 velocity)
	{
		// If attacking, let the attack animation play (optional, skipping for now)
		if (_isAttacking) return; 

		if (IsOnFloor())
		{
			if (Mathf.Abs(velocity.X) > 10)
				_animPlayer.Play("Run");
			else
				_animPlayer.Play("Idle");
		}
		else
		{
			if (velocity.Y < 0)
				_animPlayer.Play("Jump");
			else
				_animPlayer.Play("Fall");
		}
	}

	private async void PerformAttack()
	{
		_isAttacking = true;
		var shape = _hitboxArea.GetNode<CollisionShape2D>("CollisionShape2D");
		shape.Disabled = false;
		
		// Optional: Play an "Attack" animation here if you make one
		
		await ToSignal(GetTree().CreateTimer(AttackDuration), SceneTreeTimer.SignalName.Timeout);
		
		shape.Disabled = true;
		_isAttacking = false;
	}

	public void TakeDamage(int amount, Vector2 knockback)
	{
		_currentHealth -= amount;
		Velocity = knockback;
		
		Modulate = Colors.Red;
		GetTree().CreateTimer(0.1).Timeout += () => Modulate = Colors.White;

		if (_currentHealth <= 0) Die();
	}

	private async void Die()
	{
		// 1. Stop all movement and input
		// This prevents the player from running while dead
		SetPhysicsProcess(false); 
		Velocity = Vector2.Zero;

		// 2. Play the Death Animation
		GD.Print("Player Died");
		_animPlayer.Play("Death");

		// 3. Wait for the animation to finish (Length is 1.0s)
		await ToSignal(GetTree().CreateTimer(1.0), SceneTreeTimer.SignalName.Timeout);

		// 4. Restart the Level
		GetTree().ReloadCurrentScene();
	}
}

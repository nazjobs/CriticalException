using Godot;
using System;

public partial class PlayerController : CharacterBody2D
{
	[ExportCategory("Movement Stats")]
	[Export] public float Speed = 300.0f;
	[Export] public float JumpVelocity = -500.0f;
	[Export] public float DoubleJumpVelocity = -400.0f; // Usually slightly weaker than main jump
	[Export] public float Gravity = 1200.0f;
	[Export] public float Acceleration = 1500.0f;
	[Export] public float Friction = 1200.0f;

	[ExportCategory("Game Feel")]
	[Export] public float JumpCutValue = 0.5f;
	[Export] public double CoyoteTime = 0.1;
	[Export] public double JumpBufferTime = 0.1;
	[Export] public int MaxJumps = 2;
	
	[ExportCategory("Combat")]
	[Export] public float AttackDuration = 0.2f;
	private bool _isAttacking = false;
	private Area2D _hitboxArea;

	private double _coyoteTimer = 0;
	private double _jumpBufferTimer = 0;
	private int _jumpCount = 0;
	
	public override void _Ready()
	{
		// This looks for the node path: Player -> WeaponPivot -> MeleeHitbox
		_hitboxArea = GetNode<Area2D>("WeaponPivot/MeleeHitbox");
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// --- TIMERS ---
		if (_coyoteTimer > 0) _coyoteTimer -= delta;
		if (_jumpBufferTimer > 0) _jumpBufferTimer -= delta;

		// --- GRAVITY ---
		if (!IsOnFloor())
		{
			velocity.Y += Gravity * (float)delta;
		}
		else
		{
			_coyoteTimer = CoyoteTime;
			_jumpCount = 0;
		}

		// --- JUMP ---
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

		// --- MOVEMENT & WEAPON FACING ---
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		
		if (direction.X != 0)
		{
			velocity.X = Mathf.MoveToward(velocity.X, direction.X * Speed, Acceleration * (float)delta);
			
			// NEW CODE: Flip the WeaponPivot based on direction
			// If moving right (1), scale is 1. If moving left (-1), scale is -1.
			var pivot = GetNode<Node2D>("WeaponPivot");
			pivot.Scale = new Vector2(direction.X > 0 ? 1 : -1, 1);
		}
		else
		{
			velocity.X = Mathf.MoveToward(velocity.X, 0, Friction * (float)delta);
		}
		
		// --- ATTACK INPUT ---
		if (Input.IsActionJustPressed("attack") && !_isAttacking)
		{
			PerformAttack();
		}

		Velocity = velocity;
		MoveAndSlide();
	}
	private async void PerformAttack()
	{
		_isAttacking = true;
		
		// 1. Enable the Hitbox CollisionShape
		// This makes the "Sword" solid so it can hit the "Hurtbox"
		var shape = _hitboxArea.GetNode<CollisionShape2D>("CollisionShape2D");
		shape.Disabled = false;
		
		
		// 2. Wait for 0.2 seconds (AttackDuration)
		await ToSignal(GetTree().CreateTimer(AttackDuration), SceneTreeTimer.SignalName.Timeout);
		
		// 3. Disable the Hitbox again
		shape.Disabled = true;
		_isAttacking = false;
	}
}

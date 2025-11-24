using Godot;
using System;

public partial class PlayerController : CharacterBody2D
{
	[ExportCategory("Movement Stats")]
	[Export] public float Speed = 300.0f;
	[Export] public float JumpVelocity = -500.0f;
	[Export] public float Gravity = 1200.0f;
	[Export] public float Acceleration = 1500.0f;
	[Export] public float Friction = 1200.0f;

	[ExportCategory("Game Feel")]
	[Export] public float JumpCutValue = 0.5f; // How much velocity is cut when releasing space
	[Export] public double CoyoteTime = 0.1;   // Seconds you can jump after falling
	[Export] public double JumpBufferTime = 0.1; // Seconds your input is remembered

	private double _coyoteTimer = 0;
	private double _jumpBufferTimer = 0;

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// 1. UPDATE TIMERS
		// Subtract delta from timers if they are active
		if (_coyoteTimer > 0) _coyoteTimer -= delta;
		if (_jumpBufferTimer > 0) _jumpBufferTimer -= delta;

		// 2. GRAVITY & COYOTE TIME
		if (!IsOnFloor())
		{
			velocity.Y += Gravity * (float)delta;
		}
		else
		{
			// Reset Coyote timer while on the ground
			_coyoteTimer = CoyoteTime;
		}

		// 3. JUMP BUFFERING (Input Memory)
		if (Input.IsActionJustPressed("ui_accept"))
		{
			_jumpBufferTimer = JumpBufferTime;
		}

		// 4. PERFORM JUMP (The Magic Logic)
		// If we want to jump AND we are allowed to jump (Coyote Time)
		if (_jumpBufferTimer > 0 && _coyoteTimer > 0)
		{
			velocity.Y = JumpVelocity;
			_jumpBufferTimer = 0; // Consume the input
			_coyoteTimer = 0;     // Consume the ground state
		}

		// 5. VARIABLE JUMP HEIGHT (The "Mario" Effect)
		// If player releases button while moving up, cut the jump short
		if (Input.IsActionJustReleased("ui_accept") && velocity.Y < 0)
		{
			velocity.Y *= JumpCutValue;
		}

		// 6. HORIZONTAL MOVEMENT
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		if (direction.X != 0)
		{
			velocity.X = Mathf.MoveToward(velocity.X, direction.X * Speed, Acceleration * (float)delta);
		}
		else
		{
			velocity.X = Mathf.MoveToward(velocity.X, 0, Friction * (float)delta);
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}

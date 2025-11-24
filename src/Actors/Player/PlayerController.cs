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

	private double _coyoteTimer = 0;
	private double _jumpBufferTimer = 0;
	private int _jumpCount = 0;

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// 1. UPDATE TIMERS
		if (_coyoteTimer > 0) _coyoteTimer -= delta;
		if (_jumpBufferTimer > 0) _jumpBufferTimer -= delta;

		// 2. GRAVITY & FLOOR LOGIC
		if (!IsOnFloor())
		{
			velocity.Y += Gravity * (float)delta;
		}
		else
		{
			_coyoteTimer = CoyoteTime;
			_jumpCount = 0; // Reset jumps when touching ground
		}

		// 3. BUFFER INPUT
		if (Input.IsActionJustPressed("ui_accept"))
		{
			_jumpBufferTimer = JumpBufferTime;
		}

		// 4. JUMP LOGIC
		// Case A: Ground Jump (Normal)
		if (_jumpBufferTimer > 0 && _coyoteTimer > 0)
		{
			velocity.Y = JumpVelocity;
			_jumpBufferTimer = 0;
			_coyoteTimer = 0;
			_jumpCount = 1; // First jump used
		}
		// Case B: Air Jump (Double Jump)
		else if (Input.IsActionJustPressed("ui_accept") && _jumpCount < MaxJumps && _jumpCount > 0)
		{
			velocity.Y = DoubleJumpVelocity;
			_jumpCount++;
		}

		// 5. VARIABLE JUMP HEIGHT
		if (Input.IsActionJustReleased("ui_accept") && velocity.Y < 0)
		{
			velocity.Y *= JumpCutValue;
		}

		// 6. MOVEMENT
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

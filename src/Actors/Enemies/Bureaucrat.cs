using Godot;

public partial class Bureaucrat : CharacterBody2D
{
	[ExportCategory("Movement")]
	[Export] public float Speed = 60.0f;
	[Export] public float Gravity = 980.0f;
	
	[ExportCategory("Aggression")]
	[Export] public float PushForceHorizontal = 600.0f;
	[Export] public float PushForceVertical = -400.0f; // Negative is UP

	// Internal State
	private bool _isStunned = false;
	private int _direction = 1; // 1 = Right, -1 = Left

	// Nodes
	private CollisionShape2D _collider;
	private Sprite2D _sprite;
	private Area2D _repulsor;

	public override void _Ready()
	{
		_collider = GetNode<CollisionShape2D>("CollisionShape2D");
		_sprite = GetNode<Sprite2D>("Sprite2D");
		_repulsor = GetNode<Area2D>("Repulsor");

		// Connect the "Push" signal manually
		_repulsor.BodyEntered += OnRepulsorContact;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_isStunned) return; // Don't move if stunned

		Vector2 velocity = Velocity;

		// 1. Apply Gravity
		if (!IsOnFloor())
		{
			velocity.Y += Gravity * (float)delta;
		}

		// 2. Patrol Movement
		velocity.X = Speed * _direction;

		// 3. Apply and Move
		Velocity = velocity;
		MoveAndSlide();

		// 4. Wall Detection (Bounce)
		if (IsOnWall())
		{
			FlipDirection();
		}
		
		// Walk Animation
		
		if (!_isStunned && Mathf.Abs(velocity.X) > 0)
		{
			// Play walk if moving
			// (Assuming you named the AnimationPlayer "AnimationPlayer")
			GetNode<AnimationPlayer>("AnimationPlayer").Play("Walk");
		}
		else
		{
			// Stop or play Idle
			GetNode<AnimationPlayer>("AnimationPlayer").Stop();
		}
	}

	private void FlipDirection()
	{
		_direction *= -1;
		_sprite.FlipH = (_direction < 0);
	}

	// --- THE PUSH LOGIC ---
	private void OnRepulsorContact(Node2D body)
	{
		if (_isStunned) return;

		// CHANGE: Only push the PlayerController (Alex)
		// Ignoring generic IDamageable prevents kicking Drones
		if (body is PlayerController player)
		{
			GD.Print("Bureaucrat rejects YOU specifically!");

			float dirX = (body.GlobalPosition.X > GlobalPosition.X) ? 1 : -1;
			Vector2 kick = new Vector2(dirX * PushForceHorizontal, PushForceVertical);

			// Apply the kick to the player
			player.TakeDamage(0, kick);
		}
	}

	// --- STUN LOGIC (Existing) ---
	public void ApplyStun()
	{
		if (_isStunned) return;
		
		GD.Print("Bureaucrat confused by logic!");
		_isStunned = true;
		
		// 1. Visuals
		_sprite.Modulate = new Color(0.5f, 0.5f, 1f, 0.5f); // Ghost Blue
		
		// 2. Disable Physics
		// Disable the hard wall
		_collider.SetDeferred("disabled", true); 
		// Disable the "Push" zone too so you don't get kicked while walking through
		_repulsor.SetDeferred("monitoring", false);

		// 3. Timer
		GetTree().CreateTimer(3.0f).Timeout += Recover;
	}

	private void Recover()
	{
		_isStunned = false;
		_sprite.Modulate = Colors.White;
		_collider.SetDeferred("disabled", false);
		_repulsor.SetDeferred("monitoring", true);
	}
}

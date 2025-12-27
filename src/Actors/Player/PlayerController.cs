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
	
	//	--- Shooting Variables ---
	[Export] public PackedScene ProjectileScene; // Drag the bullet here
	[Export] public float FireRate = 0.5f;
	private bool _canShoot = true;
	private Marker2D _muzzle;

	//	--- Area Effect Ability ---
	[ExportCategory("Abilities")]
	[Export] public float ShoutCooldown = 3.0f;
	private bool _canShout = true;
	private Area2D _paradoxArea;
	private Sprite2D _paradoxSprite;
	
	// --- AUDIO VARIABLES ---
	private AudioStreamPlayer2D _sfxJump;   
	private AudioStreamPlayer2D _sfxShoot;  
	private AudioStreamPlayer2D _sfxHit;    
	private AudioStreamPlayer2D _sfxDie;
	private AudioStreamPlayer2D _sfxMelee;
	private AudioStreamPlayer2D _sfxBoom;
	
	
	[Export] public PackedScene GameOverScene;
	
	public event Action<int> OnHealthChanged; 


	public override void _Ready()
	{
		
		// We look inside the "AudioManager" folder we created in the scene
		_sfxJump = GetNode<AudioStreamPlayer2D>("AudioManager/SFX_Jump");
		_sfxShoot = GetNode<AudioStreamPlayer2D>("AudioManager/SFX_Shoot");
		_sfxHit = GetNode<AudioStreamPlayer2D>("AudioManager/SFX_Hit");
		_sfxDie = GetNode<AudioStreamPlayer2D>("AudioManager/SFX_Die");
		_sfxMelee = GetNode<AudioStreamPlayer2D>("AudioManager/SFX_Melee");
		_sfxBoom = GetNode<AudioStreamPlayer2D>("AudioManager/SFX_Boom");
		
		
		_paradoxArea = GetNode<Area2D>("ParadoxArea");
		_paradoxSprite = _paradoxArea.GetNode<Sprite2D>("Sprite2D");
		
		_muzzle = GetNode<Marker2D>("WeaponPivot/Muzzle"); // Marker2D
		
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
			_sfxJump.Play(); // <--- PLAY JUMP SOUND
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
		
		if (Input.IsActionJustPressed("shout") && _canShout)
		{
			PerformShout();
		}
		
		// 
		if (Input.IsActionPressed("fire") && _canShoot)
		{
			Shoot();
		}
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
		
		_sfxMelee.Play();
		
		// 1. Play the Animation (Visuals + Physics if you keyed the collision)
		_animPlayer.Play("Attack");

		// 2. Wait for it to finish
		// We use the length of the animation to determine how long to wait
		await ToSignal(GetTree().CreateTimer(0.3), SceneTreeTimer.SignalName.Timeout);
		
		// 3. Reset
		_isAttacking = false;
		// If you didn't keyframe the collision shape in the animation, disable it here manually:
		// _hitboxArea.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true; 
		
		await ToSignal(GetTree().CreateTimer(AttackDuration), SceneTreeTimer.SignalName.Timeout);
		
		shape.Disabled = true;
		_isAttacking = false;
	}
	
	private void Shoot()
	{
		if (ProjectileScene == null) return;

		_canShoot = false;
		
		// 1. Create Bullet
		var bullet = ProjectileScene.Instantiate<Projectile>();
		
		// 2. Position it at the Muzzle
		bullet.GlobalPosition = _muzzle.GlobalPosition;
		
		_sfxShoot.Play();
		
		// 3. Set Direction based on where character is facing
		// If Sprite is flipped (facing left), shoot left
		Vector2 shootDir = _sprite.FlipH ? Vector2.Left : Vector2.Right;
		bullet.Direction = shootDir;
		
		// 4. Add to the World (Not as a child of Player, otherwise it moves with him!)
		GetTree().Root.AddChild(bullet);

		// 5. Cooldown Timer
		GetTree().CreateTimer(FireRate).Timeout += () => _canShoot = true;
	}
		private async void PerformShout()
	{
		_canShout = false;
		GD.Print("OBJECTION!");
		
		// 1. Enable Physics
		_paradoxArea.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
		_paradoxSprite.Visible = true;
		
		// 2. Play the Pulse Animation
		// We get the AnimationPlayer strictly inside the ParadoxArea
		_paradoxArea.GetNode<AnimationPlayer>("AnimationPlayer").Play("Pulse");

		// 3. Wait for physics to catch up (The fix we did earlier)
		await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
		
		// 4. Scan logic (Existing code...)
		var bodies = _paradoxArea.GetOverlappingBodies();
		foreach (var body in bodies)
		{
			if (body is Bureaucrat b) b.ApplyStun();
		}

		// 5. Wait for animation to finish (0.5s)
		await ToSignal(GetTree().CreateTimer(0.5), SceneTreeTimer.SignalName.Timeout);
		
		// 6. Disable Physics & Reset Visuals
		_paradoxArea.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
		_paradoxSprite.Visible = false; // Hide it again
		
		// Reset scale/alpha for next time (optional if animation resets it at start)
		_paradoxSprite.Scale = new Vector2(1, 1); 
		_paradoxSprite.Modulate = new Color(1, 1, 1, 1);

		// 7. Cooldown
		await ToSignal(GetTree().CreateTimer(ShoutCooldown), SceneTreeTimer.SignalName.Timeout);
		_canShout = true;
	}

	public void TakeDamage(int amount, Vector2 knockback)
	{
		_sfxHit.Play();
		_currentHealth -= amount;
		
		OnHealthChanged?.Invoke(_currentHealth);

		Velocity = knockback;
		
		Modulate = Colors.Red;
		GetTree().CreateTimer(0.1).Timeout += () => Modulate = Colors.White;

		if (_currentHealth <= 0) Die();
	}

	private async void Die()
	{
		_sfxDie.Play();
		
		// 1. Stop Physics/Input
		SetPhysicsProcess(false);
		Velocity = Vector2.Zero;

		// 2. Play Animation
		GD.Print("Player Died");
		_animPlayer.Play("Death");

		// 3. Wait for animation to finish
		await ToSignal(GetTree().CreateTimer(1.0), SceneTreeTimer.SignalName.Timeout);

		// 4. SHOW GAME OVER SCREEN (New Logic)
		if (GameOverScene != null)
		{
			// Instantiate the UI and add it to the Scene Root (so it covers everything)
			var gameOver = GameOverScene.Instantiate();
			GetTree().Root.AddChild(gameOver);
			
			// Optional: Pause the game behind the UI
			// GetTree().Paused = true; 
			// (Note: If you pause, make sure the GameOver buttons have Process Mode: Always)
		}
		else
		{
			// Fallback if you forgot to assign the scene
			GD.Print("Error: GameOverScene not assigned in Player!");
			GetTree().ReloadCurrentScene();
		}
	}
}

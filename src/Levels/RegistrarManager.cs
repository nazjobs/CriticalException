using Godot;
using System;

public partial class RegistrarManager : Node2D
{
	[Export] public float TimeLimit = 60.0f;
	[Export] public Label TimerLabel; 
	private double _timeLeft;
	private bool _isActive = true;
	private PlayerController _player;

	public override void _Ready()
	{
		_timeLeft = TimeLimit;
		_player = GetTree().GetFirstNodeInGroup("Player") as PlayerController;
		GD.Print("Welcome to the Registrar. Take a number.");
	}

	public override void _Process(double delta)
	{
		if (!_isActive) return;

		_timeLeft -= delta;
		
		// --- UPDATE TIMER ---
		if (TimerLabel != null)
		{
			// Format: "TIME: 59" (No decimals needed for bureaucracy)
			TimerLabel.Text = $"TIME: {Mathf.CeilToInt(_timeLeft)}";
			
			// Optional: Make it red when low
			if (_timeLeft < 10) TimerLabel.Modulate = Colors.Red;
			else TimerLabel.Modulate = Colors.White;
		}
		// -----------------

		// Simple Debug UI in Console
		// (We will make a real UI bar in the next phase)
		if (Mathf.Round(_timeLeft) % 5 == 0) 
		{
			// Print every 5 seconds roughly
			// GD.Print($"Time remaining: {_timeLeft:F1}"); 
		}

		if (_timeLeft <= 0)
		{
			TriggerExhaustion();
		}
	}

	private void TriggerExhaustion()
	{
		_isActive = false;
		GD.Print("The window is closed. Come back tomorrow.");
		
		if (_player != null)
		{
			// Kill the player (triggering the death animation)
			// You might want to pass a specific reason if you expand the system later
			_player.TakeDamage(999, Vector2.Zero); 
		}
	}
}

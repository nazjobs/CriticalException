using Godot;

public partial class HUD : CanvasLayer
{
	private ProgressBar _bar;

	public override void _Ready()
	{
		_bar = GetNode<ProgressBar>("GPABar");
		
		// We need to find the player dynamically since the HUD might load before/after him
		var player = GetTree().GetFirstNodeInGroup("Player") as PlayerController;
		if (player != null)
		{
			player.OnHealthChanged += UpdateGPA;
		}
	}

	private void UpdateGPA(int newHealth)
	{
		_bar.Value = newHealth;
	}
}

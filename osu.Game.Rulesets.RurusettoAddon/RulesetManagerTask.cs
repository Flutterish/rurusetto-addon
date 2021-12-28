namespace osu.Game.Rulesets.RurusettoAddon {
	public record RulesetManagerTask ( TaskType Type, string Source ) { }

	public enum TaskType {
		Install,
		Update,
		Remove
	}
}

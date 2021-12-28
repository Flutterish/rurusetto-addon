namespace osu.Game.Rulesets.RurusettoAddon {
	public record RulesetManagerTask ( TaskType Type, string Source ) {
		public string Ruleset { get; init; }
	}

	public enum TaskType {
		Install,
		Update,
		Remove
	}
}

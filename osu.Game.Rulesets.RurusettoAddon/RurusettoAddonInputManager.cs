using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.RurusettoAddon {
	public class RurusettoAddonInputManager : RulesetInputManager<RurusettoAddonAction> {
        public RurusettoAddonInputManager(RulesetInfo ruleset) : base(ruleset, 0, SimultaneousBindingMode.Unique) { }
    }

    public enum RurusettoAddonAction { }
}

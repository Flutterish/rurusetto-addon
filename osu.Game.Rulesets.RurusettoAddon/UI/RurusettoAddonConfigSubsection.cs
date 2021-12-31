using osu.Framework.Localisation;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.RurusettoAddon.Configuration;

namespace osu.Game.Rulesets.RurusettoAddon.UI {
	public class RurusettoAddonConfigSubsection : RulesetSettingsSubsection {
		public RurusettoAddonConfigSubsection ( Ruleset ruleset ) : base( ruleset ) { }

		protected override LocalisableString Header => "Rurusetto Addon";

		protected override void LoadComplete () {
			base.LoadComplete();
			var config = Config as RurusettoConfigManager;

			Add( new SettingsTextBox { LabelText = "API Address", Current = config.GetBindable<string>( RurusettoSetting.APIAddress ) } );
		}
	}
}

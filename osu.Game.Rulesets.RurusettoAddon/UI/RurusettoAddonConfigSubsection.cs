using osu.Framework.Localisation;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.RurusettoAddon.Configuration;

namespace osu.Game.Rulesets.RurusettoAddon.UI;

public class RurusettoAddonConfigSubsection : RulesetSettingsSubsection {
	public RurusettoAddonConfigSubsection ( Ruleset ruleset ) : base( ruleset ) { }

	protected override LocalisableString Header => Localisation.Strings.SettingsHeader;

	protected override void LoadComplete () {
		base.LoadComplete();
		var config = (RurusettoConfigManager)Config;

		Add( new SettingsTextBox { LabelText = Localisation.Strings.SettingsApiAddress, Current = config.GetBindable<string>( RurusettoSetting.APIAddress ) } );
	}
}
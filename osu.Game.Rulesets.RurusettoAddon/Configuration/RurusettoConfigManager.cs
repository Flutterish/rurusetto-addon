﻿using osu.Game.Configuration;
using osu.Game.Rulesets.Configuration;

namespace osu.Game.Rulesets.RurusettoAddon.Configuration;

public class RurusettoConfigManager : RulesetConfigManager<RurusettoSetting> {
	public RurusettoConfigManager ( SettingsStore store, RulesetInfo ruleset, int? variant = null ) : base( store, ruleset, variant ) {
		AddBindable( RurusettoSetting.APIAddress, new Bindable<string>( RurusettoAPI.DefaultAPIAddress ) );
	}
}

public enum RurusettoSetting {
	APIAddress
}
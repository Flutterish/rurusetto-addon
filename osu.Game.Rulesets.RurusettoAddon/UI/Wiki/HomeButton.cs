﻿using osu.Game.Graphics.UserInterface;

namespace osu.Game.Rulesets.RurusettoAddon.UI.Wiki;

public class HomeButton : GrayButton {
	RulesetDetail entry;
	public HomeButton ( RulesetDetail entry ) : base( FontAwesome.Solid.Home ) {
		this.entry = entry;
		TooltipText = Localisation.Strings.HomePage;
	}

	[BackgroundDependencyLoader( permitNulls: true )]
	private void load ( OsuGame game, OsuColour colours ) {
		Background.Colour = colours.Blue3;
		Icon.Colour = Colour4.White;
		Icon.Scale = new osuTK.Vector2( 1.5f );

		if ( !string.IsNullOrWhiteSpace( entry.Source ) ) {
			Action = () => {
				game?.OpenUrlExternally( entry.Source );
			};
		}
	}
}
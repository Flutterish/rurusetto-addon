using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.RurusettoAddon.API;

namespace osu.Game.Rulesets.RurusettoAddon.UI.Info {
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
}

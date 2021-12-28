using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.RurusettoAddon.API;

namespace osu.Game.Rulesets.RurusettoAddon.UI {
	public class HomeButton : GrayButton {
		RulesetDetail entry;
		public HomeButton ( RulesetDetail entry ) : base( FontAwesome.Solid.Home ) {
			this.entry = entry;
			TooltipText = "Home Page";
		}

		[BackgroundDependencyLoader(permitNulls: true)]
		private void load ( OsuGame game ) {
			Background.Colour = Colour4.FromHex( "#6291D7" );
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

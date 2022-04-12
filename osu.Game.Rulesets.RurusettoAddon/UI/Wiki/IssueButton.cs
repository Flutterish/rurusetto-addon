using osu.Game.Graphics.UserInterface;

namespace osu.Game.Rulesets.RurusettoAddon.UI.Wiki;

public class IssueButton : GrayButton {
	RulesetDetail entry;
	public IssueButton ( RulesetDetail entry ) : base( FontAwesome.Solid.Exclamation ) {
		this.entry = entry;
		TooltipText = Localisation.Strings.ReportIssue;
	}

	[BackgroundDependencyLoader( permitNulls: true )]
	private void load ( OsuGame game ) {
		Background.Colour = Colour4.FromHex( "#FF6060" );
		Icon.Colour = Colour4.White;
		Icon.Scale = new osuTK.Vector2( 1.2f );

		if ( !string.IsNullOrWhiteSpace( entry.Source ) && entry.Source.StartsWith( "https://github.com/" ) ) {
			Action = () => {
				game?.OpenUrlExternally( entry.Source.TrimEnd( '/' ) + "/issues/new" );
			};
		}
	}
}
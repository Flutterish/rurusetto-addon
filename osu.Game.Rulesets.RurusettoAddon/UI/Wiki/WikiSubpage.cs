using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.RurusettoAddon.API;

namespace osu.Game.Rulesets.RurusettoAddon.UI.Wiki;

public class WikiSubpage : WikiPage {
	string slug;
	public WikiSubpage ( APIRuleset ruleset, string slug ) : base( ruleset ) {
		this.slug = slug;
	}

	public override bool Refresh () {
		ClearInternal();

		Ruleset.FlushSubpage( slug );
		var content = new FillFlowContainer {
			RelativeSizeAxes = Axes.X,
			AutoSizeAxes = Axes.Y,
			Direction = FillDirection.Vertical
		};
		AddInternal( content );

		Overlay.StartLoading( Tab );
		Ruleset.RequestSubpage( slug, subpage => {
			var markdown = new MarkdownPage( Ruleset ) { Text = subpage.Content ?? "" };
			content.Child = markdown;
			Overlay.FinishLoadiong( Tab );

		}, failure: e => {
			content.Add( new Container {
				Padding = new MarginPadding { Horizontal = -32 },
				AutoSizeAxes = Axes.Y,
				RelativeSizeAxes = Axes.X,
				Child = new RequestFailedDrawable {
					ContentText = Localisation.Strings.PageFetchError,
					ButtonClicked = () => Refresh()
				}
			} );

			Overlay.FinishLoadiong( Tab );
		} );

		return true;
	}
}

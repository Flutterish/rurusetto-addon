using osu.Game.Graphics.Containers.Markdown;

namespace osu.Game.Rulesets.RurusettoAddon.UI.Wiki;

public class MarkdownPage : WikiPage {
	ContentMarkdown? content;
	public MarkdownPage ( APIRuleset ruleset ) : base( ruleset ) { }

	string text = "";
	public string Text {
		get => text;
		set {
			text = value;
			if ( content != null )
				content.Text = value;
		}
	}

	public override bool Refresh () {
		ClearInternal();

		var address = API.GetEndpoint( Ruleset.Slug is null ? "/rulesets" : $"/rulesets/{Ruleset.Slug}" ).AbsoluteUri;
		AddInternal( content = new ContentMarkdown( address ) {
			RelativeSizeAxes = Axes.X,
			AutoSizeAxes = Axes.Y,
			Text = text
		} );

		return true;
	}

	private class ContentMarkdown : OsuMarkdownContainer {
		public ContentMarkdown ( string address ) {
			DocumentUrl = address;
			var uri = new Uri( address );
			RootUrl = $"{uri.Scheme}://{uri.Host}";
		}
	}
}

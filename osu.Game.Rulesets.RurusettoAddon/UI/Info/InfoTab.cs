using Humanizer;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers.Markdown;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.RurusettoAddon.API;
using System;

namespace osu.Game.Rulesets.RurusettoAddon.UI.Info {
	public class InfoTab : OverlayTab {
		FillFlowContainer content;
		ListingEntry entry;
		public InfoTab ( ListingEntry entry ) {
			this.entry = entry;

			AddInternal( content = new FillFlowContainer {
				Direction = FillDirection.Full,
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Padding = new MarginPadding { Horizontal = 16, Top = 8 }
			} );
		}

		protected override bool RequiresLoading => true;
		protected override void LoadContent () {
			API.RequestRulesetDetail( entry.ShortName ).ContinueWith( t => {
				Schedule( () => {
					var entry = t.Result;

					content.Add( new OsuSpriteText {
						Text = entry.Name.Humanize(),
						Font = OsuFont.GetFont( size: 34, weight: FontWeight.Black ),
						Margin = new MarginPadding { Bottom = 20 }
					} );

					content.Add( new ContentMarkdown( API.GetEndpoint( $"/rulesets/{entry.ShortName}" ).AbsoluteUri ) {
						RelativeSizeAxes = Axes.X,
						AutoSizeAxes = Axes.Y,
						Text = entry.Content,
						Margin = new MarginPadding { Left = 6 }
					} );

					OnContentLoaded();
				} );
			} );
		}

		private class ContentMarkdown : OsuMarkdownContainer {
			public ContentMarkdown ( string address ) {
				DocumentUrl = address;
				var uri = new Uri( address );
				RootUrl = $"{uri.Scheme}://{uri.Host}";
			}
		}
	}
}

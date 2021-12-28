using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Containers.Markdown;
using osu.Game.Overlays;
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
				Padding = new MarginPadding { Horizontal = 32, Top = 8 }
			} );
		}

		OverlayColourProvider colourProvider;
		[BackgroundDependencyLoader]
		private void load ( OverlayColourProvider colourProvider ) {
			this.colourProvider = colourProvider;
		}

		protected override bool RequiresLoading => true;
		protected override void LoadContent () {
			API.RequestRulesetDetail( entry.ShortName ).ContinueWith( t => {
				API.RequestImage( t.Result.CoverDark ).ContinueWith( t => Schedule( () => {
					AddInternal( new Container {
						RelativeSizeAxes = Axes.X,
						Height = 220,
						Children = new Drawable[] {
							new Sprite {
								RelativeSizeAxes = Axes.Both,
								Texture = t.Result,
								FillMode = FillMode.Fill,
								Origin = Anchor.Centre,
								Anchor = Anchor.Centre
							},
							new Box {
								Colour = new ColourInfo {
									HasSingleColour = false,
									BottomLeft = colourProvider.Background5,
									BottomRight = colourProvider.Background5,
									TopLeft = colourProvider.Background5.Opacity( 0.5f ),
									TopRight = colourProvider.Background5.Opacity( 0.5f )
								},
								RelativeSizeAxes = Axes.Both,
								Anchor = Anchor.BottomCentre,
								Origin = Anchor.BottomCentre
							}
						},
						Masking = true,
						Depth = 1,
						MaskingSmoothness = 0
					} );
				} ) );
				
				Schedule( () => {
					var entry = t.Result;

					content.Add( new Container {
						Name = "Padding",
						RelativeSizeAxes = Axes.X,
						Height = 160
					} );

					content.Add( new Container {
						RelativeSizeAxes = Axes.X,
						AutoSizeAxes = Axes.Y,
						Children = new Drawable[] {
							new FillFlowContainer {
								Direction = FillDirection.Horizontal,
								RelativeSizeAxes = Axes.X,
								Height = 170f * 14 / 20,
								Children = new Drawable[] {
									new RulesetLogo( this.entry ) {
										Height = 170f * 14 / 20,
										Width = 170f * 14 / 20,
										Margin = new MarginPadding { Right = 16 }
									},
									new DrawableRurusettoUser( this.entry.Owner, this.entry.IsVerified ) {
										Anchor = Anchor.BottomLeft,
										Origin = Anchor.BottomLeft,
										Height = 64f * 14 / 20,
										Margin = new MarginPadding { Bottom = 4 }
									}
								},
								Margin = new MarginPadding { Bottom = 20 }
							},
							new RulesetDownloadButton( this.entry ) {
								Height = 40f * 14 / 20,
								Width = 200f * 14 / 20,
								Anchor = Anchor.BottomRight,
								Origin = Anchor.BottomRight,
								Y = -28
							}
						}
					} );

					content.Add( new ContentMarkdown( API.GetEndpoint( $"/rulesets/{entry.ShortName}" ).AbsoluteUri ) {
						RelativeSizeAxes = Axes.X,
						AutoSizeAxes = Axes.Y,
						Text = entry.Content,
						Margin = new MarginPadding { Left = 6, Bottom = 400 }
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

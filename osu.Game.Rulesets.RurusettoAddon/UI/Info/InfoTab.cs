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
using osuTK;
using System;

namespace osu.Game.Rulesets.RurusettoAddon.UI.Info {
	public class InfoTab : OverlayTab {
		FillFlowContainer content;
		ListingEntry entry;
		Sprite cover;
		ContentMarkdown markdown;
		protected FillFlowContainer Tags;
		FillFlowContainer buttons;
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
			AddInternal( new Container {
				RelativeSizeAxes = Axes.X,
				Height = 220,
				Children = new Drawable[] {
					cover = new Sprite {
						RelativeSizeAxes = Axes.Both,
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

			content.Add( new Container {
				Name = "Padding",
				RelativeSizeAxes = Axes.X,
				Height = 160,
				Child = Tags = new FillFlowContainer {
					Direction = FillDirection.Horizontal,
					AutoSizeAxes = Axes.Both,
					Spacing = new Vector2( 6, 0 ),
					Margin = new MarginPadding { Top = 16 }
				},
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
							new RulesetLogo( entry ) {
								Height = 170f * 14 / 20,
								Width = 170f * 14 / 20,
								Margin = new MarginPadding { Right = 16 }
							},
							new DrawableRurusettoUser( entry.Owner, entry.IsVerified ) {
								Anchor = Anchor.BottomLeft,
								Origin = Anchor.BottomLeft,
								Height = 64f * 14 / 20,
								Margin = new MarginPadding { Bottom = 4 }
							}
						},
						Margin = new MarginPadding { Bottom = 20 }
					},
					buttons = new FillFlowContainer {
						Y = -28,
						Spacing = new Vector2( 8, 0 ),
						AutoSizeAxes = Axes.Both,
						Origin = Anchor.BottomRight,
						Anchor = Anchor.BottomRight,
						Children = new Drawable[] {
							new RulesetDownloadButton( entry ) {
								Height = 40f * 14 / 20,
								Width = 200f * 14 / 20,
								Anchor = Anchor.CentreRight,
								Origin = Anchor.CentreRight
							}
						}
					}
				}
			} );

			content.Add( markdown = new ContentMarkdown( API.GetEndpoint( $"/rulesets/{entry.Slug}" ).AbsoluteUri ) {
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Margin = new MarginPadding { Left = 6, Bottom = 400 }
			} );

			API.RequestRulesetDetail( entry.Slug ).ContinueWith( t => {
				API.RequestImage( t.Result.CoverDark ).ContinueWith( t => Schedule( () => {
					cover.Texture = t.Result;
				} ) );

				Schedule( () => {
					var entry = t.Result;
					markdown.Text = entry.Content;
					if ( entry.IsArchived ) {
						Tags.Add( DrawableTag.CreateArchived( large: true ) );
					}
					if ( this.entry.IsLocal ) {
						Tags.Add( DrawableTag.CreateLocal( large: true ) );
						if ( this.entry.LocalRulesetInfo != null && DownloadManager.IsHardCodedRuleset( this.entry.LocalRulesetInfo ) ) {
							Tags.Add( DrawableTag.CreateHardCoded( large: true ) );
						}
					}
					if ( this.entry.FaliedImport ) {
						Tags.Add( DrawableTag.CreateFailledImport( large: true ) );
					}

					buttons.Add( new HomeButton( entry ) {
						Height = 40f * 14 / 20,
						Width = 120f * 14 / 20,
						Anchor = Anchor.CentreRight,
						Origin = Anchor.CentreRight
					} );
					buttons.Add( new IssueButton( entry ) {
						Height = 40f * 14 / 20,
						Width = 40f * 14 / 20,
						Anchor = Anchor.CentreRight,
						Origin = Anchor.CentreRight
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

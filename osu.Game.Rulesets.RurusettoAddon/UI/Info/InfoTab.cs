using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers.Markdown;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Rulesets.RurusettoAddon.API;
using osu.Game.Rulesets.RurusettoAddon.UI.Users;
using osuTK;
using System;
using System.Collections.Generic;

namespace osu.Game.Rulesets.RurusettoAddon.UI.Info {
	public class InfoTab : OverlayTab {
		FillFlowContainer content;
		APIRuleset ruleset;
		Sprite cover;
		Container subpageContent;
		ContentMarkdown mainPageMarkdown;
		protected FillFlowContainer Tags;
		protected FillFlowContainer Status;
		SubpageSectionTabControl subpageTabControl;
		FillFlowContainer buttons;
		public InfoTab ( APIRuleset ruleset ) {
			this.ruleset = ruleset;

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

		SubpageListingEntry main;
		SubpageListingEntry changelog;
		Dictionary<SubpageListingEntry, Drawable> subpageDrawables = new();

		[Resolved]
		private LocalisationManager localisation { get; set; }

		ILocalisedBindableString contentBindable;

		protected override bool RequiresLoading => true;
		protected override void LoadContent () {
			int loadsLeft = 2;

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
				Children = new Drawable[] {
					Tags = new FillFlowContainer {
						Direction = FillDirection.Horizontal,
						AutoSizeAxes = Axes.Both,
						Spacing = new Vector2( 6, 0 ),
						Margin = new MarginPadding { Top = 16 }
					},
					Status = new FillFlowContainer {
						Anchor = Anchor.TopRight,
						Origin = Anchor.TopRight,
						Direction = FillDirection.Vertical,
						AutoSizeAxes = Axes.Both,
						Spacing = new Vector2( 0, 6 ),
						Margin = new MarginPadding { Top = 16 }
					}
				}
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
							new RulesetLogo( ruleset ) {
								Height = 170f * 14 / 20,
								Width = 170f * 14 / 20,
								Margin = new MarginPadding { Right = 16 },
								UseDarkerBackground = true
							},
							new DrawableRurusettoUser( Users.GetUserIdentity( ruleset.Owner ), ruleset.IsVerified ) {
								Anchor = Anchor.BottomLeft,
								Origin = Anchor.BottomLeft,
								Height = 64f * 14 / 20,
								Margin = new MarginPadding { Bottom = 4 },
								UseDarkerBackground = true
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
							new RulesetDownloadButton( ruleset ) {
								Height = 40f * 14 / 20,
								Width = 200f * 14 / 20,
								Anchor = Anchor.CentreRight,
								Origin = Anchor.CentreRight,
								UseDarkerBackground = true
							}
						}
					}
				}
			} );

			content.Add( new Container {
				Height = 28,
				RelativeSizeAxes = Axes.X,
				Child = new Container( ) {
					RelativeSizeAxes = Axes.Both,
					Padding = new MarginPadding { Horizontal = 6 },
					Y = -6,
					Child = subpageTabControl = new() {
						RelativeSizeAxes = Axes.X
					}
				},
				Margin = new MarginPadding { Bottom = 12 }
			} );

			content.Add( subpageContent = new Container {
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Margin = new MarginPadding { Top = 16, Bottom = 400 }
			} );

			subpageTabControl.Current.BindValueChanged( v => {
				subpageContent.Clear( disposeChildren: false );

				if ( !subpageDrawables.TryGetValue( v.NewValue, out var subpage ) ) {
					var content = createMarkdownContainer();

					Overlay.StartLoading( this );
					ruleset.RequestSubpage( v.NewValue.Slug, subpage => {
						content.Text = subpage.Content ?? "";
						Overlay.FinishLoadiong( this );
					}, failure: () => { /* TODO report this */ } );

					subpage = content;
					subpageDrawables.Add( v.NewValue, content );
				}

				subpageContent.Add( subpage );
			} );

			subpageDrawables.Add( main = new() { Title = Localisation.Strings.MainPage }, mainPageMarkdown = createMarkdownContainer() );

			ruleset.RequestSubpages( subpages => {
				subpageTabControl.AddItem( main );

				if ( ruleset.ListingEntry?.Status?.Changelog is string log && !string.IsNullOrWhiteSpace( log ) ) {
					subpageTabControl.AddItem( changelog = new() { Title = Localisation.Strings.ChangelogPage } );
					subpageDrawables.Add( changelog, createMarkdownContainer().With( d => d.Text = log ) );
				}

				foreach ( var i in subpages ) {
					subpageTabControl.AddItem( i );
				}

				subpageTabControl.Current.Value = main;

				if ( --loadsLeft <= 0 )
					OnContentLoaded();

			}, failure: () => {
				// TODO report this 

				if ( --loadsLeft <= 0 )
					OnContentLoaded();
			} );

			bool isCoverLoaded = false;
			API.RequestImage( StaticAPIResource.DefaultCover, texture => {
				// TODO load a default local cover defore the default web cover too
				if ( !isCoverLoaded ) {
					cover.Texture = texture;
				}
			}, failure: () => { /* TODO report this */ } );

			ruleset.RequestDarkCover( texture => {
				isCoverLoaded = true;
				cover.Texture = texture;
			}, failure: () => { /* TODO report this */ } );

			ruleset.RequestDetail( detail => {
				contentBindable = localisation.GetLocalisedBindableString( detail.Content );
				
				contentBindable.BindValueChanged( v => {
					mainPageMarkdown.Text = v.NewValue;
				}, true );

				Tags.AddRange( ruleset.GenerateTags( detail, large: true, includePlayability: false ) );
				if ( ruleset.ListingEntry?.Status?.IsPlayable == true ) {
					Status.Add( DrawableTag.CreatePlayable( large: true ).With( d => {
						d.Anchor = Anchor.TopRight;
						d.Origin = Anchor.TopRight;
					} ) );
				}
				else if ( ruleset.ListingEntry?.Status?.IsBorked == true ) {
					Status.Add( DrawableTag.CreateBorked( large: true ).With( d => {
						d.Anchor = Anchor.TopRight;
						d.Origin = Anchor.TopRight;
					} ) );
				}
				Status.Add( new OsuSpriteText {
					Anchor = Anchor.TopRight,
					Origin = Anchor.TopRight,
					Font = OsuFont.GetFont( Typeface.Inter, size: 14 ),
					Text = string.IsNullOrWhiteSpace( ruleset.ListingEntry?.Status?.LatestVersion ) ? Localisation.Strings.UnknownVersion : ruleset.ListingEntry.Status.LatestVersion
				} );

				buttons.Add( new HomeButton( detail ) {
					Height = 40f * 14 / 20,
					Width = 120f * 14 / 20,
					Anchor = Anchor.CentreRight,
					Origin = Anchor.CentreRight
				} );
				buttons.Add( new IssueButton( detail ) {
					Height = 40f * 14 / 20,
					Width = 40f * 14 / 20,
					Anchor = Anchor.CentreRight,
					Origin = Anchor.CentreRight
				} );

				if ( --loadsLeft <= 0 )
					OnContentLoaded();

			}, failure: () => {
				// TODO report this

				if ( --loadsLeft <= 0 )
					OnContentLoaded();
			} );
		}

		private ContentMarkdown createMarkdownContainer () {
			return new ContentMarkdown( ruleset.Slug is null ? API.GetEndpoint( "/rulesets" ).AbsoluteUri : API.GetEndpoint( $"/rulesets/{ruleset.Slug}" ).AbsoluteUri ) {
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y
			};
		}

		private class ContentMarkdown : OsuMarkdownContainer {
			public ContentMarkdown ( string address ) {
				DocumentUrl = address;
				var uri = new Uri( address );
				RootUrl = $"{uri.Scheme}://{uri.Host}";
			}
		}

		// https://github.com/ppy/osu/blob/2fd4647e6eeb7c08861d8e526af97aff5c0e39f6/osu.Game/Overlays/UserProfileOverlay.cs#L153
		private class SubpageSectionTabControl : OverlayTabControl<SubpageListingEntry> {
			private const float bar_height = 2;

			public SubpageSectionTabControl () {
				TabContainer.RelativeSizeAxes &= ~Axes.X;
				TabContainer.AutoSizeAxes |= Axes.X;
				TabContainer.Anchor |= Anchor.x1;
				TabContainer.Origin |= Anchor.x1;

				Height = 36 + bar_height;
				BarHeight = bar_height;
			}

			protected override TabItem<SubpageListingEntry> CreateTabItem ( SubpageListingEntry value ) => new SubpageSectionTabItem( value ) {
				AccentColour = AccentColour,
			};

			[BackgroundDependencyLoader]
			private void load ( OverlayColourProvider colourProvider ) {
				AccentColour = colourProvider.Highlight1;
			}

			protected override bool OnClick ( ClickEvent e ) => true;

			protected override bool OnHover ( HoverEvent e ) => true;

			private class SubpageSectionTabItem : OverlayTabItem {
				public SubpageSectionTabItem ( SubpageListingEntry value ) : base( value ) {
					Text.Text = value.Title;
					Text.Font = Text.Font.With( size: 16 );
					Text.Margin = new MarginPadding { Bottom = 10 + bar_height };
					Bar.ExpandedSize = 10;
					Bar.Margin = new MarginPadding { Bottom = bar_height };
				}
			}
		}
	}
}

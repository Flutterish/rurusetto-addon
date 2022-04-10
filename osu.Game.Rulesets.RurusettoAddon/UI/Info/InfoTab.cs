﻿using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Containers.Markdown;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Rulesets.RurusettoAddon.API;
using osu.Game.Rulesets.RurusettoAddon.UI.Users;
using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.RurusettoAddon.UI.Info {
	public class InfoTab : OverlayTab {
		FillFlowContainer content;
		APIRuleset ruleset;
		Sprite cover;
		FillFlowContainer info;
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
		Texture defaultCover;
		[BackgroundDependencyLoader]
		private void load ( OverlayColourProvider colourProvider, GameHost host, TextureStore textures, RurusettoAddonRuleset ruleset ) {
			this.colourProvider = colourProvider;
			defaultCover = ruleset.GetTexture( host, textures, TextureNames.DefaultCover );
		}

		SubpageListingEntry main;
		SubpageListingEntry changelog;
		SubpageListingEntry recommended;
		Dictionary<SubpageListingEntry, Drawable> subpageDrawables = new();

		[Resolved]
		private LocalisationManager localisation { get; set; }

		ILocalisedBindableString contentBindable;

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
				Child = new Container {
					RelativeSizeAxes = Axes.Both,
					Padding = new MarginPadding { Horizontal = 6 },
					Y = -6,
					Child = subpageTabControl = new() {
						RelativeSizeAxes = Axes.X
					}
				},
				Margin = new MarginPadding { Bottom = 12 }
			} );

			content.Add( info = new FillFlowContainer {
				Padding = new MarginPadding { Horizontal = -32 },
				Direction = FillDirection.Vertical,
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y
			} );

			content.Add( subpageContent = new Container {
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Margin = new MarginPadding { Top = 16, Bottom = 400 }
			} );

			subpageTabControl.Current.BindValueChanged( _ => {
				loadCurrentPage();
			} );

			main = new() { Title = Localisation.Strings.MainPage };
			mainPageMarkdown = createMarkdownContainer();
			loadSubpages();

			bool isCoverLoaded = false;
			cover.Texture = defaultCover;
			API.RequestImage( StaticAPIResource.DefaultCover, texture => {
				if ( !isCoverLoaded ) {
					cover.Texture = texture;
				}
			} );

			ruleset.RequestDarkCover( texture => {
				isCoverLoaded = true;
				cover.Texture = texture;
			} );

			loadHeader();
		}

		void loadSubpages () {
			Overlay.StartLoading( this );

			subpageTabControl.Current.Value = null;
			subpageTabControl.Clear();
			subpageDrawables.Clear();

			subpageDrawables.Add( main, mainPageMarkdown );
			changelog = null;

			void addDefaultSubpages () {
				subpageTabControl.AddItem( main );
				subpageTabControl.PinItem( main );

				if ( ruleset.ListingEntry?.Status?.Changelog is string log && !string.IsNullOrWhiteSpace( log ) ) {
					changelog = new() { Title = Localisation.Strings.ChangelogPage };
					subpageTabControl.AddItem( changelog );
					subpageTabControl.PinItem( changelog );
					subpageDrawables.Add( changelog, createMarkdownContainer().With( d => d.Text = log ) );
				}

				ruleset.RequestRecommendations( RurusettoAPI.RecommendationSource.All, r => {
					if ( r.Count == 0 || OnlineAPI is null )
						return;

					recommended = new() { Title = Localisation.Strings.RecommendedBeatmapsPage };
					subpageTabControl.AddItem( recommended );
					subpageTabControl.PinItem( recommended );
				} );
			}

			ruleset.RequestSubpages( subpages => {
				addDefaultSubpages();

				foreach ( var i in subpages ) {
					subpageTabControl.AddItem( i );
				}

				subpageTabControl.Current.Value = main;
				Overlay.FinishLoadiong( this );

			}, failure: e => {
				addDefaultSubpages();

				info.Add( new RequestFailedDrawable {
					ContentText = Localisation.Strings.SubpagesFetchError,
					ButtonClicked = RefreshSubpages
				} );

				subpageTabControl.Current.Value = main;
				Overlay.FinishLoadiong( this );
			} );
		}

		public void RefreshSubpages () {
			info.Clear();
			ruleset.FlushSubpageListing();
			loadSubpages();
		}

		void loadCurrentPage () {
			var page = subpageTabControl.Current.Value;

			subpageContent.Clear( disposeChildren: false );
			if ( page is null )
				return;

			if ( page == main ) {
				loadMainPage();
			}

			if ( page == recommended ) {
				loadRecommendedPage();
			}

			if ( !subpageDrawables.TryGetValue( page, out var subpage ) ) {
				var markdown = createMarkdownContainer();
				var content = new FillFlowContainer {
					RelativeSizeAxes = Axes.X,
					AutoSizeAxes = Axes.Y,
					Direction = FillDirection.Vertical,
					Child = markdown
				};

				Overlay.StartLoading( this );
				ruleset.RequestSubpage( page.Slug, subpage => {
					markdown.Text = subpage.Content ?? "";
					Overlay.FinishLoadiong( this );

				}, failure: e => {
					content.Insert( -1, new Container {
						Padding = new MarginPadding { Horizontal = -32 },
						AutoSizeAxes = Axes.Y,
						RelativeSizeAxes = Axes.X,
						Child = new RequestFailedDrawable {
							ContentText = Localisation.Strings.PageFetchError,
							ButtonClicked = RefreshCurrentPage
						}
					} );

					Overlay.FinishLoadiong( this );
				} );

				subpage = content;
				subpageDrawables.Add( page, content );
			}

			subpageContent.Add( subpage );
		}

		public void RefreshCurrentPage () {
			var page = subpageTabControl.Current.Value;
			if ( page is null )
				return;

			if ( page != main && page != changelog ) {
				subpageDrawables.Remove( page );
				ruleset.FlushSubpage( page.Slug );
			}

			loadCurrentPage();
		}

		void loadMainPage () {
			Overlay.StartLoading( this );
			ruleset.RequestDetail( detail => {
				contentBindable?.UnbindAll();
				contentBindable = localisation.GetLocalisedBindableString( detail.Content );

				contentBindable.BindValueChanged( v => {
					mainPageMarkdown.Text = v.NewValue;
				}, true );
				Overlay.FinishLoadiong( this );

			}, failure: e => {
				// TODO RequestFailedDrawable
				API.LogFailure( $"Could not retrieve detail for {ruleset}", e );
				Overlay.FinishLoadiong( this );
			} );
		}

		[Resolved( canBeNull: true )]
		protected IAPIProvider OnlineAPI { get; private set; }

		private void loadRecommendedPage () {
			if ( subpageDrawables.ContainsKey( recommended ) )
				return;

			Overlay.StartLoading( this );

			var container = new Container {
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y
			};
			subpageDrawables.Add( recommended, container );

			ruleset.RequestRecommendations( RurusettoAPI.RecommendationSource.All, r => {
				var all = new ReverseChildIDFillFlowContainer<Drawable> {
					Direction = FillDirection.Vertical,
					RelativeSizeAxes = Axes.X,
					AutoSizeAxes = Axes.Y,
					Spacing = new( 10 )
				};
				container.Child = all;

				int loadedCount = 0;
				foreach ( var group in r.GroupBy( x => x.Recommender.ID ).OrderBy( x => (x.Key.Value == ruleset.Owner?.ID) ? 1 : 2 ).ThenByDescending( x => x.Count() ) ) {
					var list = new ReverseChildIDFillFlowContainer<Drawable> {
						Direction = FillDirection.Full,
						RelativeSizeAxes = Axes.X,
						AutoSizeAxes = Axes.Y,
						Spacing = new( 10 ),
						Anchor = Anchor.TopCentre,
						Origin = Anchor.TopCentre,
						Margin = new() { Bottom = 20 }
					};
					all.Add( new GridContainer {
						RelativeSizeAxes = Axes.X,
						Width = 0.9f,
						Height = 30,
						Anchor = Anchor.TopCentre,
						Origin = Anchor.TopCentre,
						ColumnDimensions = new Dimension[] {
							new(),
							new( GridSizeMode.AutoSize ),
							new()
						},
						Content = new Drawable[][] {
							new Drawable[] {
								new Circle {
									Height = 3,
									RelativeSizeAxes = Axes.X,
									Colour = colourProvider.Colour1,
									Anchor = Anchor.Centre,
									Origin = Anchor.Centre
								},
								new DrawableRurusettoUser( Users.GetUserIdentity( group.Key.Value ), group.Key == ruleset.Owner?.ID ) {
									Anchor = Anchor.Centre,
									Origin = Anchor.Centre,
									Height = 30,
									Margin = new MarginPadding { Horizontal = 10 }
								},
								new Circle {
									Height = 3,
									RelativeSizeAxes = Axes.X,
									Colour = colourProvider.Colour1,
									Anchor = Anchor.Centre,
									Origin = Anchor.Centre
								},
							}
						}
					} );
					all.Add( list );

					foreach ( var i in group ) {
						var request = new GetBeatmapSetRequest( i.BeatmapID, BeatmapSetLookupType.BeatmapId );

						request.Success += v => {
							v.Beatmaps = v.Beatmaps.Where( x => x.DifficultyName == i.Version ).ToArray();

							list.Add( new BeatmapCardNormal( v ) {
								Anchor = Anchor.TopCentre
							} );

							if ( ++loadedCount == r.Count ) {
								Overlay.FinishLoadiong( this );
							}
						};
						request.Failure += v => {
							list.Add( new BeatmapCardNormal( new() {
								Artist = i.Artist,
								ArtistUnicode = i.Artist,
								BPM = i.BPM,
								Title = i.Title,
								TitleUnicode = i.Title,
								AuthorString = i.Creator,
								Status = (BeatmapOnlineStatus)i.Status,
								Beatmaps = new APIBeatmap[] {
								new() {
									BPM = i.BPM,
									StarRating = i.StarDifficulty
								}
							}
							} ) {
								Anchor = Anchor.TopCentre
							} );

							if ( ++loadedCount == r.Count ) {
								Overlay.FinishLoadiong( this );
							}
						};

						if ( OnlineAPI is DummyAPIAccess )
							request.Fail( new Exception() );
						else
							OnlineAPI.PerformAsync( request );
					}
				}
			}, failure: e => {
				container.Child = new RequestFailedDrawable {
					ContentText = Localisation.Strings.ErrorMessageGeneric
					// TODO retry
				};
				API.LogFailure( $"Could not retrieve recommendations for {ruleset}", e );
				Overlay.FinishLoadiong( this );
			} );
		}

		void loadHeader () {
			Overlay.StartLoading( this );
			ruleset.RequestDetail( detail => {
				Tags.Clear();
				Status.Clear();

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
				Overlay.FinishLoadiong( this );

			}, failure: e => {
				API.LogFailure( $"Could not retrieve detail for {ruleset}", e );
				Overlay.FinishLoadiong( this );
			} );
		}

		public override bool Refresh () {
			RefreshSubpages();

			return true;
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

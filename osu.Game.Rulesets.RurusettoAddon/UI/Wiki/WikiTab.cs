using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Rulesets.RurusettoAddon.UI.Users;
using osuTK.Input;

#nullable disable
namespace osu.Game.Rulesets.RurusettoAddon.UI.Wiki;

[Cached]
public class WikiTab : OverlayTab {
	FillFlowContainer content;
	APIRuleset ruleset;
	Sprite cover;
	FillFlowContainer info;
	Container subpageContent;
	MarkdownPage mainPage;
	protected FillFlowContainer Tags;
	protected FillFlowContainer Status;
	SubpageSectionTabControl subpageTabControl;
	FillFlowContainer buttons;
	public WikiTab ( APIRuleset ruleset ) {
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
	Dictionary<SubpageListingEntry, WikiPage> subpageDrawables = new();

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
						new DrawableRurusettoUser( Users.GetUser( ruleset.Owner ), ruleset.IsVerified ) {
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
			Margin = new MarginPadding { Top = 16 }
		} );

		subpageTabControl.Current.BindValueChanged( _ => {
			loadCurrentPage();
		} );

		main = new() { Title = Localisation.Strings.MainPage };
		mainPage = new MarkdownPage( ruleset );
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

		subpageDrawables.Add( main, mainPage );
		changelog = null;

		void addDefaultSubpages () {
			subpageTabControl.AddItem( main );
			subpageTabControl.PinItem( main );

			if ( ruleset.ListingEntry?.Status?.Changelog is string log && !string.IsNullOrWhiteSpace( log ) ) {
				changelog = new() { Title = Localisation.Strings.ChangelogPage };
				subpageTabControl.AddItem( changelog );
				subpageTabControl.PinItem( changelog );
				subpageDrawables.Add( changelog, new MarkdownPage( ruleset ) { Text = log } );
			}

			ruleset.RequestRecommendations( RurusettoAPI.RecommendationSource.All, r => {
				if ( r.Count == 0 )
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

		if ( !subpageDrawables.TryGetValue( page, out var subpage ) ) {
			if ( page == recommended ) {
				subpageDrawables.Add( recommended, subpage = new RecommendedBeatmapsPage( ruleset ) );
			}
			else {
				subpageDrawables.Add( page, subpage = new WikiSubpage( ruleset, page.Slug ) );
			}
		}

		subpageContent.Add( subpage );
	}

	public void RefreshCurrentPage () {
		var page = subpageTabControl.Current.Value;
		if ( page is null )
			return;

		if ( !subpageDrawables[page].Refresh() ) {
			subpageDrawables.Remove( page );
			loadCurrentPage();
		}
	}

	void loadMainPage () {
		Overlay.StartLoading( this );
		ruleset.RequestDetail( detail => {
			contentBindable?.UnbindAll();
			contentBindable = localisation.GetLocalisedBindableString( detail.Content );

			contentBindable.BindValueChanged( v => {
				mainPage.Text = v.NewValue;
			}, true );
			Overlay.FinishLoadiong( this );

		}, failure: e => {
			// TODO RequestFailedDrawable
			API.LogFailure( $"Could not retrieve detail for {ruleset}", e );
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

	protected override bool OnKeyDown ( KeyDownEvent e ) {
		if ( e.Key is Key.F5 ) { // NOTE o!f doenst seem to have a 'refresh' action
			if ( e.ShiftPressed )
				return Refresh();
			else {
				RefreshCurrentPage();
				return true;
			}
		}

		return false;
	}

	public override bool Refresh () {
		RefreshSubpages();

		return true;
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
using Humanizer;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Rulesets.RurusettoAddon.UI.Users;

namespace osu.Game.Rulesets.RurusettoAddon.UI.Listing;

public class DrawableListingEntry : VisibilityContainer {
	protected override bool StartHidden => false;

	private BufferedContainer content;
	protected override Container<Drawable> Content => content;

	[Resolved]
	protected RurusettoOverlay Overlay { get; private set; } = null!;
	[Resolved]
	protected RurusettoAPI API { get; private set; } = null!;
	[Resolved]
	protected RulesetDownloadManager DownloadManager { get; private set; } = null!;
	[Resolved]
	protected APIUserStore Users { get; private set; } = null!;
	public APIRuleset Ruleset { get; private set; }
	protected FillFlowContainer Tags;

	public DrawableListingEntry ( APIRuleset ruleset ) {
		Ruleset = ruleset;

		Height = 160;
		Masking = true;
		CornerRadius = 8;
		AlwaysPresent = true;

		Margin = new MarginPadding {
			Horizontal = 8,
			Vertical = 8
		};

		Tags = new FillFlowContainer {
			Direction = FillDirection.Horizontal,
			AutoSizeAxes = Axes.Both,
			Spacing = new Vector2( 6, 0 )
		};

		AddInternal( content = new() {
			RelativeSizeAxes = Axes.Both
		} );

		AddInternal( new RulesetManagementContextMenu( ruleset ) );
	}

	ILocalisedBindableString? nameBindable;

	[BackgroundDependencyLoader]
	private void load ( OverlayColourProvider colours, LocalisationManager localisation, GameHost host, TextureStore textures, RurusettoAddonRuleset ruleset ) {
		var color = colours.Background4;
		Sprite cover;
		Drawable coverContainer;

		Add( new Box {
			Colour = color,
			RelativeSizeAxes = Axes.Both
		} );
		Add( coverContainer = new Container {
			RelativeSizeAxes = Axes.X,
			Height = 80,
			Children = new Drawable[] {
				cover = new Sprite {
					RelativeSizeAxes = Axes.Both,
					FillMode = FillMode.Fill,
					Origin = Anchor.Centre,
					Anchor = Anchor.Centre,
					Scale = new Vector2( 1.2f )
				},
				new Box {
					Colour = new ColourInfo {
						HasSingleColour = false,
						BottomLeft = color.Opacity( 0.75f ),
						BottomRight = color.Opacity( 0.75f ),
						TopLeft = color.Opacity( 0.5f ),
						TopRight = color.Opacity( 0.5f )
					},
					RelativeSizeAxes = Axes.Both,
					Anchor = Anchor.BottomCentre,
					Origin = Anchor.BottomCentre
				}
			}
		} );
		Add( new Box {
			Colour = color,
			RelativeSizeAxes = Axes.X,
			Height = Height - coverContainer.Height,
			Origin = Anchor.BottomLeft,
			Anchor = Anchor.BottomLeft
		} );
		OsuSpriteText rulesetName;
		Add( new Container {
			Padding = new MarginPadding( 24f * 14 / 20 ) { Bottom = 24f * 14 / 20 - 4 },
			RelativeSizeAxes = Axes.Both,
			Children = new Drawable[] {
				Tags,
				new RulesetLogo( Ruleset ) {
					Width = 80f * 14 / 20,
					Height = 80f * 14 / 20,
					Anchor = Anchor.CentreLeft,
					Origin = Anchor.CentreLeft
				},
				new Container {
					AutoSizeAxes = Axes.X,
					Anchor = Anchor.CentreLeft,
					Origin = Anchor.CentreLeft,
					Height = 80f * 14 / 20,
					X = (80 + 12) * 14 / 20,
					Children = new Drawable[] {
						rulesetName = new OsuSpriteText {
							Font = OsuFont.GetFont( size: 24 )
						},
						new DrawableRurusettoUser( Users.GetUser( Ruleset.Owner ), Ruleset.IsVerified ) {
							Height = 34f * 14 / 20,
							Origin = Anchor.BottomLeft,
							Anchor = Anchor.BottomLeft
						}
					}
				},
				new GridContainer {
					RelativeSizeAxes = Axes.X,
					Height = 50f * 14 / 20,
					Anchor = Anchor.BottomLeft,
					Origin = Anchor.BottomLeft,
					ColumnDimensions = new Dimension[] {
						new( GridSizeMode.Distributed ),
						new( GridSizeMode.Absolute, 50f * 14 / 20 )
					},
					RowDimensions = new Dimension[] {
						new( GridSizeMode.Distributed )
					},
					Content = new Drawable[][] {
						new Drawable[] {
							new TogglableScrollContainer {
								RelativeSizeAxes = Axes.X,
								Padding = new MarginPadding { Right = 4 },
								Height = 30,
								ScrollbarVisible = false,
								Anchor = Anchor.BottomLeft,
								Origin = Anchor.BottomLeft,
								Child = new OsuTextFlowContainer( s => s.Font = OsuFont.GetFont( size: 14 ) ) {
									AutoSizeAxes = Axes.Y,
									RelativeSizeAxes = Axes.X,
									Text = Ruleset.Description
								}
							},
							new RulesetDownloadButton( Ruleset ) {
								RelativeSizeAxes = Axes.Both,
								ProvideContextMenu = false
							}
						}
					}
				}
			}
		} );

		nameBindable = localisation.GetLocalisedBindableString( Ruleset.Name );
		nameBindable.BindValueChanged( v => {
			rulesetName.Text = v.NewValue.Humanize().ToLower();
		}, true );

		bool isCoverLoaded = false;
		cover.Texture = ruleset.GetTexture( host, textures, TextureNames.DefaultCover );
		API.RequestImage( StaticAPIResource.DefaultCover, texture => {
			if ( !isCoverLoaded ) {
				cover.Texture = texture;
			}
		} );

		Ruleset.RequestDarkCover( texture => {
			cover.Texture = texture;
			isCoverLoaded = true;
		} );

		Ruleset.RequestDetail( detail => {
			Tags.AddRange( Ruleset.GenerateTags( detail ) );
		} );

		Add( new HoverClickSounds() );
	}

	private bool isMaskedAway = true;
	protected override bool ComputeIsMaskedAway ( RectangleF maskingBounds ) {
		return isMaskedAway = base.ComputeIsMaskedAway( maskingBounds );
	}

	protected override void Update () {
		base.Update();

		var availableSize = Parent.ChildSize.X * 0.9f + Margin.Left + Margin.Right;
		const float minWidth = 280;
		int entriesPerLine = (int)Math.Max( 1, availableSize / ( minWidth + Margin.Left + Margin.Right ) );
		Width = availableSize / entriesPerLine - Margin.Left - Margin.Right;

		if ( State.Value == Visibility.Hidden && !isMaskedAway ) {
			Show();
		}
	}

	protected override void PopIn () {
		Alpha = 1;
		using ( BeginDelayedSequence( 100, true ) ) {
			content.MoveToY( 200 ).Then().MoveToY( 0, 500, Easing.Out );
			content.FadeIn( 350 );
		}
	}

	protected override void PopOut () {
		content.Alpha = 0;
		Alpha = 0;
	}

	protected override bool OnClick ( ClickEvent e ) {
		Overlay.Header.NavigateTo(
			Ruleset,
			Ruleset.Name == Localisation.Strings.UntitledRuleset
				? Ruleset.Name
				: Ruleset.Name.ToString().Humanize().ToLower(),
			perserveCategories: true
		);

		return true;
	}
}
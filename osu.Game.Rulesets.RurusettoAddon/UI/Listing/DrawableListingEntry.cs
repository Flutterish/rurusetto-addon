using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.RurusettoAddon.API;
using osu.Game.Rulesets.RurusettoAddon.UI.Overlay;
using osuTK;

namespace osu.Game.Rulesets.RurusettoAddon.UI.Listing {
	public class DrawableListingEntry : VisibilityContainer {
		Sprite cover;
		Drawable coverContainer;

		protected override bool StartHidden => false;

		private BufferedContainer content;
		protected override Container<Drawable> Content => content;

		[Resolved]
		protected RurusettoOverlay Overlay { get; private set; }
		[Resolved]
		protected RurusettoAPI API { get; private set; }
		protected ListingEntry Entry;
		protected FillFlowContainer Tags;

		public DrawableListingEntry ( ListingEntry entry ) {
			Entry = entry;

			RelativeSizeAxes = Axes.X;
			Width = 0.3f;
			Height = 160;
			Masking = true;
			CornerRadius = 8;
			AlwaysPresent = true;

			AddInternal( content = new() {
				RelativeSizeAxes = Axes.Both
			} );

			Margin = new MarginPadding {
				Horizontal = 8,
				Vertical = 8
			};

			var color = Colour4.FromHex( "#2E3835" );
			var color2 = Colour4.FromHex( "#394642" );

			Add( new Box {
				Colour = color,
				RelativeSizeAxes = Axes.Both,
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
			Add( new Container {
				Padding = new MarginPadding( 24f * 14 / 20 ) { Bottom = 24f * 14 / 20 - 4 },
				RelativeSizeAxes = Axes.Both,
				Children = new Drawable[] {
					Tags = new FillFlowContainer {
						Direction = FillDirection.Horizontal,
						AutoSizeAxes = Axes.Both,
						Spacing = new Vector2( 6, 0 )
					},
					new RulesetLogo( entry ) {
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
							new OsuSpriteText {
								Text = entry.Name.ToLower(),
								Font = OsuFont.GetFont( size: 24 )
							},
							new DrawableRurusettoUser( entry.Owner, entry.IsVerified ) {
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
										Text = entry.Description
									}
								},
								new RulesetDownloadButton( entry ) {
									RelativeSizeAxes = Axes.Both
								}
							}
						}
					}
				}
			} );
		}
		protected override void LoadComplete () {
			base.LoadComplete();

			bool isCoverLoaded = false;
			API.RequestImage( StaticAPIResource.DefaultCover ).ContinueWith( t => Schedule( () => {
				if ( !isCoverLoaded ) {
					cover.Texture = t.Result;
				}
			} ) );

			API.RequestRulesetDetail( Entry.ShortName ).ContinueWith( t => {
				if ( t.Result.IsArchived ) {
					Schedule( () => {
						Tags.Add( DrawableTag.CreateArchived() );
					} );
				}

				API.RequestImage( t.Result.CoverDark ).ContinueWith( t => {
					Schedule( () => {
						cover.Texture = t.Result;
						isCoverLoaded = true;
					} );
				} );
			} );

			Add( new HoverClickSounds() );
		}

		private bool isMaskedAway = true;
		protected override bool ComputeIsMaskedAway ( RectangleF maskingBounds ) {
			return isMaskedAway = base.ComputeIsMaskedAway( maskingBounds );
		}

		protected override void Update () {
			base.Update();

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
			Overlay.Header.SelectedRuleset.Value = Entry;
			
			return true;
		}
	}
}

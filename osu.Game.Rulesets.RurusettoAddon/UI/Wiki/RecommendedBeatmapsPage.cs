using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Graphics.Containers.Markdown;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.RurusettoAddon.UI.Users;

namespace osu.Game.Rulesets.RurusettoAddon.UI.Wiki;

public class RecommendedBeatmapsPage : WikiPage {
	public RecommendedBeatmapsPage ( APIRuleset ruleset ) : base( ruleset ) { }

	public override bool Refresh () {
		ClearInternal();
		Ruleset.FlushRecommendations();
		loadRecommendedPage();

		return true;
	}

	[Resolved]
	protected IAPIProvider OnlineAPI { get; private set; } = null!;

	private void loadRecommendedPage () {
		Overlay.StartLoading( Tab );

		// this is here because if we added to internal instead, if we refresh we could add things twice
		var container = new Container {
			RelativeSizeAxes = Axes.X,
			AutoSizeAxes = Axes.Y
		};
		AddInternal( container );

		Ruleset.RequestRecommendations( RurusettoAPI.RecommendationSource.All, r => {
			var all = new ReverseChildIDFillFlowContainer<Drawable> {
				Direction = FillDirection.Vertical,
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Spacing = new( 10 )
			};
			container.Child = all;

			int loadedCount = 0;
			foreach ( var group in r.GroupBy( x => x.Recommender.ID!.Value ).OrderBy( x => x.Key == Ruleset.Owner?.ID ? 1 : 2 ).ThenByDescending( x => x.Count() ) ) {
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
								Colour = ColourProvider.Colour1,
								Anchor = Anchor.Centre,
								Origin = Anchor.Centre
							},
							new DrawableRurusettoUser( Users.GetUser( group.Key ), group.Key == Ruleset.Owner?.ID ) {
								Anchor = Anchor.Centre,
								Origin = Anchor.Centre,
								Height = 30,
								Margin = new MarginPadding { Horizontal = 10 }
							},
							new Circle {
								Height = 3,
								RelativeSizeAxes = Axes.X,
								Colour = ColourProvider.Colour1,
								Anchor = Anchor.Centre,
								Origin = Anchor.Centre
							},
						}
					}
				} );
				all.Add( list );

				void add ( BeatmapRecommendation i, APIBeatmapSet v ) {
					v.Beatmaps = v.Beatmaps.Where( x => x.DifficultyName == i.Version ).ToArray();

					list.Add( new ReverseChildIDFillFlowContainer<Drawable> {
						AutoSizeAxes = Axes.Both,
						Direction = FillDirection.Vertical,
						Anchor = Anchor.TopCentre,
						Origin = Anchor.TopCentre,
						Children = new Drawable[] {
							new BeatmapCardNormal( v ),
							new FillFlowContainer {
								AutoSizeAxes = Axes.Y,
								RelativeSizeAxes = Axes.X,
								Direction = FillDirection.Horizontal,
								Margin = new() { Top = 5 },
								Children = new Drawable[] {
									new SpriteIcon {
										Icon = FontAwesome.Solid.QuoteLeft,
										Size = new Vector2( 34, 18 ) * 0.6f
									},
									new OsuMarkdownContainer {
										AutoSizeAxes = Axes.Y,
										RelativeSizeAxes = Axes.X,
										Text = i.Comment,
										Margin = new() { Left = 4 - 38 * 0.6f }
									}
								}
							}
						}
					} );
				}

				foreach ( var i in group.OrderByDescending( x => x.CreatedAt ) ) {
					var request = new GetBeatmapSetRequest( i.BeatmapID, BeatmapSetLookupType.BeatmapId );

					request.Success += v => {
						add( i, v );

						if ( ++loadedCount == r.Count ) {
							Overlay.FinishLoadiong( Tab );
						}
					};
					void onFail ( Exception e ) {
						add( i, new() {
							Artist = i.Artist,
							ArtistUnicode = i.Artist,
							BPM = i.BPM,
							Title = i.Title,
							TitleUnicode = i.Title,
							AuthorString = i.Creator,
							Status = (BeatmapOnlineStatus)i.Status,
							Beatmaps = new APIBeatmap[] {
								new() {
									OnlineID = i.BeatmapID,
									OnlineBeatmapSetID = i.BeatmapSetID,
									RulesetID = 1,
									BPM = i.BPM,
									StarRating = i.StarDifficulty
								}
							}
						} );

						if ( ++loadedCount == r.Count ) {
							Overlay.FinishLoadiong( Tab );
						}
					}
					request.Failure += onFail;

					if ( OnlineAPI is DummyAPIAccess )
						onFail( null! );
					else
						OnlineAPI.PerformAsync( request );
				}
			}
		}, failure: e => {
			container.Child = new RequestFailedDrawable {
				ContentText = Localisation.Strings.ErrorMessageGeneric
				// TODO retry
			};
			API.LogFailure( $"Could not retrieve recommendations for {Ruleset}", e );
			Overlay.FinishLoadiong( Tab );
		} );
	}
}

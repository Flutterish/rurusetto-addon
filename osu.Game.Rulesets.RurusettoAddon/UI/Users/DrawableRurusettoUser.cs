using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays;
using osu.Game.Rulesets.RurusettoAddon.API;
using osu.Game.Rulesets.RurusettoAddon.UI.Overlay;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace osu.Game.Rulesets.RurusettoAddon.UI.Users {
	public class DrawableRurusettoUser : CompositeDrawable, IHasTooltip {
		[Resolved, MaybeNull, NotNull]
		protected RurusettoOverlay Overlay { get; private set; }

		[Resolved( canBeNull: true )]
		protected UserProfileOverlay? ProfileOverlay { get; private set; }
		[Resolved( canBeNull: true )]
		protected IAPIProvider? OnlineAPI { get; private set; }

		private UserIdentity user;
		private Container pfpContainer;
		private Sprite pfp;

		FillFlowContainer usernameFlow;
		LocalisableString usernameText;
		OsuTextFlowContainer username;
		FillFlowContainer verticalFlow;
		private bool isVerified;
		Drawable? verifiedDrawable;
		UserProfile? profile;

		public DrawableRurusettoUser ( UserIdentity user, bool isVerified = false ) {
			this.isVerified = isVerified;
			this.user = user;
			AutoSizeAxes = Axes.X;

			var color2 = Colour4.FromHex( "#394642" );

			AddInternal( new HoverClickSounds( HoverSampleSet.Button ) );
			AddInternal( usernameFlow = new FillFlowContainer {
				RelativeSizeAxes = Axes.Y,
				AutoSizeAxes = Axes.X,
				Direction = FillDirection.Horizontal,
				Children = new Drawable[] {
					pfpContainer = new Container {
						Anchor = Anchor.CentreLeft,
						Origin = Anchor.CentreLeft,
						Children = new Drawable[] {
							new Box {
								RelativeSizeAxes = Axes.Both,
								FillAspectRatio = 1,
								FillMode = FillMode.Fill,
								Colour = color2
							},
							pfp = new Sprite {
								RelativeSizeAxes = Axes.Both,
								FillMode = FillMode.Fit
							}
						},
						Masking = true,
						CornerRadius = 4,
						Margin = new MarginPadding { Right = 12 }
					},
					verticalFlow = new FillFlowContainer {
						Direction = FillDirection.Vertical,
						AutoSizeAxes = Axes.Both,
						Anchor = Anchor.CentreLeft,
						Origin = Anchor.CentreLeft,
						Spacing = new osuTK.Vector2( 0, 4 ),
						Child = username = new OsuTextFlowContainer {
							TextAnchor = Anchor.CentreLeft,
							Anchor = Anchor.CentreLeft,
							Origin = Anchor.CentreLeft,
							AutoSizeAxes = Axes.Both,
							Margin = new MarginPadding { Right = 5 }
						}
					}
				}
			} );

			makeShort();
		}

		private bool isTall;
		protected override void Update () {
			base.Update();

			pfpContainer.Width = pfpContainer.Height = DrawHeight;
			if ( DrawHeight > 34 && !isTall )
				makeTall();
			else if ( DrawHeight <= 34 && isTall )
				makeShort();
		}

		private void makeShort () {
			isTall = false;

			if ( verifiedDrawable != null ) {
				( verifiedDrawable.Parent as Container<Drawable> )!.Remove( verifiedDrawable );
			}

			if ( isVerified ) {
				usernameFlow.Add( verifiedDrawable = new VerifiedIcon {
					Anchor = Anchor.CentreLeft,
					Origin = Anchor.CentreLeft,
					Height = 15,
					Width = 15
				} );
			}
		}

		private void makeTall () {
			isTall = true;

			if ( verifiedDrawable != null ) {
				( verifiedDrawable.Parent as Container<Drawable> )!.Remove( verifiedDrawable );
			}

			if ( isVerified ) {
				verticalFlow.Add( verifiedDrawable = new FillFlowContainer {
					AutoSizeAxes = Axes.Both,
					Direction = FillDirection.Horizontal,
					Anchor = Anchor.CentreLeft,
					Origin = Anchor.CentreLeft,
					Children = new Drawable[] {
						new VerifiedIcon {
							Anchor = Anchor.CentreLeft,
							Origin = Anchor.CentreLeft,
							Height = 15,
							Width = 15
						},
						new OsuSpriteText {
							Colour = Colour4.HotPink,
							Text = "Verified Ruleset Creator",
							Anchor = Anchor.CentreLeft,
							Origin = Anchor.CentreLeft,
							Margin = new MarginPadding { Left = 5 }
						}
					}
				} );
			}
		}

		protected override void LoadComplete () {
			base.LoadComplete();

			user.RequestDetail( profile => {
				this.profile = profile;
				usernameText = profile.Username ?? "";
				username.Text = profile.Username ?? "Unknown";
			}, failure: () => { /* TODO report this */ } );

			user.RequestProfilePicture( texture => {
				pfp.Texture = texture;
			}, fallback => {
				pfp.Texture = fallback;
			} );
		}

		public LocalisableString TooltipText => usernameText;

		protected override bool OnClick ( ClickEvent e ) {
			if ( profile?.OsuUsername is string osuUsername && ProfileOverlay != null && OnlineAPI != null ) {
				var request = new GetUserRequest( osuUsername );
				request.Success += v => {
					ProfileOverlay.ShowUser( v );
				};
				request.Failure += v => {
					// :(
				};
				OnlineAPI.PerformAsync( request );
			}

			//if ( user.HasProfile )
			//	Overlay.Header.SelectedInfo.Value = user;

			return true;
		}
	}
}

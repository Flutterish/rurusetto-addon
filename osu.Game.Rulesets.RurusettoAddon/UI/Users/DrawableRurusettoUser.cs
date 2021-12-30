using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.RurusettoAddon.API;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace osu.Game.Rulesets.RurusettoAddon.UI.Users {
	public class DrawableRurusettoUser : CompositeDrawable, IHasTooltip {
		[Resolved, MaybeNull, NotNull]
		protected RurusettoAPI API { get; private set; }
		private UserDetail? detail;
		private Container pfpContainer;
		private Sprite pfp;

		FillFlowContainer usernameFlow;
		FillFlowContainer verticalFlow;
		private bool isVerified;
		Drawable? verifiedDrawable;

		public DrawableRurusettoUser ( UserDetail? detail, bool isVerified = false ) {
			this.isVerified = isVerified;
			this.detail = detail;
			AutoSizeAxes = Axes.X;

			var color2 = Colour4.FromHex( "#394642" );

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
						Child = new OsuTextFlowContainer {
							TextAnchor = Anchor.CentreLeft,
							Anchor = Anchor.CentreLeft,
							Origin = Anchor.CentreLeft,
							AutoSizeAxes = Axes.Both,
							Text = detail?.Info?.Username ?? "Unknown",
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

			( detail?.Image is null ? API.RequestImage( StaticAPIResource.DefaultProfileImage ) : API.RequestImage( detail.Image ) ).ContinueWith( t => Schedule( () => {
				pfp.Texture = t.Result;
			} ) );
		}

		public LocalisableString TooltipText => detail?.Info?.Username ?? "";
	}
}

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.RurusettoAddon.API;

namespace osu.Game.Rulesets.RurusettoAddon.UI {
	public class DrawableRurusettoUser : CompositeDrawable, IHasTooltip {
		[Resolved]
		protected RurusettoAPI API { get; private set; }
		private UserDetail detail;
		private Container pfpContainer;
		private Sprite pfp;

		FillFlowContainer flow;

		public DrawableRurusettoUser ( UserDetail detail, bool isVerified = false ) {
			this.detail = detail;
			AutoSizeAxes = Axes.X;

			var color2 = Colour4.FromHex( "#394642" );

			AddInternal( flow = new FillFlowContainer {
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
						CornerRadius = 4
					},
					new OsuTextFlowContainer {
						TextAnchor = Anchor.CentreLeft,
						Anchor = Anchor.CentreLeft,
						Origin = Anchor.CentreLeft,
						AutoSizeAxes = Axes.Both,
						Text = detail.Info?.Username ?? "Unknown",
						Margin = new MarginPadding { Left = 12, Right = 5 }
					}
				}
			} );

			if ( isVerified ) {
				flow.Add( new VerifiedIcon {
					Anchor = Anchor.CentreLeft,
					Origin = Anchor.CentreLeft,
					Height = 15,
					Width = 15
				} );
			}
		}

		protected override void Update () {
			base.Update();

			pfpContainer.Width = pfpContainer.Height = DrawHeight;
		}

		protected override void LoadComplete () {
			base.LoadComplete();

			( detail.Image is null ? API.RequestImage( StaticAPIResource.DefaultProfileImage ) : API.RequestImage( detail.Image ) ).ContinueWith( t => Schedule( () => {
				pfp.Texture = t.Result;
			} ) );
		}

		public LocalisableString TooltipText => detail.Info?.Username ?? "";
	}
}

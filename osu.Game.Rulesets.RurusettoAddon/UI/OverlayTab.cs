using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.RurusettoAddon.API;
using osu.Game.Rulesets.RurusettoAddon.UI.Overlay;

namespace osu.Game.Rulesets.RurusettoAddon.UI {
	public abstract class OverlayTab : VisibilityContainer {
		[Resolved]
		protected RurusettoOverlay Overlay { get; private set; }
		[Resolved]
		protected RurusettoAPI API { get; private set; }
		[Resolved]
		protected RulesetDownloadManager DownloadManager { get; private set; }
		[Resolved]
		protected RulesetIdentityManager Rulesets { get; private set; }
		[Resolved]
		protected UserIdentityManager Users { get; private set; }

		public OverlayTab () {
			RelativeSizeAxes = Axes.X;
			AutoSizeAxes = Axes.Y;

			Origin = Anchor.TopCentre;
			Anchor = Anchor.TopCentre;
		}

		protected override void LoadComplete () {
			base.LoadComplete();

			if ( RequiresLoading ) {
				Overlay.StartLoading( this );
			}

			LoadContent();
		}

		protected override void PopIn () {
			this.FadeIn( 200 ).ScaleTo( 1, 300, Easing.Out );
		}

		protected override void PopOut () {
			this.FadeOut( 200 ).ScaleTo( 0.8f, 300, Easing.Out );
		}

		protected override bool StartHidden => true;

		protected virtual bool RequiresLoading => false;
		protected abstract void LoadContent ();
		protected virtual void OnContentLoaded () {
			Overlay.FinishLoadiong( this );
		}
	}
}

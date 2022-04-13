using osu.Framework.Input.Events;
using osuTK.Input;

namespace osu.Game.Rulesets.RurusettoAddon.UI;

public abstract class OverlayTab : VisibilityContainer {
	[Resolved]
	protected RurusettoOverlay Overlay { get; private set; } = null!;
	[Resolved]
	protected RurusettoAPI API { get; private set; } = null!;
	[Resolved]
	protected RulesetDownloader Downloader { get; private set; } = null!;
	[Resolved]
	protected APIRulesetStore Rulesets { get; private set; } = null!;
	[Resolved]
	protected APIUserStore Users { get; private set; } = null!;

	public OverlayTab () {
		RelativeSizeAxes = Axes.X;
		AutoSizeAxes = Axes.Y;

		Origin = Anchor.TopCentre;
		Anchor = Anchor.TopCentre;
	}

	protected override void LoadComplete () {
		base.LoadComplete();

		LoadContent();
	}

	protected override void PopIn () {
		this.FadeIn( 200 ).ScaleTo( 1, 300, Easing.Out );
	}

	protected override void PopOut () {
		this.FadeOut( 200 ).ScaleTo( 0.8f, 300, Easing.Out );
	}

	protected override bool StartHidden => true;

	protected abstract void LoadContent ();
	protected virtual void OnContentLoaded () { }

	public override bool AcceptsFocus => true;
	public override bool RequestsFocus => true;
	protected override bool OnKeyDown ( KeyDownEvent e ) {
		if ( e.Key is Key.F5 ) { // NOTE o!f doenst seem to have a 'refresh' action
			return Refresh();
		}

		return false;
	}

	public virtual bool Refresh () {
		return false;
	}
}
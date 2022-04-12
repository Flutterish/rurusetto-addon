using osu.Game.Overlays;

namespace osu.Game.Rulesets.RurusettoAddon.UI.Wiki;

public abstract class WikiPage : CompositeDrawable {
	[Resolved]
	protected WikiTab Tab { get; private set; } = null!;
	[Resolved]
	protected RurusettoOverlay Overlay { get; private set; } = null!;
	[Resolved]
	protected RurusettoAPI API { get; private set; } = null!;
	[Resolved]
	protected OverlayColourProvider ColourProvider { get; private set; } = null!;
	[Resolved]
	protected UserIdentityManager Users { get; private set; } = null!;

	protected readonly APIRuleset Ruleset;

	public WikiPage ( APIRuleset ruleset ) {
		RelativeSizeAxes = Axes.X;
		AutoSizeAxes = Axes.Y;
		Ruleset = ruleset;
	}

	public abstract bool Refresh ();

	protected override void LoadComplete () {
		base.LoadComplete();
		Refresh();
	}
}
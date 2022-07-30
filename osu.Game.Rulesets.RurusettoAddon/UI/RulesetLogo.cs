using osu.Game.Overlays;
using TagLib.IFD;

namespace osu.Game.Rulesets.RurusettoAddon.UI;

public class RulesetLogo : CompositeDrawable {
	[Resolved]
	protected RurusettoAPI API { get; private set; } = null!;

	APIRuleset ruleset;
	public bool UseDarkerBackground { get; init; }
	public RulesetLogo ( APIRuleset ruleset ) {
		this.ruleset = ruleset;
	}

	[BackgroundDependencyLoader]
	private void load ( OverlayColourProvider colours ) {
		var color = UseDarkerBackground ? colours.Background4 : colours.Background3;

		InternalChildren = new Drawable[] {
			new Circle {
				RelativeSizeAxes = Axes.Both,
				Colour = color
			}
		};

		ruleset.RequestDarkLogo( logo => {
			try {
				AddInternal( logo );
			}
			catch {
				RemoveInternal( logo );
				ruleset.RequestDarkLogo( AddInternal, AddInternal, useLocalIcon: false );
			}
		}, fallback => AddInternal( fallback ) );
	}

	bool subtreeWorks = true;
	public override bool UpdateSubTree () {
		if ( !subtreeWorks )
			return false;
		
		try {
			return base.UpdateSubTree();
		}
		catch {
			subtreeWorks = false;
			return false;
		}
	}
}
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Overlays;
using osu.Game.Rulesets.RurusettoAddon.API;

namespace osu.Game.Rulesets.RurusettoAddon.UI {
	public class RulesetLogo : CompositeDrawable {
		[Resolved]
		protected RurusettoAPI API { get; private set; }

		RulesetIdentity ruleset;
		public RulesetLogo ( RulesetIdentity ruleset ) {
			this.ruleset = ruleset;
		}

		[BackgroundDependencyLoader]
		private void load ( OverlayColourProvider colours ) {
			var color = colours.Background3;

			InternalChildren = new Drawable[] {
				new Circle {
					RelativeSizeAxes = Axes.Both,
					Colour = color
				}
			};

			ruleset.RequestDarkLogo( logo => {
				AddInternal( logo );
			}, fallback => {
				AddInternal( fallback );
			} );
		}
	}
}

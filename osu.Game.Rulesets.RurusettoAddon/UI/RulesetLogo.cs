using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.RurusettoAddon.API;

namespace osu.Game.Rulesets.RurusettoAddon.UI {
	public class RulesetLogo : CompositeDrawable {
		[Resolved]
		protected RurusettoAPI API { get; private set; }

		RulesetIdentity ruleset;
		public RulesetLogo ( RulesetIdentity ruleset ) {
			this.ruleset = ruleset;
			var color2 = Colour4.FromHex( "#394642" );

			InternalChildren = new Drawable[] {
				new Circle {
					RelativeSizeAxes = Axes.Both,
					Colour = color2
				}
			};
		}

		protected override void LoadComplete () {
			base.LoadComplete();

			ruleset.RequestDarkLogo().ContinueWith( t => Schedule( () => {
				AddInternal( t.Result );
			} ) );
		}
	}
}

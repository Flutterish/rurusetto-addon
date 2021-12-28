using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.RurusettoAddon.API;

namespace osu.Game.Rulesets.RurusettoAddon.UI {
	public class RulesetLogo : CompositeDrawable {
		Sprite logo;
		[Resolved]
		protected RurusettoAPI API { get; private set; }

		ListingEntry entry;
		public RulesetLogo ( ListingEntry entry ) {
			this.entry = entry;
			var color2 = Colour4.FromHex( "#394642" );

			InternalChildren = new Drawable[] {
				new Circle {
					RelativeSizeAxes = Axes.Both,
					Colour = color2
				},
				logo = new Sprite {
					RelativeSizeAxes = Axes.Both,
					FillMode = FillMode.Fit
				}
			};
		}

		protected override void LoadComplete () {
			base.LoadComplete();
			
			if ( entry.LocalRulesetInfo != null ) {
				RemoveInternal( logo );
				AddInternal( entry.LocalRulesetInfo.CreateInstance().CreateIcon().With( d => {
					d.RelativeSizeAxes = Axes.Both;
					d.Size = new osuTK.Vector2( 1 );
				} ) );
			}
			else {
				API.RequestImage( entry.DarkIcon ).ContinueWith( t => Schedule( () => {
					logo.Texture = t.Result;
				} ) );
			}
		}
	}
}

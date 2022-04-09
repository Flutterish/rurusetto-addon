using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;

namespace osu.Game.Rulesets.RurusettoAddon.UI {
	public class TogglableScrollContainer : OsuScrollContainer {
		public TogglableScrollContainer ( Direction direction = Direction.Vertical ) : base( direction ) { }

		protected bool CanScroll => ScrollDirection switch {
			Direction.Vertical => DrawHeight < ScrollContent.DrawHeight,
			_ => DrawWidth < ScrollContent.DrawWidth
		};

		protected override bool OnMouseDown ( MouseDownEvent e ) {
			return CanScroll ? base.OnMouseDown( e ) : false;
		}

		protected override bool OnDragStart ( DragStartEvent e ) {
			return CanScroll ? base.OnDragStart( e ) : false;
		}

		public override bool DragBlocksClick => CanScroll ? base.DragBlocksClick : false;

		protected override bool OnHover ( HoverEvent e ) {
			return CanScroll ? base.OnHover( e ) : false;
		}

		protected override bool OnScroll ( ScrollEvent e ) {
			return CanScroll ? base.OnScroll( e ) : false;
		}
	}
}

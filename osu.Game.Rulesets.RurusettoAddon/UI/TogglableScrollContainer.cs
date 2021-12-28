using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Rulesets.RurusettoAddon.UI {
	public class TogglableScrollContainer : OsuScrollContainer {
		public TogglableScrollContainer () : base( Direction.Vertical ) { }

		protected bool CanScroll => DrawHeight < ScrollContent.DrawHeight;

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
	}
}

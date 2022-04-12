using osu.Framework.Input.Events;

namespace osu.Game.Rulesets.RurusettoAddon.UI;

public class TogglableScrollContainer : OsuScrollContainer {
	public TogglableScrollContainer ( Direction direction = Direction.Vertical ) : base( direction ) { }

	protected bool CanScroll => ScrollDirection switch {
		Direction.Vertical => DrawHeight < ScrollContent.DrawHeight,
		_ => DrawWidth < ScrollContent.DrawWidth
	};

	protected override bool OnMouseDown ( MouseDownEvent e )
		=> CanScroll && base.OnMouseDown( e );

	protected override bool OnDragStart ( DragStartEvent e ) 
		=> CanScroll && base.OnDragStart( e );

	public override bool DragBlocksClick => CanScroll && base.DragBlocksClick;

	protected override bool OnHover ( HoverEvent e )
		=> CanScroll && base.OnHover( e );

	protected override bool OnScroll ( ScrollEvent e ) 
		=> CanScroll && base.OnScroll( e );
}
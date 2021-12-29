using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System.Threading.Tasks;

namespace osu.Game.Rulesets.RurusettoAddon.UI.Listing {
	public class ListingTab : OverlayTab {
		FillFlowContainer content;
		public ListingTab () {
			AddInternal( content = new FillFlowContainer {
				Direction = FillDirection.Full,
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Padding = new MarginPadding { Horizontal = 32, Top = 8 }
			} );
		}

		Task refreshTask = null;
		public void ReloadListing () {
			Schedule( () => {
				Overlay.StartLoading( this );
				content.Clear();
			} );

			Task task = null;
			// TODO we should allow to refresh identities too, but we would need to "update" existing ones
			// rather than create new ones since they are reference based and we use as such
			task = refreshTask = Identities.RequestIdentities().ContinueWith( t => {
				Schedule( () => {
					if ( task != refreshTask )
						return;

					foreach ( var i in t.Result ) {
						content.Add( new DrawableListingEntry( i ) {
							Anchor = Anchor.TopCentre,
							Origin = Anchor.TopCentre
						} );
					}

					OnContentLoaded();
				} );
			} );
		}

		protected override bool RequiresLoading => true;
		protected override void LoadContent () {
			ReloadListing();
		}
	}
}

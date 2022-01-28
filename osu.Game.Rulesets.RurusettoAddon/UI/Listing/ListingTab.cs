using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.RurusettoAddon.API;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace osu.Game.Rulesets.RurusettoAddon.UI.Listing {
	public class ListingTab : OverlayTab {
		ListingEntryContainer content;
		Bindable<string> apiAddress = new( RurusettoAPI.DefaultAPIAddress );
		public ListingTab () {
			AddInternal( content = new() {
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
			task = refreshTask = Rulesets.RequestIdentities().ContinueWith( t => {
				Schedule( () => {
					Overlay.FinishLoadiong( this );
					if ( task != refreshTask )
						return;

					foreach ( var i in t.Result ) {
						content.Add( new DrawableListingEntry( i ) {
							Anchor = Anchor.TopCentre,
							Origin = Anchor.TopCentre
						} );
					}
				} );
			} );
		}

		[BackgroundDependencyLoader]
		private void load () {
			apiAddress.BindTo( API.Address );
			apiAddress.BindValueChanged( _ => Refresh() );
		}

		protected override bool Refresh () {
			Rulesets.Refresh();
			ReloadListing();

			return true;
		}

		protected override bool RequiresLoading => false;
		protected override void LoadContent () {
			ReloadListing();
		}

		private class ListingEntryContainer : FillFlowContainer<DrawableListingEntry> {
			Dictionary<Drawable, APIRuleset> rulesets = new();
			public override IEnumerable<Drawable> FlowingChildren => base.FlowingChildren.OrderBy( x => 
				(rulesets[x].Source == Source.Local) ? 2 : 1
			).ThenBy( x =>
				(rulesets[x].ListingEntry?.CanDownload == true) ? 1 : 2
			).ThenBy( x =>
				(rulesets[x].ListingEntry?.Status?.IsPlayable == true) ? 1 : 2
			).ThenBy( x =>
				(rulesets[x].ListingEntry?.Status?.IsBorked == true) ? 3 : 2
			).ThenByDescending( x => 
				rulesets[x].ListingEntry?.Status?.LatestUpdate
			);

			public override void Add ( DrawableListingEntry drawable ) {
				base.Add( drawable );

				rulesets.Add( drawable, null );
				rulesets[ drawable ] = drawable.Ruleset;
			}

			public override bool Remove ( DrawableListingEntry drawable ) {
				rulesets.Remove( drawable );
				return base.Remove( drawable );
			}
		}
	}
}

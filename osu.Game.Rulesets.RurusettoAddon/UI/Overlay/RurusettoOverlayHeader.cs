using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Overlays;
using osu.Game.Rulesets.RurusettoAddon.API;

namespace osu.Game.Rulesets.RurusettoAddon.UI.Overlay {
	public class RurusettoOverlayHeader : BreadcrumbControlOverlayHeader {
		static readonly LocalisableString listingText = "listing";

		[Resolved]
		protected RurusettoAPI API { get; private set; }

		public RurusettoOverlayHeader () {
			TabControl.AddItem( listingText );

			SelectedRuleset.ValueChanged += v => {
				if ( v.NewValue?.ShortName == v.OldValue?.ShortName )
					return;

				if ( v.OldValue != null ) {
					TabControl.RemoveItem( v.OldValue.Name.ToLower() );
				}

				if ( v.NewValue != null ) {
					TabControl.AddItem( v.NewValue.Name.ToLower() );
					Current.Value = v.NewValue.Name.ToLower();

					API.RequestRulesetDetail( v.NewValue.ShortName ).ContinueWith( t => API.RequestImage( t.Result.CoverDark ).ContinueWith( t => Schedule( () => {
						background.SetCover( t.Result );
					} ) ) );
				}
				else {
					Current.Value = listingText;
					background.SetCover( null );
				}
			};

			Current.ValueChanged += v => {
				if ( v.NewValue == listingText ) {
					SelectedRuleset.Value = null;
				}
			};
		}

		public readonly Bindable<ListingEntry> SelectedRuleset = new();

		protected override OverlayTitle CreateTitle ()
			=> new HeaderTitle();

		private RurusettoOverlayBackground background;
		protected override RurusettoOverlayBackground CreateBackground ()
			=> background = new RurusettoOverlayBackground ();

		private class HeaderTitle : OverlayTitle {
			public HeaderTitle () {
				Title = "rūrusetto";
				Description = "browse and manage rulesets";
				IconTexture = "Icons/Hexacons/chart";
			}
		}
	}
}

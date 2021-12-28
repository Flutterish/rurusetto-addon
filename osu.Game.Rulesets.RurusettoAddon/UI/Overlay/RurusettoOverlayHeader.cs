using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Overlays;
using osu.Game.Rulesets.RurusettoAddon.API;

namespace osu.Game.Rulesets.RurusettoAddon.UI.Overlay {
	public class RurusettoOverlayHeader : BreadcrumbControlOverlayHeader {
		static readonly LocalisableString listingText = "listing";

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
				}
				else {
					Current.Value = listingText;
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

		protected override Drawable CreateBackground ()
			=> new RurusettoOverlayBackground();

		private class HeaderTitle : OverlayTitle {
			public HeaderTitle () {
				Title = "rūrusetto";
				Description = "browse and manage rulesets";
				IconTexture = "Icons/Hexacons/chart";
			}
		}
	}
}

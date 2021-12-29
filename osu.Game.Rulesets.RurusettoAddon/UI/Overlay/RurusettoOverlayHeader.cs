using Humanizer;
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
				if ( v.NewValue?.Name == v.OldValue?.Name )
					return;

				var oldName = v.OldValue?.Name.Humanize().ToLower();
				var newName = v.NewValue?.Name.Humanize().ToLower();

				if ( oldName != null ) {
					TabControl.RemoveItem( oldName );
				}

				if ( newName != null ) {
					TabControl.AddItem( newName ); // TODO this can fail if there are duplicate names
					Current.Value = newName;

					v.NewValue.RequestDetail().ContinueWith( t => v.NewValue.RequestDarkCover( t.Result ).ContinueWith( t => Schedule( () => {
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

		public readonly Bindable<RulesetIdentity> SelectedRuleset = new();

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

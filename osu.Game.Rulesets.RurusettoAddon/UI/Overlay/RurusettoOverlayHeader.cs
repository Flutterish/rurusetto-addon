using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Overlays;
using osu.Game.Rulesets.RurusettoAddon.API;

namespace osu.Game.Rulesets.RurusettoAddon.UI.Overlay {
	public class RurusettoOverlayHeader : BreadcrumbControlOverlayHeader {
		static readonly LocalisableString listingText = Localisation.Strings.ListingTab;

		[Resolved]
		protected RurusettoAPI API { get; private set; }

		public RurusettoOverlayHeader () {
			TabControl.AddItem( listingText );

			SelectedInfo.ValueChanged += v => {
				if ( v.OldValue != null )
					TabControl.RemoveItem( selectedTab );

				if ( v.NewValue is APIRuleset ruleset ) {
					LocalisableString newName = ruleset.Name == Localisation.Strings.UntitledRuleset
					 ? ruleset.Name
					 : ruleset.Name.ToString().Humanize().ToLower();

					TabControl.AddItem( selectedTab = newName ); // TODO this can fail if there are duplicate names?
					Current.Value = selectedTab;

					ruleset.RequestDarkCover( texture => {
						background.SetCover( texture );
					}, failure: () => { /* TODO report this */ } );
				}
				else if ( v.NewValue is APIUser user ) {
					TabControl.AddItem( selectedTab = $"user" );
					Current.Value = selectedTab;

					user.RequestDetail( detail => {
						if ( Current.Value == selectedTab ) {
							TabControl.RemoveItem( selectedTab );
							TabControl.AddItem( selectedTab = $"{detail.Username} (user)" );
							Current.Value = selectedTab;
						}
					}, failure: () => { /* TODO report this */ } );
				}
				else {
					Current.Value = listingText;
					background.SetCover( null );
				}
			};

			Current.ValueChanged += v => {
				if ( v.NewValue == listingText ) {
					SelectedInfo.Value = null;
				}
			};
		}

		/// <summary>
		/// Can be either <see cref="APIRuleset"/>, <see cref="APIUser"/> or <see langword="null"/>
		/// </summary>
		public readonly Bindable<object> SelectedInfo = new();
		LocalisableString selectedTab;

		protected override OverlayTitle CreateTitle ()
			=> new HeaderTitle();

		private RurusettoOverlayBackground background;
		protected override RurusettoOverlayBackground CreateBackground ()
			=> background = new RurusettoOverlayBackground ();

		private class HeaderTitle : OverlayTitle {
			public HeaderTitle () {
				Title = "rūrusetto";
				Description = Localisation.Strings.RurusettoDescription;
				IconTexture = "Icons/Hexacons/chart";
			}
		}
	}
}

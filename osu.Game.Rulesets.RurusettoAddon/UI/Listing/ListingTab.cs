using Humanizer;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.RurusettoAddon.API;
using System.Collections.Generic;
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
			API.ClearLocalWiki();
			task = refreshTask = API.RequestRulesetListing().ContinueWith( t => {
				Schedule( () => {
					if ( task != refreshTask )
						return;

					Dictionary<string, ListingEntry> entries = new();
					foreach ( var i in t.Result ) {
						var next = new DrawableListingEntry( i ) {
							Anchor = Anchor.TopCentre,
							Origin = Anchor.TopCentre
						};
						content.Add( next );

						if ( !string.IsNullOrWhiteSpace( i.Download ) )
							entries.Add( System.IO.Path.GetFileName( i.Download ), i );
					}

					foreach ( var i in DownloadManager.InstalledRulesets ) {
						if ( !entries.ContainsKey( System.IO.Path.GetFileName( i.CreateInstance()?.GetType().Assembly.Location ) ) ) {
							var local = API.CreateLocalEntry( i.ShortName, i.Name );
							local.ListingEntry.LocalRulesetInfo = i;
							local.ListingEntry.IsLocal = true;

							API.InjectLocalRuleset( local );
							
							var next = new DrawableListingEntry( local.ListingEntry ) {
								Anchor = Anchor.TopCentre,
								Origin = Anchor.TopCentre
							};
							content.Add( next );
						}
					}

					foreach ( var i in DownloadManager.UnimportedRulesets ) {
						if ( entries.TryGetValue( i.filename, out var e ) ) {
							e.FaliedImport = true;
						}
						else {
							var name = i.shortname;
							var local = API.CreateLocalEntry( name.ToLower(), name.Humanize() );
							local.ListingEntry.FaliedImport = true;
							local.ListingEntry.IsLocal = true;

							API.InjectLocalRuleset( local );

							var next = new DrawableListingEntry( local.ListingEntry ) {
								Anchor = Anchor.TopCentre,
								Origin = Anchor.TopCentre
							};
							content.Add( next );
						}
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

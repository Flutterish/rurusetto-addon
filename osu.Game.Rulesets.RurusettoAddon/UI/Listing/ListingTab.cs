using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.RurusettoAddon.API;
using System;
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
			task = refreshTask = API.RequestRulesetListing().ContinueWith( t => {
				Schedule( () => {
					if ( task != refreshTask )
						return;

					HashSet<string> entries = new();
					foreach ( var i in t.Result ) {
						var next = new DrawableListingEntry( i ) {
							Anchor = Anchor.TopCentre,
							Origin = Anchor.TopCentre
						};
						content.Add( next );

						entries.Add( System.IO.Path.GetFileName( i.Download ) );
					}

					foreach ( var i in DownloadManager.InstalledRulesets ) {
						if ( !entries.Contains( System.IO.Path.GetFileName( i.CreateInstance()?.GetType().Assembly.Location ) ) ) {
							var local = new LocalRulesetWikiEntry() {
								ListingEntry = new() {
									ShortName = i.ShortName,
									Name = i.Name,
									Description = "Local ruleset, not listed on the wiki.",
									CanDownload = false,
									Owner = new UserDetail(),
									LocalRulesetInfo = i
								},
								Detail = new RulesetDetail {
									CanDownload = false,
									Content = "Local ruleset, not listed on the wiki.",
									CreatedAt = DateTime.Now,
									Creator = new(),
									Description = "Local ruleset, not listed on the wiki.",
									LastEditedAt = DateTime.Now,
									LastEditedBy = new(),
									Name = i.Name,
									ShortName = i.ShortName,
									CoverDark = StaticAPIResource.DefaultCover.GetURI(),
									CoverLight = StaticAPIResource.DefaultCover.GetURI(),
									Owner = new()
								}
							};

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

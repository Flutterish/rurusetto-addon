using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.RurusettoAddon.API;
using System;

namespace osu.Game.Rulesets.RurusettoAddon.UI {
	public class RulesetDownloadButton : GrayButton, IHasContextMenu {
		[Resolved]
		public RulesetDownloadManager DownloadManager { get; private set; }

		public readonly Bindable<DownloadState> State = new();

		LoadingSpinner spinner;
		ListingEntry entry;
		Warning warning;
		public RulesetDownloadButton ( ListingEntry entry ) : base( FontAwesome.Solid.Download ) {
			this.entry = entry;
			Action = onClick;
		}

		protected override void LoadComplete () {
			base.LoadComplete();

			Icon.Colour = Colour4.White;
			Icon.Scale = new osuTK.Vector2( 1.5f );
			Background.Colour = Colour4.FromHex( "#394642" );

			Add( spinner = new LoadingSpinner {
				Scale = new osuTK.Vector2( 0.45f )
			} );

			AddInternal( warning = new Warning {
				Height = 16,
				Width = 16,
				Alpha = 0,
				X = -7,
				Y = -7
			} );

			DownloadManager.BindWith( entry.ShortName, State );
			State.BindValueChanged( v => Schedule( () => {
				switch ( v.NewValue ) {
					case DownloadState.Unknown:
						this.FadeTo( 0.6f, 200 );
						TooltipText = "Checking...";
						warning.FadeOut( 200 );
						break;

					case DownloadState.NotAvailableOnline:
						spinner.Alpha = 0;
						Icon.Alpha = 1;
						this.FadeTo( 0.6f, 200 );
						TooltipText = "Unavailable Online";
						warning.FadeOut( 200 );
						Icon.Scale = new osuTK.Vector2( 1.5f );
						Icon.Icon = FontAwesome.Solid.Download;
						break;

					case DownloadState.AvailableOnline:
						spinner.Alpha = 0;
						Icon.Alpha = 1;
						this.FadeTo( 1f, 200 );
						Background.FadeColour( Colour4.FromHex( "#394642" ), 200, Easing.InOutExpo );
						Icon.Scale = new osuTK.Vector2( 1.5f );
						Icon.Icon = FontAwesome.Solid.Download;
						TooltipText = "Download";
						warning.FadeOut( 200 );
						break;

					case DownloadState.ToBeRemoved:
					case DownloadState.ToBeImported:
					case DownloadState.OutdatedAvailableLocally:
					case DownloadState.AvailableLocally:
						spinner.Alpha = 0;
						Icon.Alpha = 1;
						this.FadeTo( 1f, 200 );
						Background.FadeColour( Colour4.FromHex( "#6CB946" ), 200, Easing.InOutExpo );
						Icon.Scale = new osuTK.Vector2( 1.7f );
						Icon.Icon = FontAwesome.Regular.CheckCircle;
						TooltipText = "Installed";
						if ( v.NewValue == DownloadState.AvailableLocally ) {
							warning.FadeOut( 200 );
						}
						else {
							warning.FadeIn( 200 );
							warning.TooltipText = v.NewValue switch {
								DownloadState.ToBeRemoved => "Will be removed on restart!",
								DownloadState.ToBeImported => "Will be installed on restart!",
								_ => "Outdated"
							};
						}
						break;

					case DownloadState.Downloading:
						Icon.Alpha = 0;
						spinner.Alpha = 1;
						this.FadeTo( 1f, 200 );
						Background.FadeColour( Colour4.FromHex( "#6291D7" ), 200, Easing.InOutExpo );

						TooltipText = "Downloading...";
						warning.FadeOut( 200 );
						break;
				}
			} ), true );

			OsuMenuItem download = new( "Download", MenuItemType.Standard, () => DownloadManager.DownloadRuleset( entry.ShortName ) );
			OsuMenuItem update = new( "Update", MenuItemType.Standard, () => DownloadManager.UpdateRuleset( entry.ShortName ) );
			OsuMenuItem remove = new( "Remove", MenuItemType.Destructive, () => DownloadManager.RemoveRuleset( entry.ShortName ) );
			OsuMenuItem cancelDownload = new( "Cancel Download", MenuItemType.Standard, () => DownloadManager.CancelRulesetDownload( entry.ShortName ) );
			OsuMenuItem cancelRemoval = new( "Cancel Removal", MenuItemType.Standard, () => DownloadManager.CancelRulesetRemoval( entry.ShortName ) );
			OsuMenuItem refresh = new( "Refresh", MenuItemType.Standard, () => DownloadManager.CheckAvailability( entry.ShortName ) );

			State.BindValueChanged( v => Schedule( () => {
				ContextMenuItems = v.NewValue switch {
					DownloadState.AvailableOnline => new MenuItem[] { refresh, download },
					DownloadState.AvailableLocally => new MenuItem[] { refresh, remove },
					DownloadState.OutdatedAvailableLocally => new MenuItem[] { refresh, update, remove },
					DownloadState.Downloading => new MenuItem[] { cancelDownload },
					DownloadState.ToBeImported => new MenuItem[] { refresh, remove },
					DownloadState.ToBeRemoved => new MenuItem[] { refresh, cancelRemoval },
					DownloadState.NotAvailableOnline => new MenuItem[] { refresh },
					_ or DownloadState.Unknown => Array.Empty<MenuItem>()
				};
			} ), true );

			Schedule( () => FinishTransforms( true ) );
		}

		[Resolved]
		private IBindable<RulesetInfo> currentRuleset { get; set; }
		
		void onClick () {
			if ( State.Value == DownloadState.AvailableLocally && currentRuleset is Bindable<RulesetInfo> current ) {
				current.Value = DownloadManager.GetLocalRuleset( entry.ShortName, entry.Name, "" );
			}
			else if ( State.Value == DownloadState.AvailableOnline ) {
				DownloadManager.DownloadRuleset( entry.ShortName );
			}
			else if ( State.Value == DownloadState.OutdatedAvailableLocally ) {
				DownloadManager.UpdateRuleset( entry.ShortName );
			}
		}

		public MenuItem[] ContextMenuItems { get; private set; }

		private class Warning : CircularContainer, IHasTooltip {
			public Warning () {
				Add( new Box {
					RelativeSizeAxes = Axes.Both,
					Colour = Colour4.FromHex( "#FF6060" )
				} );
				Add( new Circle {
					Origin = Anchor.Centre,
					Anchor = Anchor.Centre,
					RelativeSizeAxes = Axes.Both,
					Size = new osuTK.Vector2( 0.55f ),
					Colour = Colour4.White
				} );

				Masking = true;
			}
				
			public LocalisableString TooltipText { get; set; }
		}
	}
}

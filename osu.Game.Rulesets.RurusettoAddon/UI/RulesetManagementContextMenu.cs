﻿using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.RurusettoAddon.UI.Menus;

namespace osu.Game.Rulesets.RurusettoAddon.UI;

public class RulesetManagementContextMenu : CompositeDrawable, IHasContextMenu {
	[Resolved]
	public RulesetDownloader Downloader { get; private set; } = null!;

	public readonly Bindable<DownloadState> State = new( DownloadState.NotDownloading );
	public readonly Bindable<Availability> Avail = new( Availability.Unknown );

	APIRuleset ruleset;
	LocalisableOsuMenuItem download;
	LocalisableOsuMenuItem update;
	LocalisableOsuMenuItem redownload;
	LocalisableOsuMenuItem remove;
	LocalisableOsuMenuItem cancelDownload;
	LocalisableOsuMenuItem cancelUpdate;
	LocalisableOsuMenuItem cancelRemoval;
	LocalisableOsuMenuItem refresh;

	public RulesetManagementContextMenu ( APIRuleset ruleset ) {
		this.ruleset = ruleset;

		RelativeSizeAxes = Axes.Both;

		download = new( Localisation.Strings.Download, MenuItemType.Standard, () => Downloader.DownloadRuleset( ruleset ) );
		update = new( Localisation.Strings.Update, MenuItemType.Standard, () => Downloader.UpdateRuleset( ruleset ) );
		redownload = new( Localisation.Strings.Redownload, MenuItemType.Standard, () => Downloader.UpdateRuleset( ruleset ) );
		remove = new( Localisation.Strings.Remove, MenuItemType.Destructive, () => Downloader.RemoveRuleset( ruleset ) );
		cancelDownload = new( Localisation.Strings.CancelDownload, MenuItemType.Standard, () => Downloader.CancelRulesetDownload( ruleset ) );
		cancelUpdate = new( Localisation.Strings.CancelUpdate, MenuItemType.Standard, () => Downloader.CancelRulesetDownload( ruleset ) );
		cancelRemoval = new( Localisation.Strings.CancelRemove, MenuItemType.Standard, () => Downloader.CancelRulesetRemoval( ruleset ) );
		refresh = new( Localisation.Strings.Refresh, MenuItemType.Standard, () => Downloader.CheckAvailability( ruleset ) );
	}

	protected override void LoadComplete () {
		base.LoadComplete();

		Downloader.BindWith( ruleset, State );
		Downloader.BindWith( ruleset, Avail );

		State.ValueChanged += _ => Schedule( updateContextMenu );
		Avail.ValueChanged += _ => Schedule( updateContextMenu );

		updateContextMenu();
	}

	private void updateContextMenu () {
		if ( Avail.Value == Availability.Unknown ) {
			ContextMenuItems = Array.Empty<MenuItem>();
			return;
		}

		if ( !ruleset.IsModifiable ) {
			ContextMenuItems = Array.Empty<MenuItem>();
			return;
		}

		if ( State.Value == DownloadState.Downloading ) {
			ContextMenuItems = Avail.Value.HasFlagFast( Availability.AvailableLocally )
				? new MenuItem[] { cancelUpdate }
				: new MenuItem[] { cancelDownload };
		}
		else if ( State.Value is DownloadState.ToBeImported or DownloadState.ToBeRemoved || Avail.Value.HasFlagFast( Availability.AvailableLocally ) ) {
			if ( State.Value == DownloadState.ToBeImported ) {
				ContextMenuItems = Avail.Value.HasFlagFast( Availability.AvailableLocally )
					? new MenuItem[] { refresh, cancelUpdate }
					: new MenuItem[] { refresh, remove };
			}
			else if ( State.Value == DownloadState.ToBeRemoved ) {
				ContextMenuItems = new MenuItem[] { refresh, cancelRemoval };
			}
			else if ( Avail.Value.HasFlagFast( Availability.Outdated ) ) {
				ContextMenuItems = Avail.Value.HasFlagFast( Availability.AvailableOnline )
					? new MenuItem[] { refresh, update, remove }
					: new MenuItem[] { refresh, remove };
			}
			else {
				ContextMenuItems = Avail.Value.HasFlagFast( Availability.AvailableOnline )
					? new MenuItem[] { refresh, redownload, remove }
					: new MenuItem[] { refresh, remove };
			}
		}
		else if ( Avail.Value.HasFlagFast( Availability.NotAvailableOnline ) ) {
			ContextMenuItems = new MenuItem[] { refresh };
		}
		else if ( Avail.Value.HasFlagFast( Availability.AvailableOnline ) ) {
			ContextMenuItems = new MenuItem[] { refresh, download };
		}
		else {
			ContextMenuItems = Array.Empty<MenuItem>();
		}
	}

	public MenuItem[] ContextMenuItems { get; private set; } = Array.Empty<MenuItem>();
}
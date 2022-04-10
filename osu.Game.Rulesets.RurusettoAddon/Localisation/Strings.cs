using osu.Framework.Localisation;

namespace osu.Game.Rulesets.RurusettoAddon.Localisation {
	public static class Strings {
		private const string prefix = "osu.Game.Rulesets.RurusettoAddon.Localisation.Strings";
		
		/// <summary>
		/// Local ruleset, not listed on the wiki.
		/// </summary>
		public static LocalisableString LocalRulesetDescription => new TranslatableString( getKey( "local-ruleset-description" ), "Local ruleset, not listed on the wiki." );
		
		/// <summary>
		/// Failed to fetch the ruleset wiki page. Sorry!
		/// You can still try visiting it at [rurusetto]({0}).
		/// </summary>
		public static LocalisableString PageLoadFailed ( LocalisableString wikiLink )
			=> new TranslatableString( getKey( "page-load-failed" ), "Failed to fetch the ruleset wiki page. Sorry!\nYou can still try visiting it at [rurusetto]({0}).", wikiLink );
		
		/// <summary>
		/// listing
		/// </summary>
		public static LocalisableString ListingTab => new TranslatableString( getKey( "listing-tab" ), "listing" );
		
		/// <summary>
		/// users
		/// </summary>
		public static LocalisableString UsersTab => new TranslatableString( getKey( "users-tab" ), "users" );
		
		/// <summary>
		/// collections
		/// </summary>
		public static LocalisableString CollectionsTab => new TranslatableString( getKey( "collections-tab" ), "collections" );
		
		/// <summary>
		/// browse and manage rulesets
		/// </summary>
		public static LocalisableString RurusettoDescription => new TranslatableString( getKey( "rurusetto-description" ), "browse and manage rulesets" );
		
		/// <summary>
		/// Unknown
		/// </summary>
		public static LocalisableString UserUnknown => new TranslatableString( getKey( "user-unknown" ), "Unknown" );
		
		/// <summary>
		/// ARCHIVED
		/// </summary>
		public static LocalisableString TagArchived => new TranslatableString( getKey( "tag-archived" ), "ARCHIVED" );
		
		/// <summary>
		/// This ruleset is no longer maintained
		/// </summary>
		public static LocalisableString TagArchivedTooltip => new TranslatableString( getKey( "tag-archived-tooltip" ), "This ruleset is no longer maintained" );
		
		/// <summary>
		/// UNLISTED
		/// </summary>
		public static LocalisableString TagLocal => new TranslatableString( getKey( "tag-local" ), "UNLISTED" );
		
		/// <summary>
		/// This ruleset is installed locally, but is not listed on the wiki
		/// </summary>
		public static LocalisableString TagLocalTooltip => new TranslatableString( getKey( "tag-local-tooltip" ), "This ruleset is installed locally, but is not listed on the wiki" );
		
		/// <summary>
		/// HARD CODED
		/// </summary>
		public static LocalisableString TagHardcoded => new TranslatableString( getKey( "tag-hardcoded" ), "HARD CODED" );
		
		/// <summary>
		/// This ruleset is hard coded into the game and cannot be modified
		/// </summary>
		public static LocalisableString TagHardcodedTooltip => new TranslatableString( getKey( "tag-hardcoded-tooltip" ), "This ruleset is hard coded into the game and cannot be modified" );
		
		/// <summary>
		/// FAILED IMPORT
		/// </summary>
		public static LocalisableString TagFailedImport => new TranslatableString( getKey( "tag-failed-import" ), "FAILED IMPORT" );
		
		/// <summary>
		/// This ruleset is downloaded, but failed to import
		/// </summary>
		public static LocalisableString TagFailedImportTooltip => new TranslatableString( getKey( "tag-failed-import-tooltip" ), "This ruleset is downloaded, but failed to import" );
		
		/// <summary>
		/// BORKED
		/// </summary>
		public static LocalisableString TagBorked => new TranslatableString( getKey( "tag-borked" ), "BORKED" );
		
		/// <summary>
		/// This ruleset does not work
		/// </summary>
		public static LocalisableString TagBorkedTooltip => new TranslatableString( getKey( "tag-borked-tooltip" ), "This ruleset does not work" );
		
		/// <summary>
		/// PLAYABLE
		/// </summary>
		public static LocalisableString TagPlayable => new TranslatableString( getKey( "tag-playable" ), "PLAYABLE" );
		
		/// <summary>
		/// This ruleset works
		/// </summary>
		public static LocalisableString TagPlayableTooltip => new TranslatableString( getKey( "tag-playable-tooltip" ), "This ruleset works" );
		
		/// <summary>
		/// PRE-RELEASE
		/// </summary>
		public static LocalisableString TagPrerelease => new TranslatableString( getKey( "tag-prerelease" ), "PRE-RELEASE" );
		
		/// <summary>
		/// The current version is a pre-release
		/// </summary>
		public static LocalisableString TagPrereleaseTooltip => new TranslatableString( getKey( "tag-prerelease-tooltip" ), "The current version is a pre-release" );
		
		/// <summary>
		/// Home Page
		/// </summary>
		public static LocalisableString HomePage => new TranslatableString( getKey( "home-page" ), "Home Page" );
		
		/// <summary>
		/// Report Issue
		/// </summary>
		public static LocalisableString ReportIssue => new TranslatableString( getKey( "report-issue" ), "Report Issue" );
		
		/// <summary>
		/// Checking...
		/// </summary>
		public static LocalisableString DownloadChecking => new TranslatableString( getKey( "download-checking" ), "Checking..." );
		
		/// <summary>
		/// Unavailable Online
		/// </summary>
		public static LocalisableString UnavailableOnline => new TranslatableString( getKey( "unavailable-online" ), "Unavailable Online" );
		
		/// <summary>
		/// Installed, not available online
		/// </summary>
		public static LocalisableString InstalledUnavailableOnline => new TranslatableString( getKey( "installed-unavailable-online" ), "Installed, not available online" );
		
		/// <summary>
		/// Download
		/// </summary>
		public static LocalisableString Download => new TranslatableString( getKey( "download" ), "Download" );
		
		/// <summary>
		/// Re-download
		/// </summary>
		public static LocalisableString Redownload => new TranslatableString( getKey( "redownload" ), "Re-download" );
		
		/// <summary>
		/// Downloading...
		/// </summary>
		public static LocalisableString Downloading => new TranslatableString( getKey( "downloading" ), "Downloading..." );
		
		/// <summary>
		/// Update
		/// </summary>
		public static LocalisableString Update => new TranslatableString( getKey( "update" ), "Update" );
		
		/// <summary>
		/// Remove
		/// </summary>
		public static LocalisableString Remove => new TranslatableString( getKey( "remove" ), "Remove" );
		
		/// <summary>
		/// Cancel Download
		/// </summary>
		public static LocalisableString CancelDownload => new TranslatableString( getKey( "cancel-download" ), "Cancel Download" );
		
		/// <summary>
		/// Cancel Removal
		/// </summary>
		public static LocalisableString CancelRemove => new TranslatableString( getKey( "cancel-remove" ), "Cancel Removal" );
		
		/// <summary>
		/// Cancel Update
		/// </summary>
		public static LocalisableString CancelUpdate => new TranslatableString( getKey( "cancel-update" ), "Cancel Update" );
		
		/// <summary>
		/// Refresh
		/// </summary>
		public static LocalisableString Refresh => new TranslatableString( getKey( "refresh" ), "Refresh" );
		
		/// <summary>
		/// Will be removed on restart!
		/// </summary>
		public static LocalisableString ToBeRemoved => new TranslatableString( getKey( "to-be-removed" ), "Will be removed on restart!" );
		
		/// <summary>
		/// Will be installed on restart!
		/// </summary>
		public static LocalisableString ToBeInstalled => new TranslatableString( getKey( "to-be-installed" ), "Will be installed on restart!" );
		
		/// <summary>
		/// Will be updated on restart!
		/// </summary>
		public static LocalisableString ToBeUpdated => new TranslatableString( getKey( "to-be-updated" ), "Will be updated on restart!" );
		
		/// <summary>
		/// Installed
		/// </summary>
		public static LocalisableString Installed => new TranslatableString( getKey( "installed" ), "Installed" );
		
		/// <summary>
		/// Outdated
		/// </summary>
		public static LocalisableString Outdated => new TranslatableString( getKey( "outdated" ), "Outdated" );
		
		/// <summary>
		/// Verified Ruleset Creator
		/// </summary>
		public static LocalisableString CreatorVerified => new TranslatableString( getKey( "creator-verified" ), "Verified Ruleset Creator" );
		
		/// <summary>
		/// Could not load rurusetto-addon: Please report this to the rurusetto-addon repository NOT the osu!lazer repository: Code {0}
		/// </summary>
		public static LocalisableString LoadError ( LocalisableString errorCode )
			=> new TranslatableString( getKey( "load-error" ), "Could not load rurusetto-addon: Please report this to the rurusetto-addon repository NOT the osu!lazer repository: Code {0}", errorCode );
		
		/// <summary>
		/// Main
		/// </summary>
		public static LocalisableString MainPage => new TranslatableString( getKey( "main-page" ), "Main" );
		
		/// <summary>
		/// Changelog
		/// </summary>
		public static LocalisableString ChangelogPage => new TranslatableString( getKey( "changelog-page" ), "Changelog" );
		
		/// <summary>
		/// Unknown Version
		/// </summary>
		public static LocalisableString UnknownVersion => new TranslatableString( getKey( "unknown-version" ), "Unknown Version" );
		
		/// <summary>
		/// Rurusetto Addon
		/// </summary>
		public static LocalisableString SettingsHeader => new TranslatableString( getKey( "settings-header" ), "Rurusetto Addon" );
		
		/// <summary>
		/// API Address
		/// </summary>
		public static LocalisableString SettingsApiAddress => new TranslatableString( getKey( "settings-api-address" ), "API Address" );
		
		/// <summary>
		/// Untitled Ruleset
		/// </summary>
		public static LocalisableString UntitledRuleset => new TranslatableString( getKey( "untitled-ruleset" ), "Untitled Ruleset" );
		
		/// <summary>
		/// Oh no!
		/// </summary>
		public static LocalisableString ErrorHeader => new TranslatableString( getKey( "error-header" ), "Oh no!" );
		
		/// <summary>
		/// Please make sure you have an internet connection and the API address in settings is correct
		/// </summary>
		public static LocalisableString ErrorFooter => new TranslatableString( getKey( "error-footer" ), "Please make sure you have an internet connection and the API address in settings is correct" );
		
		/// <summary>
		/// Retry
		/// </summary>
		public static LocalisableString Retry => new TranslatableString( getKey( "retry" ), "Retry" );
		
		/// <summary>
		/// Could not retrieve the ruleset listing
		/// </summary>
		public static LocalisableString ListingFetchError => new TranslatableString( getKey( "listing-fetch-error" ), "Could not retrieve the ruleset listing" );
		
		/// <summary>
		/// Could not load the page
		/// </summary>
		public static LocalisableString PageFetchError => new TranslatableString( getKey( "page-fetch-error" ), "Could not load the page" );
		
		/// <summary>
		/// Could not retrieve subpages
		/// </summary>
		public static LocalisableString SubpagesFetchError => new TranslatableString( getKey( "subpages-fetch-error" ), "Could not retrieve subpages" );
		
		/// <summary>
		/// Something went wrong, but I don't know what!
		/// </summary>
		public static LocalisableString ErrorMessageGeneric => new TranslatableString( getKey( "error-message-generic" ), "Something went wrong, but I don't know what!" );
		
		/// <summary>
		/// It seems rurusetto addon couldn't finish some work. Please make sure all your changes were applied correctly
		/// </summary>
		public static LocalisableString NotificationWorkIncomplete => new TranslatableString( getKey( "notification-work-incomplete" ), "It seems rurusetto addon couldn't finish some work. Please make sure all your changes were applied correctly" );
		
		private static string getKey ( string key ) => $"{prefix}:{key}";
	}
}

﻿using Newtonsoft.Json;

namespace osu.Game.Rulesets.RurusettoAddon.API {
	public record ListingEntry {
		/// <summary> The ID of the ruleset in Rūrusetto database. </summary>
		[JsonProperty( "id" )]
		public int ID;

		/// <summary> The name of the ruleset. </summary>
		[JsonProperty( "name" )]
		public string Name;
		/// <summary> The slug of the ruleset. Use in the URL of the ruleset's wiki page. </summary>
		[JsonProperty( "slug" )]
		public string ShortName;
		/// <summary> The short description of the rulesets. </summary>
		[JsonProperty( "description" )]
		public string Description;

		/// <summary> The URL of the ruleset icon that use in website's default theme (dark theme). </summary>
		[JsonProperty( "icon" )]
		public string DarkIcon;
		/// <summary>The URL of the ruleset icon that use in website's light theme. </summary>
		[JsonProperty( "light_icon" )]
		public string LightIcon;

		/// <summary> The user_detail of the ruleset's current owner. </summary>
		[JsonProperty( "owner_detail" )]
		public UserDetail Owner;

		/// <summary> True if the wiki maintainer has verified that the the owner is the real owner of this ruleset. </summary>
		[JsonProperty( "verified" )]
		public bool IsVerified;
		/// <summary> URL for download the latest release of ruleset from GitHub </summary>
		[JsonProperty( "direct_download_link" )]
		public string Download;
		/// <summary>
		/// True if website can render the direct download link from the source and github_download_filename 
		/// so user can download directly from direct_download_link.
		/// </summary>
		[JsonProperty( "can_download" )]
		public bool CanDownload;

		public RulesetInfo LocalRulesetInfo;
		public bool IsLocal;
		public bool FaliedImport;
	}
}

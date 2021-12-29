using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace osu.Game.Rulesets.RurusettoAddon.API {
	public record RulesetDetail {
		/// <summary>
		/// Whether the request has succeded
		/// </summary>
		public bool Success => string.IsNullOrWhiteSpace( Detail );

		/// <summary> The ID of the ruleset in Rūrusetto database. </summary>
		[JsonProperty( "id" )]
		public int ID { get; init; }

		/// <summary> The name of the ruleset. </summary>
		[JsonProperty( "name" )]
		public string Name { get; init; }
		/// <summary> The slug of the ruleset. Use in the URL of the ruleset's wiki page. </summary>
		[JsonProperty( "slug" )]
		public string Slug { get; init; }
		/// <summary> The short description of the rulesets. </summary>
		[JsonProperty( "description" )]
		public string Description { get; init; }

		/// <summary> The URL of the ruleset icon that use in website's default theme (dark theme). </summary>
		[JsonProperty( "icon" )]
		public string DarkIcon { get; init; }
		/// <summary> The URL of the ruleset icon that use in website's light theme. </summary>
		[JsonProperty( "light_icon" )]
		public string LightIcon { get; init; }
		/// <summary> The URL of the ruleset logo that use in the infobox. </summary>
		[JsonProperty( "logo" )]
		public string Logo { get; init; }
		/// <summary> The URL of the cover image in ruleset's wiki page in website's default theme (dark theme). </summary>
		[JsonProperty( "cover_image" )]
		public string CoverDark { get; init; }
		/// <summary> The URL of the cover image in ruleset's wiki page in website's light theme. </summary>
		[JsonProperty( "cover_image_light" )]
		public string CoverLight { get; init; }
		/// <summary> The URL of the image that use in the opengraph part of the wiki URL. </summary>
		[JsonProperty( "opengraph_image" )]
		public string OpengraphImage { get; init; }

		/// <summary> The URL of the CSS file that's override the website's default styling. </summary>
		[JsonProperty( "custom_css" )]
		public string CustomCSS { get; init; }
		/// <summary> Wiki main content in markdown format. </summary>
		[JsonProperty( "content" )]
		public string Content { get; init; }

		/// <summary> The URL source of the rulesets. </summary>
		[JsonProperty( "source" )]
		public string Source { get; init; }
		/// <summary> Filename that use in rendering the direct download link with the source link. </summary>
		[JsonProperty( "github_download_filename" )]
		public string GithubFilename { get; init; }
		/// <summary> URL for download the latest release of ruleset from GitHub </summary>
		[JsonProperty( "direct_download_link" )]
		public string Download { get; init; }
		/// <summary>
		/// True if website can render the direct download link from the source and github_download_filename 
		/// so user can download directly from direct_download_link.
		/// </summary>
		[JsonProperty( "can_download" )]
		public bool CanDownload { get; init; }

		/// <summary> The user_detail of the ruleset's current owner </summary>
		[JsonProperty( "owner_detail" )]
		public UserDetail Owner { get; init; }

		/// <summary> The user_detail of the user who create this wiki page, not the owner. </summary>
		[JsonProperty( "creator_detail" )]
		public UserDetail Creator { get; init; }
		/// <summary> The UTC time that the wiki page has create in JSON time format. </summary>
		[JsonProperty( "created_at" )]
		public DateTime? CreatedAt { get; init; }

		/// <summary> The user_detail of the user who edit the wiki page last time. </summary>
		[JsonProperty( "last_edited_by_detail" )]
		public UserDetail LastEditedBy { get; init; }
		/// <summary> The UTC time of the latest wiki edit. </summary>
		[JsonProperty( "last_edited_at" )]
		public DateTime? LastEditedAt { get; init; }

		/// <summary> True if the wiki maintainer has verified that the the owner is the real owner of this ruleset. </summary>
		[JsonProperty( "verified" )]
		public bool IsVerified { get; init; }
		/// <summary> True if this ruleset is stop update or archived by rulesets creator. </summary>
		[JsonProperty( "archive" )]
		public bool IsArchived { get; init; }

		[JsonProperty( "detail" )]
		private string Detail { get; init; }
	}
}

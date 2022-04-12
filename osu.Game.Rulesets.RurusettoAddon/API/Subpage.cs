using Newtonsoft.Json;

#nullable disable
namespace osu.Game.Rulesets.RurusettoAddon.API;

public record Subpage {
	/// <summary> Details of ruleset. </summary>
	[JsonProperty( "ruleset_detail" )]
	public RulesetDetail RulesetDetail { get; init; }
	/// <summary> Title of the subpage </summary>
	[JsonProperty( "title" )]
	public string Title { get; init; }
	/// <summary> Slug of the subpage. Use in subpage URL path. </summary>
	[JsonProperty( "slug" )]
	public string Slug { get; init; }
	/// <summary> Content of the subpage in markdown format. </summary>
	[JsonProperty( "content" )]
	public string Content { get; init; }
	/// <summary> user_detail of user who create this page. </summary>
	[JsonProperty( "creator_detail" )]
	public UserDetail Creator { get; init; }
	/// <summary> user_detail of user who last edited this subpage. </summary>
	[JsonProperty( "last_edited_by_detail" )]
	public UserDetail LastEditedBy { get; init; }
	/// <summary> The UTC time of the latest wiki edit in JSON time format. </summary>
	[JsonProperty( "last_edited_at" )]
	public DateTime? LastEditedAt { get; init; }
	/// <summary> The UTC time that the wiki page has create in JSON time format. </summary>
	[JsonProperty( "created_at" )]
	public DateTime? CreatedAt { get; init; }
}
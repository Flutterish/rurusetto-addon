using Newtonsoft.Json;

#nullable disable
namespace osu.Game.Rulesets.RurusettoAddon.API;

public record UserProfile {
	/// <summary> The ID of the user. Use in URL path to target user's profile page. </summary>
	[JsonProperty( "id" )]
	public int? ID { get; init; }

	[JsonProperty( "user" )]
	private UserInfo info { get; init; }

	/// <summary> URL of the user's profile picture. </summary>
	[JsonProperty( "image" )]
	public string ProfilePicture { get; init; }

	/// <summary> URL of the user's cover picture in website's default theme (Dark theme). </summary>
	[JsonProperty( "cover" )]
	public string DarkCover { get; init; }

	/// <summary> URL of the user's cover picture in website's light theme. </summary>
	[JsonProperty( "cover_light" )]
	public string LightCover { get; init; }

	/// <summary> User's introduction text on profile page. </summary>
	[JsonProperty( "about_me" )]
	public string Bio { get; init; }

	/// <summary> osu! account username of target user (Can be blank) </summary>
	[JsonProperty( "osu_username" )]
	public string OsuUsername { get; init; }

	/// <inheritdoc cref="UserInfo.Username"/>
	public string Username => info?.Username;

	/// <inheritdoc cref="UserInfo.Email"/>
	public string Email => info?.Email;

	/// <summary> List of tag that user has. Will be [] if no tags found in this user. </summary>
	[JsonProperty( "tags" )]
	public List<UserTag> Tags { get; init; }

	/// <summary> List of ruleset that user created. Will be [] if no created rulesets found from this user. </summary>
	[JsonProperty( "created_rulesets" )]
	public List<ShortListingEntry> CreatedRulesets { get; init; }
}

public record UserTag {
	/// <summary> The name of the tag. </summary>
	[JsonProperty( "name" )]
	public string Text { get; init; }

	/// <summary> The background color of the tag pills that show in profile. Will return in hex color (e.g. #FFFFFF). </summary>
	[JsonProperty( "pills_color" )]
	public string BackgroundColor { get; init; }

	/// <summary> The font color of the tag pills that show in profile. Will return in hex color (e.g. #FFFFFF). </summary>
	[JsonProperty( "font_color" )]
	public string ForegroundColor { get; init; }

	/// <summary> The description of the tag. </summary>
	[JsonProperty( "description" )]
	public string Description { get; init; }
}
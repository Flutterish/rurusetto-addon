using Newtonsoft.Json;

namespace osu.Game.Rulesets.RurusettoAddon.API {
	public record UserProfile {
		/// <summary> The ID of the user. Use in URL path to target user's profile page. </summary>
		[JsonProperty( "id" )]
		public int ID { get; init; }
		[JsonProperty( "user" )]
		public UserInfo Info { get; init; }
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
	}
}

using Newtonsoft.Json;

namespace osu.Game.Rulesets.RurusettoAddon.API {
	public record UserDetail {
		/// <summary> The ID of the user in Rūrusetto database. </summary>
		[JsonProperty( "id" )]
		public int ID;

		[JsonProperty( "user" )]
		public UserInfo Info;

		/// <summary> The URL of the user's profile image. </summary>
		[JsonProperty( "image" )]
		public string Image;
	}
}

using Newtonsoft.Json;

namespace osu.Game.Rulesets.RurusettoAddon.API {
	public record UserInfo {
		/// <summary> Username of request user. </summary>
		[JsonProperty( "username" )]
		public string Username;

		/// <summary> Email of request user. (Can be blank) </summary>
		[JsonProperty( "email" )]
		public string Email;
	}
}

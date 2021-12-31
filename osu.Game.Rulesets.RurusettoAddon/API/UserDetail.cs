using Newtonsoft.Json;

namespace osu.Game.Rulesets.RurusettoAddon.API {
	public record UserDetail {
		/// <summary> The ID of the user in Rūrusetto database. </summary>
		[JsonProperty( "id" )]
		public int? ID { get; init; }

		[JsonProperty( "user" )]
		private UserInfo info { get; init; }

		/// <inheritdoc cref="UserInfo.Username"/>
		public string Username => info.Username;

		/// <inheritdoc cref="UserInfo.Email"/>
		public string Email => info.Email;

		/// <summary> The URL of the user's profile image. </summary>
		[JsonProperty( "image" )]
		public string ProfilePicture { get; init; }
	}

	public record UserInfo {
		/// <summary> Username of request user. </summary>
		[JsonProperty( "username" )]
		public string Username { get; init; }

		/// <summary> Email of request user. (Can be blank) </summary>
		[JsonProperty( "email" )]
		public string Email { get; init; }
	}
}

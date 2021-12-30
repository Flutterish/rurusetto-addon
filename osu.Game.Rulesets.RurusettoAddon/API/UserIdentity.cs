using osu.Framework.Graphics.Textures;
using System;
using System.Threading.Tasks;

#nullable enable

namespace osu.Game.Rulesets.RurusettoAddon.API {
	public class UserIdentity {
		private UserIdentity () { }
		public static UserIdentity FromID ( RurusettoAPI API, int ID ) {
			return new() {
				Source = Source.Web,
				API = API,
				ID = ID,
				HasProfile = true
			};
		}
		public static UserIdentity Local ( RurusettoAPI API ) {
			return new() {
				Source = Source.Local,
				API = API
			};
		}

		public Source Source { get; private set; }
		private int ID;
		private RurusettoAPI? API;

		public bool HasProfile { get; private set; }

		public async Task<UserProfile> RequestDetail () {
			if ( Source == Source.Web && API != null ) {
				try {
					return await API.RequestUserProfile( ID );
				}
				catch ( Exception ) {
					// TODO report this
				}
			}

			return new();
		}

		public async Task<Texture> RequestProfilePicture () {
			var detail = await RequestDetail();

			if ( Source == Source.Web && API != null ) {
				if ( !string.IsNullOrWhiteSpace( detail.ProfilePicture ) ) {
					try {
						return await API.RequestImage( detail.ProfilePicture );
					}
					catch ( Exception ) {
						// TODO report this
					}
				}
			}

			if ( API != null ) {
				try {
					return await API.RequestImage( StaticAPIResource.DefaultProfileImage );
				}
				catch ( Exception ) {
					// TODO report this
				}
			}

			return Texture.WhitePixel;
		}
	}
}

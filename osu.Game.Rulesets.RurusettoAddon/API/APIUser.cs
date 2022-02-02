using osu.Framework.Graphics.Textures;
using System;

#nullable enable

namespace osu.Game.Rulesets.RurusettoAddon.API {
	public class APIUser {
		private APIUser () { }
		public static APIUser FromID ( RurusettoAPI API, int ID ) {
			return new() {
				Source = Source.Web,
				API = API,
				ID = ID,
				HasProfile = true
			};
		}
		public static APIUser Local ( RurusettoAPI API ) {
			return new() {
				Source = Source.Local,
				API = API
			};
		}

		public Source Source { get; private set; }
		private int ID;
		private RurusettoAPI? API;

		public bool HasProfile { get; private set; }

		public void RequestDetail ( Action<UserProfile> success, Action<Exception?>? failure = null ) {
			if ( Source == Source.Web && API != null ) {
				API.RequestUserProfile( ID, success, failure );
			}
			else {
				success( new() );
			}
		}

		public void RequestProfilePicture ( Action<Texture> success, Action<Exception?>? failure = null ) {
			void requestDefault ( Exception? e = null ) {
				if ( API != null ) {
					API.RequestImage( StaticAPIResource.DefaultProfileImage, success, failure: e => {
						failure?.Invoke( e );
					} );
				}
			}

			if ( Source == Source.Web && API != null ) {
				RequestDetail( detail => {
					if ( !string.IsNullOrWhiteSpace( detail.ProfilePicture ) ) {
						API.RequestImage( detail.ProfilePicture, success, requestDefault );
					}
					else {
						requestDefault();
					}
				}, failure: requestDefault );
			}
			else {
				requestDefault();
			}
		}

		public override string ToString ()
			=> Source is Source.Web ? $"User with ID = {ID}" : $"Local user";
	}
}

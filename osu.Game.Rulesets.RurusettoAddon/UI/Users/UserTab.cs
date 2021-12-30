using osu.Game.Rulesets.RurusettoAddon.API;

namespace osu.Game.Rulesets.RurusettoAddon.UI.Users {
	public class UserTab : OverlayTab {
		public readonly UserIdentity User;
		public UserTab ( UserIdentity user ) {
			User = user;
		}

		protected override bool RequiresLoading => true;
		protected override void LoadContent () {
			Add( new DrawableRurusettoUser( User, false ) { Height = 80 } );

			User.RequestDetail().ContinueWith( t => Schedule( () => {
				var user = t.Result;

				
				OnContentLoaded();
			} ) );
		}
	}
}

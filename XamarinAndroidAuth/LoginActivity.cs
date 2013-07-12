using System;
using System.Json;
using System.Threading.Tasks;
using Android.App;
using Android.Widget;
using Android.OS;
using Xamarin.Auth;

namespace XamarinAndroidAuth
{
	[Activity (Label = "LoginActivity", MainLauncher = true)]			
	public class LoginActivity : Activity
	{
		private TextView _facebookStatusView;
		private TextView _emailView;
		private TextView _genderView;
		private readonly TaskScheduler uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Login);
			_facebookStatusView = FindViewById<TextView>(Resource.Id.facebookStatus);
			_emailView = FindViewById<TextView> (Resource.Id.email);
			_genderView = FindViewById<TextView> (Resource.Id.gender);
			var loginButton = FindViewById<Button> (Resource.Id.login);
			loginButton.Click += LoginToFacebook;
		}

		void LoginToFacebook (object sender, EventArgs e)
		{
			var auth = new OAuth2Authenticator (
				clientId: "597442676937621",
				scope: "email,user_about_me",
				authorizeUrl: new Uri ("https://m.facebook.com/dialog/oauth/"),
				redirectUrl: new Uri ("http://www.facebook.com/connect/login_success.html"));

			// If authorization succeeds or is canceled, .Completed will be fired.
			auth.Completed += (s, ee) => {
				if (!ee.IsAuthenticated) {
					_facebookStatusView.Text = "Not Authenticated";
					return;
				}

				// Now that we're logged in, make a OAuth2 request to get the user's info.
				var request = new OAuth2Request ("GET", new Uri ("https://graph.facebook.com/me"), null, ee.Account);
				request.GetResponseAsync().ContinueWith (t => {
					if (t.IsFaulted)
						_facebookStatusView.Text = "Error: " + t.Exception.InnerException.Message;
					else if (t.IsCanceled)
						_facebookStatusView.Text = "Canceled";
					else
					{
						var obj = JsonValue.Parse (t.Result.GetResponseText());
						_facebookStatusView.Text = "Logged in as " + obj["name"];
						_emailView.Text = obj["email"];
						_genderView.Text = obj["gender"];
					}
				}, uiScheduler);
			};

			var intent = auth.GetUI (this);
			StartActivityForResult (intent, 42);
		}
	}
}


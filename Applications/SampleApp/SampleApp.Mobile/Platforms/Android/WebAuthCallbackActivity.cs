using Android.App;
using Android.Content;
using Microsoft.Maui.Authentication;

namespace SampleApp.Mobile.Platforms.Android
{
    [Activity(Exported = true)]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "com.mps.sampleappmobile", DataPath = "/oauth2redirect")]
    public class WebAuthCallbackActivity : WebAuthenticatorCallbackActivity
    {
        // Empty - inherits behavior from WebAuthenticatorCallbackActivity
    }
}

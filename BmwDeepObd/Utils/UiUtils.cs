using Android.Content;
using Android.Content.Res;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.Lifecycle;
using Skydoves.BalloonLib;

namespace BmwDeepObd
{
    public static class UiUtils
    {
        public static Android.Graphics.Color GetStyleColor(Context context, int attribute)
        {
            TypedArray typedArray = context.Theme.ObtainStyledAttributes([attribute]);
            return typedArray.GetColor(0, 0xFFFFFF);
        }

        public static Balloon.Builder GetBalloonBuilder(Context context)
        {
            Balloon.Builder balloonBuilder = new Balloon.Builder(context);
            balloonBuilder.SetDismissWhenClicked(true);
            balloonBuilder.SetDismissWhenTouchOutside(true);
            balloonBuilder.SetBackgroundColor(GetStyleColor(context, Resource.Attribute.balloonBackgroundColor));
            balloonBuilder.SetTextColor(GetStyleColor(context, Resource.Attribute.balloonTextColor));
            balloonBuilder.SetTextSize(14.0f);
            balloonBuilder.SetTextTypeface((int)Android.Graphics.TypefaceStyle.Italic);
            balloonBuilder.SetBalloonAnimation(BalloonAnimation.Elastic);
            balloonBuilder.SetAutoDismissDuration(ActivityCommon.BalloonDismissDuration);
            balloonBuilder.SetPadding(10);
            balloonBuilder.SetArrowOrientation(ArrowOrientation.Bottom);
            balloonBuilder.SetArrowPosition(0.5f);
            balloonBuilder.SetArrowOrientationRules(ArrowOrientationRules.AlignFixed);
            balloonBuilder.SetLifecycleOwner(ProcessLifecycleOwner.Get());

            return balloonBuilder;
        }

        public static bool ShowAlertDialogBalloon(Context context, AlertDialog alertDialog, int resId, int dismissDuration = ActivityCommon.BalloonDismissDuration)
        {
            View decorView = alertDialog.Window?.DecorView;
            if (decorView != null)
            {
                Balloon.Builder balloonBuilder = GetBalloonBuilder(context);
                balloonBuilder.SetText(context.GetString(resId));
                balloonBuilder.SetAutoDismissDuration(dismissDuration);
                Balloon balloon = balloonBuilder.Build();
                balloon.ShowAlignTop(decorView);
                return true;
            }

            return false;
        }

        public static bool ShowAlertDialogBalloon(Context context, AlertDialog alertDialog, string text, int dismissDuration = ActivityCommon.BalloonDismissDuration)
        {
            View decorView = alertDialog.Window?.DecorView;
            if (decorView != null)
            {
                Balloon.Builder balloonBuilder = GetBalloonBuilder(context);
                balloonBuilder.SetText(text);
                balloonBuilder.SetAutoDismissDuration(dismissDuration);
                Balloon balloon = balloonBuilder.Build();
                balloon.ShowAlignTop(decorView);
                return true;
            }

            return false;
        }
    }
}

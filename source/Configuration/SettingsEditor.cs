using System.Linq;
using Bubbles.Core;
using UnityEngine;
using Verse;

namespace Bubbles.Configuration;

public static class SettingsEditor
{
  private static string?[] _colorBuffer = new string[4];

  private static Vector2 _scrollPosition = Vector2.zero;
  private static Rect _viewRect;

  public static void DrawSettings(Rect rect)
  {
    var headerRect = new Rect(rect.x, rect.y, rect.width, Widgets.BackButtonHeight);
    if (Widgets.ButtonText(headerRect, "Bubbles.ResetToDefault".TranslateSimple())) { Reset(); }
    headerRect.height += Listing.DefaultGap;
    var listingRect = new Rect(rect.x, headerRect.yMax, rect.width, rect.height - headerRect.height);

    if (_viewRect == default) { _viewRect = new Rect(0f, 0f, listingRect.width - GenUI.ScrollBarWidth - GenUI.GapTiny, 99999f); }
    Widgets.BeginScrollView(listingRect, ref _scrollPosition, _viewRect);

    var l = new Listing_Settings();
    l.Begin(_viewRect);

    var doTextColors = Settings.DoTextColors.Value;
    l.CheckboxLabeled("Bubbles.DoTextColors".TranslateSimple(), ref Settings.DoTextColors.Value);
    if (doTextColors != Settings.DoTextColors.Value) { Bubbler.Rebuild(); }

    l.CheckboxLabeled("Bubbles.DoNonPlayer".TranslateSimple(), ref Settings.DoNonPlayer.Value);
    l.CheckboxLabeled("Bubbles.DoAnimals".TranslateSimple(), ref Settings.DoAnimals.Value);
    l.CheckboxLabeled("Bubbles.DoDrafted".TranslateSimple(), ref Settings.DoDrafted.Value);
    l.CheckboxLabeled("Bubbles.HearingCheck".TranslateSimple() + " (Experimental)", ref Settings.HearingCheck.Value);
    if (Settings.HearingCheck.Value) { l.SliderLabeled("Bubbles.HearingRange".TranslateSimple(), ref Settings.HearingRange.Value, 1f, 32f, 1f); }
    else { l.Gap(Text.LineHeight + GenUI.GapLabel + l.verticalSpacing); }

    l.SliderLabeled("Bubbles.AutoHideSpeed".TranslateSimple(), ref Settings.AutoHideSpeed.Value, 1, 4, display: Settings.AutoHideSpeed.Value == Settings.AutoHideSpeedDisabled ? "Bubbles.AutoHideSpeedOff".TranslateSimple() : Settings.AutoHideSpeed.Value.ToString());

    l.SliderLabeled("Bubbles.AltitudeBase".TranslateSimple(), ref Settings.AltitudeBase.Value, 3, 44);
    l.SliderLabeled("Bubbles.AltitudeMax".TranslateSimple(), ref Settings.AltitudeMax.Value, 20, 60);
    l.SliderLabeled("Bubbles.ScaleMax".TranslateSimple(), ref Settings.ScaleMax.Value, 1f, 5f, 0.05f, Settings.ScaleMax.Value.ToStringPercent());
    l.SliderLabeled("Bubbles.PawnMax".TranslateSimple(), ref Settings.PawnMax.Value, 1, 15);

    l.SliderLabeled("Bubbles.FontSize".TranslateSimple(), ref Settings.FontSize.Value, 5, 30);
    l.SliderLabeled("Bubbles.PaddingX".TranslateSimple(), ref Settings.PaddingX.Value, 1, 40);
    l.SliderLabeled("Bubbles.PaddingY".TranslateSimple(), ref Settings.PaddingY.Value, 1, 40);
    l.SliderLabeled("Bubbles.WidthMax".TranslateSimple(), ref Settings.WidthMax.Value, 100, 500, 4);

    l.SliderLabeled("Bubbles.OffsetSpacing".TranslateSimple(), ref Settings.OffsetSpacing.Value, 2, 12);
    l.SliderLabeled("Bubbles.OffsetStart".TranslateSimple(), ref Settings.OffsetStart.Value, 0, 400, 2);

    var offsetDirection = Settings.OffsetDirection.Value.AsInt;
    l.SliderLabeled("Bubbles.OffsetDirection".TranslateSimple(), ref offsetDirection, 0, 3, display: "Bubbles.OffsetDirections".TranslateSimple()?.Split('|').ElementAtOrDefault(offsetDirection));
    Settings.OffsetDirection.Value = new Rot4(offsetDirection);

    l.SliderLabeled("Bubbles.OpacityStart".TranslateSimple(), ref Settings.OpacityStart.Value, 0.3f, 1f, 0.05f, Settings.OpacityStart.Value.ToStringPercent());
    l.SliderLabeled("Bubbles.OpacityHover".TranslateSimple(), ref Settings.OpacityHover.Value, 0.05f, 1f, 0.05f, Settings.OpacityHover.Value.ToStringPercent());
    // rim-universe #6. Real time by default: the tick-based sliders below it meant a
    // bubble lasted 1.7 seconds at Superfast, which is not long enough to read a
    // generated sentence.
    l.CheckboxLabeled("Bubbles.RealTimeFade".TranslateSimple(), ref Settings.RealTimeFade.Value);
    if (Settings.RealTimeFade.Value)
    {
      l.SliderLabeled("Bubbles.DwellSeconds".TranslateSimple(), ref Settings.DwellSeconds.Value, 0f, 20f, 0.5f, Settings.DwellSeconds.Value.ToString("0.0") + "s");
      l.SliderLabeled("Bubbles.DwellPerChar".TranslateSimple(), ref Settings.DwellPerChar.Value, 0f, 0.2f, 0.005f, Settings.DwellPerChar.Value.ToString("0.000") + "s");
      l.SliderLabeled("Bubbles.DwellMaxSeconds".TranslateSimple(), ref Settings.DwellMaxSeconds.Value, 5f, 120f, 1f, Settings.DwellMaxSeconds.Value.ToString("0") + "s");
      l.SliderLabeled("Bubbles.FadeSeconds".TranslateSimple(), ref Settings.FadeSeconds.Value, 0f, 10f, 0.1f, Settings.FadeSeconds.Value.ToString("0.0") + "s");
    }
    else
    {
      l.SliderLabeled("Bubbles.FadeStart".TranslateSimple(), ref Settings.FadeStart.Value, 100, 5000, 50);
      l.SliderLabeled("Bubbles.FadeLength".TranslateSimple(), ref Settings.FadeLength.Value, 50, 2500, 50);
    }

    l.ColorEntry("Bubbles.Background".TranslateSimple(), ref _colorBuffer[0], ref Settings.Background.Value);
    l.ColorEntry("Bubbles.Foreground".TranslateSimple(), ref _colorBuffer[1], ref Settings.Foreground.Value);
    l.ColorEntry("Bubbles.BackgroundSelected".TranslateSimple(), ref _colorBuffer[2], ref Settings.SelectedBackground.Value);
    l.ColorEntry("Bubbles.ForegroundSelected".TranslateSimple(), ref _colorBuffer[3], ref Settings.SelectedForeground.Value);

    l.End();
    Widgets.EndScrollView();

    _viewRect.height = l.CurHeight + GenUI.GapTiny;
  }

  private static void Reset()
  {
    _colorBuffer = new string[4];

    Settings.Reset();
  }

  public static void ShowWindow() => Find.WindowStack!.Add(new Dialog());

  private sealed class Dialog : Window
  {
    public override Vector2 InitialSize => new(600f, 600f);

    public Dialog()
    {
      optionalTitle = $"<b>{Mod.Name}</b>";
      doCloseX = true;
      doCloseButton = true;
      draggable = true;

      _viewRect = default;
    }

    public override void DoWindowContents(Rect rect)
    {
      rect.yMax -= 60f;
      DrawSettings(rect);
    }

    public override void PostClose()
    {
      Mod.Instance.WriteSettings();
      _viewRect = default;
    }
  }
}

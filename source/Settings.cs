using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Bubbles.Configuration;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Bubbles;

public class Settings : ModSettings
{
  public const int AutoHideSpeedDisabled = 1;

  private static readonly string[] SameConfigVersions =
  [
    "4.0"
  ];

  private static bool _resetRequired;

  public static bool Activated = true;

  public static readonly Setting<int> AutoHideSpeed = new(nameof(AutoHideSpeed), AutoHideSpeedDisabled);

  public static readonly Setting<bool> DoNonPlayer = new(nameof(DoNonPlayer), true);
  public static readonly Setting<bool> DoAnimals = new(nameof(DoAnimals), true);
  public static readonly Setting<bool> DoDrafted = new(nameof(DoDrafted), false);
  public static readonly Setting<bool> DoTextColors = new(nameof(DoTextColors), false);

  public static readonly Setting<bool> HearingCheck = new(nameof(HearingCheck), false);
  public static readonly Setting<float> HearingRange = new(nameof(HearingRange), 10f);

  public static readonly Setting<int> AltitudeBase = new(nameof(AltitudeBase), 11);
  public static readonly Setting<int> AltitudeMax = new(nameof(AltitudeMax), 40);
  public static readonly Setting<float> ScaleMax = new(nameof(ScaleMax), 1.25f);
  public static readonly Setting<int> PawnMax = new(nameof(PawnMax), 3);

  public static readonly Setting<int> FontSize = new(nameof(FontSize), 12);
  public static readonly Setting<int> PaddingX = new(nameof(PaddingX), 7);
  public static readonly Setting<int> PaddingY = new(nameof(PaddingY), 5);
  public static readonly Setting<int> WidthMax = new(nameof(WidthMax), 256);

  public static readonly Setting<int> OffsetSpacing = new(nameof(OffsetSpacing), 2);
  public static readonly Setting<int> OffsetStart = new(nameof(OffsetStart), 14);
  public static readonly Setting<Rot4> OffsetDirection = new(nameof(OffsetDirection), Rot4.North);

  public static readonly Setting<float> OpacityStart = new(nameof(OpacityStart), 0.9f);
  public static readonly Setting<float> OpacityHover = new(nameof(OpacityHover), 0.2f);

  /// <summary>Ticks the bubble holds at full opacity before it starts to go.</summary>
  public static readonly Setting<int> FadeStart = new(nameof(FadeStart), 500);

  /// <summary>
  /// Ticks the fade itself takes. Capped at <see cref="MaxFadeLength"/>.
  ///
  /// rim-universe #6. The slider used to run to 2500, which at normal speed is
  /// FORTY-ONE SECONDS of a bubble sitting at partial opacity. JK had it maxed,
  /// reasonably: the two sliders read as "start" and "length" and a player who wants
  /// longer bubbles raises both. But only the first one buys reading time — the second
  /// buys ghosting, and he described the result exactly: "they kind of semi-ghost for
  /// ages".
  ///
  /// A fade is a transition, not a state. Once it begins it should be over quickly at
  /// any game speed, so the ceiling is now short enough that it cannot be anything else.
  /// </summary>
  public static readonly Setting<int> FadeLength = new(nameof(FadeLength), 100);

  /// <summary>
  /// 300 ticks: five seconds at normal speed, under a second at Superfast. Long enough
  /// to read as a fade rather than a cut, short enough that it can never read as a
  /// second, dimmer bubble.
  /// </summary>
  public const int MaxFadeLength = 300;

  public static readonly Setting<Color> Background = new(nameof(Background), Color.white);
  public static readonly Setting<Color> Foreground = new(nameof(Foreground), Color.black);
  public static readonly Setting<Color> SelectedBackground = new(nameof(SelectedBackground), new Color(1f, 1f, 0.75f));
  public static readonly Setting<Color> SelectedForeground = new(nameof(SelectedForeground), Color.black);

  private static IEnumerable<Setting> AllSettings => typeof(Settings).GetFields().Select(static field => field.GetValue(null) as Setting).Where(static setting => setting is not null)!;

  public static void Reset() => AllSettings.Do(static setting => setting.ToDefault());

  public void CheckResetRequired()
  {
    if (!_resetRequired) { return; }
    _resetRequired = false;

    Write();

    Bubbles.Mod.Warning("Settings were reset with new update");
  }

  public override void ExposeData()
  {
    if (_resetRequired) { return; }

    var version = Scribe.mode is LoadSaveMode.Saving ? Bubbles.Mod.Version : null;
    Scribe_Values.Look(ref version, "Version");
    if (Scribe.mode is LoadSaveMode.LoadingVars && (version is null || (version is not Bubbles.Mod.Version && !SameConfigVersions.Contains(Regex.Match(version, @"^\d+\.\d+").Value))))
    {
      _resetRequired = true;
      return;
    }

    AllSettings.Do(static setting => setting.Scribe());

    // Clamp on the way in as well as in the UI, because the old slider went to 2500
    // and saved configs still hold those values. Without this the fix only reaches
    // players who happen to open the settings window. rim-universe #6.
    if (FadeLength.Value > MaxFadeLength)
    {
      var was = FadeLength.Value;
      FadeLength.Value = MaxFadeLength;
      Bubbles.Mod.Warning($"Fade length was {was} ticks, which is {was / 60}s of half-visible " +
                          $"bubble at normal speed. Clamped to {MaxFadeLength}. If you set it high " +
                          "to make bubbles last longer, raise \"ticks to start fade\" instead — " +
                          "that is the one that buys reading time.");
    }
  }
}

namespace Bubbles.Core;

/// <summary>
/// How long a bubble stays up, and how opaque it is while it does.
///
/// rim-universe #6. Lifetime was FadeStart + FadeLength *game ticks*, so the time a
/// bubble spent on screen was inversely proportional to game speed:
///
///   Normal 1x   10.0 s      Fast 3x    3.3 s
///   Superfast   1.7 s       Ultrafast  0.67 s
///
/// That was fine when a bubble said "Chitchat". With RimTalk it carries a generated
/// sentence, and reading time is wall-clock and scales with length — a fixed tick
/// count is neither. At the speed most colonies actually run at, the bubble was gone
/// before the first clause was read.
///
/// No Unity and no Verse in this file: it is source-linked into rim-universe's test
/// project and run, because "can a person read this in the time it is up" is
/// arithmetic and should not need a colony to check.
/// </summary>
public static class Dwell
{
  /// <summary>
  /// Seconds of unpaused wall clock a bubble should hold before it starts to fade.
  ///
  /// <paramref name="perChar"/> is a display rate, and it is deliberately slower than
  /// a reading rate. Prose is read at roughly 200-250 words a minute, which at about
  /// five characters a word is 0.05-0.06 s/char; the default here is 0.09, so the
  /// bubble is up for about 1.5x as long as reading it takes however long it is.
  /// A rate equal to the reading rate would give no headroom at all on a long line,
  /// because <paramref name="baseSeconds"/> is a constant and the text is not.
  /// </summary>
  public static float Seconds(int textLength, float baseSeconds, float perChar, float maxSeconds)
  {
    if (baseSeconds < 0f) { baseSeconds = 0f; }
    if (perChar < 0f) { perChar = 0f; }
    if (textLength < 0) { textLength = 0; }

    var seconds = baseSeconds + (perChar * textLength);

    // A cap of zero means "no cap". Otherwise a slider dragged to its minimum would
    // silently switch the feature off, which is the failure this issue is about.
    if (maxSeconds > 0f && seconds > maxSeconds) { seconds = maxSeconds; }

    return seconds;
  }

  /// <summary>
  /// Opacity at <paramref name="age"/> seconds: full until the dwell is up, then a
  /// linear fade, then gone. Returns 0 once it is over, which is the caller's signal
  /// to drop the bubble.
  /// </summary>
  public static float Opacity(float age, float dwell, float fade, float opacityStart)
  {
    if (age <= dwell) { return opacityStart; }

    // A fade of zero is a hard cut, not a divide by zero.
    if (fade <= 0f) { return 0f; }

    var elapsed = age - dwell;
    if (elapsed >= fade) { return 0f; }

    return opacityStart * (1f - (elapsed / fade));
  }
}

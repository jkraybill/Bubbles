using UnityEngine;
using Verse;

namespace Bubbles.Core;

/// <summary>
/// Wall-clock seconds of *unpaused* game, which is the only clock a reader has.
///
/// rim-universe #6. Not Time.realtimeSinceStartup: that keeps running while the game
/// is paused, so bubbles would expire while the player was paused reading them —
/// which is exactly when they are being read. Not ticks either, for the reason in
/// <see cref="Dwell"/>.
///
/// Advanced once per frame from <see cref="Bubbler.Draw"/>. A frame in which nothing
/// is drawn — the world map is open, the game is minimised — does not age bubbles,
/// which is the behaviour anyone would want.
/// </summary>
public static class BubbleClock
{
  public static float Now { get; private set; }

  public static void Advance()
  {
    if (Find.TickManager is null || Find.TickManager.Paused) { return; }

    // Guard against a stall or a load hitching the delta into a jump that would
    // expire every bubble on screen at once.
    var delta = Time.deltaTime;
    if (delta > 0.5f) { delta = 0.5f; }

    Now += delta;
  }
}

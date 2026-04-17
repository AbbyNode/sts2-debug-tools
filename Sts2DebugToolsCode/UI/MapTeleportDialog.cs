using Godot;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Sts2DebugTools.Sts2DebugToolsCode.UI;

/// <summary>
/// Modal overlay that lists every <see cref="MapPoint"/> in the current
/// act's map and lets the player teleport to any one of them.
///
/// Uses <see cref="RunManager.EnterMapCoordDebug"/> so the transition
/// bypasses the normal travel-restriction checks.
/// </summary>
internal static class MapTeleportDialog
{
    private const float CanvasWidth = 1920f;
    private const float CanvasHeight = 1080f;

    /// <summary>
    /// Creates and attaches the dialog to <paramref name="parent"/>.
    /// Returns immediately; the dialog is driven by user button presses.
    /// </summary>
    internal static void Show(Node parent)
    {
        var state = RunManager.Instance.DebugOnlyGetState();
        if (state == null)
        {
            MainFile.Logger.Warn("[DebugTools] MapTeleport: no active run state.");
            return;
        }

        // ------------------------------------------------------------------
        // Semi-transparent full-screen backdrop.
        // ------------------------------------------------------------------
        var overlay = new ColorRect
        {
            Name = "DebugMapTeleportOverlay",
            Color = new Color(0f, 0f, 0f, 0.72f),
            Position = Vector2.Zero,
            Size = new Vector2(CanvasWidth, CanvasHeight),
        };

        // ------------------------------------------------------------------
        // Title label.
        // ------------------------------------------------------------------
        var title = new Label
        {
            Text = "Teleport to Room",
            HorizontalAlignment = HorizontalAlignment.Center,
            Position = new Vector2(0f, 30f),
            Size = new Vector2(CanvasWidth, 60f),
        };
        title.AddThemeFontSizeOverride("font_size", 40);
        title.AddThemeColorOverride("font_color", Colors.White);
        title.AddThemeColorOverride("font_outline_color", Colors.Black);
        title.AddThemeConstantOverride("outline_size", 4);
        overlay.AddChild(title);

        // ------------------------------------------------------------------
        // Cancel button at the bottom.
        // ------------------------------------------------------------------
        var cancelBtn = new Button
        {
            Text = "Cancel",
            Position = new Vector2((CanvasWidth - 200f) / 2f, CanvasHeight - 80f),
            Size = new Vector2(200f, 50f),
        };
        cancelBtn.AddThemeFontSizeOverride("font_size", 22);
        cancelBtn.Pressed += () => overlay.QueueFree();
        overlay.AddChild(cancelBtn);

        // ------------------------------------------------------------------
        // Scrollable area for room buttons.
        // ------------------------------------------------------------------
        float scrollY = 110f;
        float scrollHeight = CanvasHeight - scrollY - 100f;

        var scroll = new ScrollContainer
        {
            Position = new Vector2(80f, scrollY),
            Size = new Vector2(CanvasWidth - 160f, scrollHeight),
        };
        overlay.AddChild(scroll);

        var grid = new GridContainer
        {
            Columns = 4,
        };
        grid.AddThemeConstantOverride("h_separation", 12);
        grid.AddThemeConstantOverride("v_separation", 12);
        scroll.AddChild(grid);

        // ------------------------------------------------------------------
        // One button per map point, ordered row-by-row then column-by-column.
        // ------------------------------------------------------------------
        var mapPoints = state.Map.GetAllMapPoints()
            .OrderBy(p => p.coord.row)
            .ThenBy(p => p.coord.col)
            .ToList();

        if (mapPoints.Count == 0)
        {
            var noRooms = new Label
            {
                Text = "No rooms found on the current map.",
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            noRooms.AddThemeFontSizeOverride("font_size", 26);
            grid.AddChild(noRooms);
        }
        else
        {
            foreach (var point in mapPoints)
            {
                var capturedCoord = point.coord;
                var capturedType = point.PointType;
                RoomType roomType = MapPointTypeToRoomType(capturedType);

                string label = $"Row {capturedCoord.row}, Col {capturedCoord.col}\n{capturedType}";
                var btn = new Button
                {
                    Text = label,
                    CustomMinimumSize = new Vector2(400f, 70f),
                    AutowrapMode = TextServer.AutowrapMode.WordSmart,
                };
                btn.AddThemeFontSizeOverride("font_size", 18);
                btn.Pressed += () =>
                {
                    overlay.QueueFree();
                    MainFile.Logger.Info(
                        $"[DebugTools] Teleporting to map point ({capturedCoord.col}, {capturedCoord.row}) [{capturedType}].");
                    _ = RunManager.Instance.EnterMapCoordDebug(capturedCoord, roomType, capturedType);
                };
                grid.AddChild(btn);
            }
        }

        parent.CallDeferred(Node.MethodName.AddChild, overlay);
        MainFile.Logger.Info($"[DebugTools] Map teleport dialog opened with {mapPoints.Count} room(s).");
    }

    // -----------------------------------------------------------------------
    //  Helpers
    // -----------------------------------------------------------------------

    private static RoomType MapPointTypeToRoomType(MapPointType pointType) => pointType switch
    {
        MapPointType.Monster  => RoomType.Monster,
        MapPointType.Elite    => RoomType.Elite,
        MapPointType.Boss     => RoomType.Boss,
        MapPointType.Treasure => RoomType.Treasure,
        MapPointType.Shop     => RoomType.Shop,
        MapPointType.RestSite => RoomType.RestSite,
        MapPointType.Ancient  => RoomType.Event,
        MapPointType.Unknown  => RoomType.Monster,
        _                     => RoomType.Monster,
    };
}

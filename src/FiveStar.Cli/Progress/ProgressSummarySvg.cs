// Copyright (c) 2026 Five Star Recomp contributors.
// SPDX-License-Identifier: GPL-3.0-or-later

namespace FiveStar.Cli.Progress;

/// <summary>
/// Renders a <see cref="ProgressStatus"/> as the short chart of the README: an area of single steps as a strip with the
/// first unfinished step named, every other area as one bar. Only finished items count towards a percentage.
/// </summary>
public static class ProgressSummarySvg
{
    private const double ContentWidth = ProgressSvg.Width - (2 * ProgressSvg.Padding);
    private const double HeadlineSize = 15;
    private const double HeadlineBaseline = 15;
    private const double StripTop = 24;
    private const double StripHeight = 20;
    private const double Gap = 2;
    private const double Radius = 2;
    private const double CurrentBaseline = 62;
    private const double StepsHeight = 84;
    private const double TextSize = 12;
    private const double CharWidth = 6.6;
    private const int MaxCurrentChars = (int)(ContentWidth / CharWidth);
    private const double BarLeft = 170;
    private const double BarWidth = 350;
    private const double BarHeight = 12;
    private const double BarBaseline = 10.5;
    private const double ValueLeft = 536;
    private const double RowHeight = 24;
    private const double MinShare = 5;
    private const double LegendSpace = 6;
    private const string Ellipsis = "…";

    public static string Render(ProgressStatus status)
    {
        var canvas = new SvgCanvas();
        var y = ProgressSvg.Padding;

        foreach (var area in status.Areas)
        {
            if (IsSteps(area))
            {
                y = DrawSteps(canvas, area, y);
            }
            else
            {
                y = DrawBar(canvas, area, y);
            }
        }

        y = ProgressSvg.DrawLegend(canvas, y + LegendSpace);

        return ProgressSvg.Document(canvas.Body, Math.Ceiling(y));
    }

    /// <summary>An area whose groups are one item each is a list of steps.</summary>
    private static bool IsSteps(ProgressArea area)
    {
        return area.Groups.Count > 1 && area.Groups.All(group => group.Total == 1);
    }

    private static double DrawSteps(SvgCanvas canvas, ProgressArea area, double y)
    {
        canvas.BoldText("ink", ProgressSvg.Padding, y + HeadlineBaseline, HeadlineSize, $"{area.Title}: {Counts(area)}");
        var pitch = (ContentWidth + Gap) / area.Groups.Count;

        for (var index = 0; index < area.Groups.Count; index++)
        {
            var x = ProgressSvg.Padding + (index * pitch);
            canvas.Rect(StepClass(area.Groups[index]), x, y + StripTop, pitch - Gap, StripHeight, Radius);
        }

        var current = area.Groups.FirstOrDefault(group => group.Done < group.Total);

        if (current is not null)
        {
            canvas.Text("ink2", ProgressSvg.Padding, y + CurrentBaseline, TextSize, Current(current));
        }

        return y + StepsHeight;
    }

    private static string StepClass(ProgressGroup group)
    {
        if (group.Partial > 0)
        {
            return CellGrid.PartialClass;
        }

        if (group.Done > 0)
        {
            return CellGrid.DoneClass;
        }

        return CellGrid.TodoClass;
    }

    private static string Current(ProgressGroup group)
    {
        var text = "Now: " + group.Name;

        if (!string.IsNullOrEmpty(group.Note))
        {
            text += " (" + group.Note + ")";
        }

        if (text.Length <= MaxCurrentChars)
        {
            return text;
        }

        return text[..(MaxCurrentChars - 1)].TrimEnd() + Ellipsis;
    }

    private static double DrawBar(SvgCanvas canvas, ProgressArea area, double y)
    {
        var total = area.Groups.Sum(group => group.Total);
        var done = area.Groups.Sum(group => group.Done);
        var partialWidth = Share(area.Groups.Sum(group => group.Partial), total);
        var todoWidth = Share(area.Groups.Sum(group => group.Todo), total);
        var doneWidth = 0.0;

        if (done > 0)
        {
            doneWidth = Math.Max(MinShare, BarWidth - partialWidth - todoWidth);
        }

        canvas.Text("ink", ProgressSvg.Padding, y + BarBaseline, TextSize, area.Title);
        DrawShare(canvas, CellGrid.DoneClass, BarLeft, y, doneWidth);
        DrawShare(canvas, CellGrid.PartialClass, BarLeft + doneWidth, y, partialWidth);
        DrawShare(canvas, CellGrid.TodoClass, BarLeft + doneWidth + partialWidth, y, todoWidth);

        var value = $"{ProgressText.Headline(done, total)}%, {Counts(area)}";
        canvas.Text("ink2", ValueLeft, y + BarBaseline, TextSize, value);

        return y + RowHeight;
    }

    /// <summary>The width of a part of the bar; a part that is not empty stays visible.</summary>
    private static double Share(double amount, double total)
    {
        if (amount <= 0)
        {
            return 0;
        }

        return Math.Max(MinShare, BarWidth * amount / total);
    }

    private static void DrawShare(SvgCanvas canvas, string cssClass, double x, double y, double width)
    {
        if (width > 0)
        {
            canvas.Rect(cssClass, x, y, width - Gap, BarHeight, Radius);
        }
    }

    private static string Counts(ProgressArea area)
    {
        var total = area.Groups.Sum(group => group.Total);
        var done = area.Groups.Sum(group => group.Done);
        var partial = area.Groups.Sum(group => group.Partial);
        var text = $"{ProgressText.Amount(done)} of {ProgressText.Amount(total)}{area.Unit ?? string.Empty}";

        if (partial > 0)
        {
            text += $", {ProgressText.Amount(partial)} partial";
        }

        return text;
    }
}

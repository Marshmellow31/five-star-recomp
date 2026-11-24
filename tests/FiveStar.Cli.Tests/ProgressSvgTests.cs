// Copyright (c) 2026 Five Star Recomp contributors.
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Xml.Linq;
using FiveStar.Cli.Progress;
using Xunit;

namespace FiveStar.Cli.Tests;

/// <summary>The progress report renders as well-formed SVG with one hover area per group; the summary does not overstate it.</summary>
public sealed class ProgressSvgTests
{
    private static readonly XNamespace Svg = "http://www.w3.org/2000/svg";

    [Fact]
    public void A_small_status_renders_one_group_area_per_group()
    {
        var status = new ProgressStatus(
            [
                new ProgressArea("shaders", "Shaders", null, 100, [
                    new ProgressGroup("Pixel", 90, 0, 10, "not yet: dxc-error (10)"),
                    new ProgressGroup("Mesh", 5, 3, 0, null),
                ]),
                new ProgressArea("archives", "Archive bytes & <more>", " GiB", null, [
                    new ProgressGroup("a.rpf", 1.25, 0, 0.75, null),
                ]),
            ],
            [new NeededColumn("Engineering", [new NeededItem("First frame", "swap chain and a real draw")])]);

        var document = XDocument.Parse(ProgressSvg.Render(status));

        var hits = document.Descendants(Svg + "rect").Where(rect => (string?)rect.Attribute("class") == "hit").ToList();
        Assert.Equal(3, hits.Count);
        Assert.All(hits, hit => Assert.NotNull(hit.Element(Svg + "title")));
    }

    [Fact]
    public void The_summary_counts_only_finished_items_and_names_the_current_step()
    {
        var status = new ProgressStatus(
            [
                new ProgressArea("boot", "Booting", " stages", null, [
                    new ProgressGroup("Load", 1, 0, 0, null),
                    new ProgressGroup("Story", 0, 1, 0, "too slow"),
                    new ProgressGroup("In game", 0, 0, 1, null),
                ]),
                new ProgressArea("shaders", "Shaders", null, null, [new ProgressGroup("Mesh", 90, 10, 0, null)]),
            ],
            null);

        var document = XDocument.Parse(ProgressSummarySvg.Render(status));

        var texts = document.Descendants(Svg + "text").Select(text => text.Value).ToList();
        Assert.Contains("Booting: 1 of 3 stages, 1 partial", texts);
        Assert.Contains("Now: Story (too slow)", texts);
        Assert.Contains("90.0%, 90 of 100, 10 partial", texts);
    }
}

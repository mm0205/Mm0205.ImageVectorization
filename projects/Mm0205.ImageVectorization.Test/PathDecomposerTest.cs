using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Mm0205.ImageVectorization.Fitting;
using Mm0205.ImageVectorization.PathExtraction;
using Mm0205.ImageVectorization.PathInterpolation;
using Mm0205.ImageVectorization.VectorOperations;
using SkiaSharp;
using Xunit;

namespace Mm0205.ImageVectorization.Test;

public class PathDecomposerTest
{
    [Fact(DisplayName = "パスなし")]
    public void TestNoPath()
    {
        var context = new VectorizationContext(new byte[0, 0]);
        new PathExtractor(context).Extract();
        Assert.False(context.ClosedPaths.Any());
    }

    [Fact(DisplayName = "1ピクセルのパス")]
    public void TestOnePixelPath()
    {
        var data = new byte[,]
        {
            {
                1
            }
        };

        var context = new VectorizationContext(data);
        new PathExtractor(context).Extract();
        Assert.True(context.ClosedPaths.Any());
    }

    [Fact(DisplayName = "アットマーク")]
    public void TestAtMark()
    {
        using var bmp = SKBitmap.Decode("/Users/munenaga/Downloads/AtMark.png");
        using var binBmp = new SKBitmap(bmp.Width, bmp.Height);
        var data = new byte[bmp.Height, bmp.Width];
        for (var y = 0; y < bmp.Height; y++)
        {
            for (var x = 0; x < bmp.Width; x++)
            {
                data[y, x] = bmp.GetPixel(x, y).Red == 0 ? (byte)1 : (byte)0;
                if (data[y, x] != 0)
                {
                    binBmp.SetPixel(x, y, SKColors.Red);
                }
                else
                {
                    binBmp.SetPixel(x, y, SKColors.Transparent);
                }
            }
        }

        using var binBmpStream = File.OpenWrite("/Users/munenaga/Downloads/AtMark.bin.png");
        binBmp.Encode(binBmpStream, SKEncodedImageFormat.Png, 100);

        var context = new VectorizationContext(data);
        new Vectorizer(context).Vectorize();

        SaveTest(data, context);
        // SavePath(bmp, context);
        // SaveInterpolatedPath(bmp, context);
    }

    private static void SaveTest(
        byte[,] data,
        VectorizationContext context
    )
    {
        var xml = new XmlDocument();
        var svg = xml.CreateElement("svg", "http://www.w3.org/2000/svg");
        xml.AppendChild(svg);

        var width = data.GetLength(1) * 10.0;
        var hw = width / 2.0;
        var widthAttribute = xml.CreateAttribute("width");
        widthAttribute.Value = width.ToString(CultureInfo.InvariantCulture);
        svg.Attributes.Append(widthAttribute);

        var height = data.GetLength(0) * 10.0;
        var hh = height / 2.0;
        var heightAttribute = xml.CreateAttribute("height");
        heightAttribute.Value = height.ToString(CultureInfo.InvariantCulture);
        svg.Attributes.Append(heightAttribute);

        var viewBoxAttribute = xml.CreateAttribute("viewBox");
        viewBoxAttribute.Value = $"-10 -10 {width + 10} {height + 10}";
        svg.Attributes.Append(viewBoxAttribute);

        // var gInterpolated = xml.CreateElement("g", "http://www.w3.org/2000/svg");
        // svg.AppendChild(gInterpolated);

        // var gInterpolatedId = xml.CreateAttribute("id");
        // gInterpolatedId.Value = "interpolated";
        // gInterpolated.Attributes.Append(gInterpolatedId);
        //
        // foreach (var path in context.InterpolatedPaths)
        // {
        //     foreach (var edge in path.Edges)
        //     {
        //         var lineElement = xml.CreateElement("line", "http://www.w3.org/2000/svg");
        //         gInterpolated.AppendChild(lineElement);
        //         
        //         var x1 = xml.CreateAttribute("x1");
        //         x1.Value = (edge.From.X * 10).ToString(CultureInfo.InvariantCulture);
        //         lineElement.Attributes.Append(x1);
        //         
        //         var y1 = xml.CreateAttribute("y1");
        //         y1.Value = ((edge.From.Y ) * 10).ToString(CultureInfo.InvariantCulture);
        //         lineElement.Attributes.Append(y1);
        //         
        //         var x2 = xml.CreateAttribute("x2");
        //         x2.Value = (edge.To.X * 10).ToString(CultureInfo.InvariantCulture);
        //         lineElement.Attributes.Append(x2);
        //         
        //         var y2 = xml.CreateAttribute("y2");
        //         y2.Value = ((edge.To.Y ) * 10).ToString(CultureInfo.InvariantCulture);
        //         lineElement.Attributes.Append(y2);
        //         
        //         var stroke = xml.CreateAttribute("stroke");
        //         stroke.Value = "Red";
        //         lineElement.Attributes.Append(stroke);
        //     }
        // }
        //

        // SaveInterpolatedPath(context, xml, svg);
        // SaveApproximateOperations(context, xml, svg);
        SaveApproximateOperations2(context, xml, svg);
        // SaveSegments(context, xml, svg);

        // SaveBitmap(data, xml, svg);

        using var fs = File.Open("/Users/munenaga/Downloads/AtMark.svg", FileMode.Create, FileAccess.Write);
        xml.Save(fs);
    }

    private static void SaveBitmap(
        byte[,] data,
        XmlDocument xml,
        XmlElement svg
    )
    {
        var gBitmap = xml.CreateElement("g", "http://www.w3.org/2000/svg");
        var gBitmapId = xml.CreateAttribute("id");
        gBitmapId.Value = "bitmap";
        gBitmap.Attributes.Append(gBitmapId);
        svg.AppendChild(gBitmap);
        for (var y = 0; y < data.GetLength(0); y++)
        {
            for (var x = 0; x < data.GetLength(1); x++)
            {
                // <rect x="120" y="" width="100" height="100" />
                var rectElement = xml.CreateElement("rect", "http://www.w3.org/2000/svg");
                gBitmap.AppendChild(rectElement);

                var xElem = xml.CreateAttribute("x");
                xElem.Value = (x * 10).ToString(CultureInfo.InvariantCulture);
                rectElement.Attributes.Append(xElem);

                var yElem = xml.CreateAttribute("y");
                yElem.Value = (y * 10).ToString(CultureInfo.InvariantCulture);
                rectElement.Attributes.Append(yElem);

                var w = xml.CreateAttribute("width");
                w.Value = "10";
                rectElement.Attributes.Append(w);

                var h = xml.CreateAttribute("height");
                h.Value = "10";
                rectElement.Attributes.Append(h);

                // var stroke = xml.CreateAttribute("stroke");
                // stroke.Value = "#000";
                // rectElement.Attributes.Append(stroke);

                var fill = xml.CreateAttribute("fill");
                fill.Value = data[y, x] != 0 ? "yellow" : "transparent";
                rectElement.Attributes.Append(fill);

                var opacity = xml.CreateAttribute("opacity");
                opacity.Value = data[y, x] != 0 ? "0.8" : "transparent";
                rectElement.Attributes.Append(opacity);
            }
        }
    }

    private static void SaveApproximateOperations(
        VectorizationContext context,
        XmlDocument xml,
        XmlElement svg
    )
    {
        var gOperations = xml.CreateElement("g", "http://www.w3.org/2000/svg");
        svg.AppendChild(gOperations);

        var operationsId = xml.CreateAttribute("id");
        operationsId.Value = "operations";
        gOperations.Attributes.Append(operationsId);

        var i = 0;
        var colors = new[]
        {
            "RosyBrown",
            "RoyalBlue",
            "SaddleBrown",
            "Salmon",
            "#00008b",
            "#8fbc8f",
            "#ffa500",
            "#ffc0cb"
        };

        foreach (var vectorPath in context.ApproximatePaths)
        {
            var gOperation = xml.CreateElement("g", "http://www.w3.org/2000/svg");
            gOperations.AppendChild(gOperation);

            var segmentClass = xml.CreateAttribute("class");
            segmentClass.Value = "operation";
            gOperation.Attributes.Append(segmentClass);

            var pathStrokeWidth = xml.CreateAttribute("stroke-width");
            pathStrokeWidth.Value = "2";
            gOperation.Attributes.Append(pathStrokeWidth);
            var pathStroke = xml.CreateAttribute("stroke");
            pathStroke.Value = colors[i];
            i = (i + 1) % colors.Length;
            gOperation.Attributes.Append(pathStroke);

            var fill = xml.CreateAttribute("fill");
            fill.Value = "transparent";
            gOperation.Attributes.Append(fill);

            var pathElement = xml.CreateElement("path", "http://www.w3.org/2000/svg");
            gOperation.AppendChild(pathElement);

            var sb = new StringBuilder();
            var first = true;

            foreach (var operation in vectorPath.Operations)
            {
                if (operation is LineTo lineTo)
                {
                    if (first)
                    {
                        sb.Append($"M {lineTo.From.X * 10} {(lineTo.From.Y) * 10} ");
                        first = false;
                    }

                    sb.Append($"L {lineTo.To.X * 10} {(lineTo.To.Y) * 10} ");
                }
                else if (operation is CurveTo curveTo)
                {
                    if (first)
                    {
                        sb.Append($"M {curveTo.P0.X * 10} {(curveTo.P0.Y) * 10} ");
                        first = false;
                    }

                    sb.Append(
                        $"Q {curveTo.P1.X * 10} {(curveTo.P1.Y) * 10} {curveTo.P2.X * 10}  {(curveTo.P2.Y) * 10}");
                }
            }

            var pathD = xml.CreateAttribute("d");
            pathD.Value = sb.ToString();
            pathElement.Attributes.Append(pathD);
        }
    }

    private static void SaveApproximateOperations2(
        VectorizationContext context,
        XmlDocument xml,
        XmlElement svg
    )
    {
        var gOperations = xml.CreateElement("g", "http://www.w3.org/2000/svg");
        svg.AppendChild(gOperations);

        var operationsId = xml.CreateAttribute("id");
        operationsId.Value = "operations";
        gOperations.Attributes.Append(operationsId);

        var gOperation = xml.CreateElement("g", "http://www.w3.org/2000/svg");
        gOperations.AppendChild(gOperation);

        var segmentClass = xml.CreateAttribute("class");
        segmentClass.Value = "operation";
        gOperation.Attributes.Append(segmentClass);

        var pathStrokeWidth = xml.CreateAttribute("stroke-width");
        pathStrokeWidth.Value = "2";
        gOperation.Attributes.Append(pathStrokeWidth);
        var pathStroke = xml.CreateAttribute("stroke");
        pathStroke.Value = "transparent";
        gOperation.Attributes.Append(pathStroke);

        var fill = xml.CreateAttribute("fill");
        fill.Value = "red";
        gOperation.Attributes.Append(fill);

        // var opacity = xml.CreateAttribute("opacity");
        // opacity.Value = "0.2";
        // gOperation.Attributes.Append(opacity);

        var pathElement = xml.CreateElement("path", "http://www.w3.org/2000/svg");
        gOperation.AppendChild(pathElement);

        var fillRule = xml.CreateAttribute("fill-rule");
        fillRule.Value = "evenodd";
        pathElement.Attributes.Append(fillRule);

        var sb = new StringBuilder();

        foreach (var vectorPath in context.ApproximatePaths)
        {
            var first = true;
            foreach (var operation in vectorPath.Operations)
            {
                if (operation is LineTo lineTo)
                {
                    if (first)
                    {
                        sb.Append($"M {lineTo.From.X * 10} {(lineTo.From.Y) * 10} ");
                        first = false;
                    }

                    sb.Append($"L {lineTo.To.X * 10} {(lineTo.To.Y) * 10} ");
                }
                else if (operation is CurveTo curveTo)
                {
                    if (first)
                    {
                        sb.Append($"M {curveTo.P0.X * 10} {(curveTo.P0.Y) * 10} ");
                        first = false;
                    }

                    sb.Append(
                        $"Q {curveTo.P1.X * 10} {(curveTo.P1.Y) * 10} {curveTo.P2.X * 10}  {(curveTo.P2.Y) * 10}");
                }
            }

            sb.Append("Z ");
        }


        var pathD = xml.CreateAttribute("d");
        pathD.Value = sb.ToString();
        pathElement.Attributes.Append(pathD);
    }

    private static void SaveSegments(
        VectorizationContext context,
        XmlDocument xml,
        XmlElement svg
    )
    {
        var gSegments = xml.CreateElement("g", "http://www.w3.org/2000/svg");
        svg.AppendChild(gSegments);

        var gSegmentsId = xml.CreateAttribute("id");
        gSegmentsId.Value = "segments";
        gSegments.Attributes.Append(gSegmentsId);

        var i = 0;
        var colors = new[]
        {
            "lavender",
            "lightsteelblue",
            "lightslategray",
            "slategray",
            "steelblue",
            "royalblue",
            "midnightblue",
            "navy",
            "darkcyan",
            "teal",
            "darkslategray",
            "darkgreen",
            "green",
            "forestgreen",
            "seagreen",
            "wheat",
            "burlywood",
            "tan",
            "khaki",
            "yellow",
            "coral",
            "tomato",
            "orangered",
        };

        foreach (var segmentList in context.PathSegments)
        {
            foreach (var segment in segmentList.Segments)
            {
                var gSegment = xml.CreateElement("g", "http://www.w3.org/2000/svg");
                gSegments.AppendChild(gSegment);

                var segmentClass = xml.CreateAttribute("class");
                segmentClass.Value = "segment";
                gSegment.Attributes.Append(segmentClass);

                var pathStroke = xml.CreateAttribute("stroke");
                pathStroke.Value = colors[i];
                i = (i + 1) % colors.Length;
                gSegment.Attributes.Append(pathStroke);
                
                var strokeWidth = xml.CreateAttribute("stroke-width");
                strokeWidth.Value = "2";
                gSegment.Attributes.Append(strokeWidth);

                var fill = xml.CreateAttribute("fill");
                fill.Value = "transparent";
                gSegment.Attributes.Append(fill);

                var pathElement = xml.CreateElement("path", "http://www.w3.org/2000/svg");
                gSegment.AppendChild(pathElement);

                var sb = new StringBuilder();
                var first = true;
                foreach (var point in segment.Points)
                {
                    if (first)
                    {
                        sb.Append($"M {point.Point.X * 10} {(point.Point.Y) * 10} ");
                        first = false;
                        continue;
                    }

                    sb.Append($"L {point.Point.X * 10} {(point.Point.Y) * 10} ");
                }

                var pathD = xml.CreateAttribute("d");
                pathD.Value = sb.ToString();
                pathElement.Attributes.Append(pathD);
            }
        }
    }

    private static void SaveInterpolatedPath(
        VectorizationContext context,
        XmlDocument xml,
        XmlElement svg
    )
    {
        var gPath = xml.CreateElement("g", "http://www.w3.org/2000/svg");
        svg.AppendChild(gPath);

        var gPathId = xml.CreateAttribute("id");
        gPathId.Value = "i-path";
        gPath.Attributes.Append(gPathId);

        // var opacity = xml.CreateAttribute("opacity");
        // opacity.Value = "0.2";
        // gPath.Attributes.Append(opacity);

        var pathElement = xml.CreateElement("path", "http://www.w3.org/2000/svg");
        gPath.AppendChild(pathElement);

        var pathStroke = xml.CreateAttribute("stroke");
        pathStroke.Value = "transparent";
        pathElement.Attributes.Append(pathStroke);

        var pathFill = xml.CreateAttribute("fill");
        pathFill.Value = "blue";
        pathElement.Attributes.Append(pathFill);

        var fillRule = xml.CreateAttribute("fill-rule");
        fillRule.Value = "evenodd";
        pathElement.Attributes.Append(fillRule);

        var sb = new StringBuilder();
        foreach (var path in context.InterpolatedPaths)
        {
            var first = true;
            foreach (var point in path.Points)
            {
                if (first)
                {
                    sb.Append($"M {point.Point.X * 10} {(point.Point.Y) * 10} ");
                    first = false;
                    continue;
                }

                sb.Append($"L {point.Point.X * 10} {(point.Point.Y) * 10} ");
            }

            sb.Append("Z ");
        }

        var pathD = xml.CreateAttribute("d");
        pathD.Value = sb.ToString();
        pathElement.Attributes.Append(pathD);
    }

    // private static void SavePath(
    //     SKBitmap bmp,
    //     VectorizationContext context
    // )
    // {
    //     using var dest = new SKBitmap(bmp.Width + 2, bmp.Height + 2);
    //     using var canvas = new SKCanvas(dest);
    //
    //     var colors = new[]
    //     {
    //         SKColors.Red,
    //         SKColors.Green,
    //         SKColors.Blue,
    //         SKColors.Black,
    //         SKColors.Yellow,
    //         SKColors.White,
    //         SKColors.Gray,
    //         SKColors.Brown,
    //     };
    //
    //     var i = 0;
    //     foreach (var path in context.ClosedPaths)
    //     {
    //         using var linePaint = new SKPaint
    //         {
    //             StrokeWidth = 1,
    //             Color = colors[i],
    //             Style = SKPaintStyle.Stroke
    //         };
    //         i = (i + 1) % colors.Length;
    //         foreach (var edge in path.Edges)
    //         {
    //             canvas.DrawLine(edge.From.X, edge.From.Y, edge.To.X, edge.To.Y, linePaint);
    //         }
    //     }
    //
    //     var encodedData = dest.Encode(SKEncodedImageFormat.Png, 100);
    //     using var stream = File.OpenWrite("/Users/munenaga/Downloads/AtMark.path.png");
    //     encodedData.SaveTo(stream);
    // }
    //
    // private static void SaveInterpolatedPath(
    //     SKBitmap bmp,
    //     VectorizationContext context
    // )
    // {
    //     using var dest = new SKBitmap(bmp.Width * 10, bmp.Height * 10);
    //     using var canvas = new SKCanvas(dest);
    //
    //     var colors = new[]
    //     {
    //         SKColors.Red,
    //         SKColors.Green,
    //         SKColors.Blue,
    //         SKColors.Black,
    //         SKColors.Yellow,
    //         SKColors.White,
    //         SKColors.Gray,
    //         SKColors.Brown,
    //     };
    //
    //     var i = 0;
    //
    //     foreach (var path in context.InterpolatedPaths)
    //     {
    //         using var linePaint = new SKPaint
    //         {
    //             StrokeWidth = 1,
    //             Color = colors[i],
    //             Style = SKPaintStyle.Stroke
    //         };
    //         i = (i + 1) % colors.Length;
    //         foreach (var edge in path.Poitns)
    //         {
    //             canvas.DrawLine((float)edge.From.X * 10, (float)edge.From.Y * 10, (float)edge.To.X * 10,
    //                 (float)edge.To.Y * 10, linePaint);
    //         }
    //     }
    //
    //     var encodedData = dest.Encode(SKEncodedImageFormat.Png, 100);
    //     using var stream = File.OpenWrite("/Users/munenaga/Downloads/AtMark.interpolated.png");
    //     encodedData.SaveTo(stream);
    // }
}
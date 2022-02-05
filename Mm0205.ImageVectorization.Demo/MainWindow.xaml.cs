using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using ABI.Microsoft.UI.Xaml.Media.Imaging;
using ABI.Windows.UI.Popups;
using Mm0205.ImageVectorization.VectorOperations;
using SkiaSharp;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mm0205.ImageVectorization.Demo
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public ObservableCollection<string> Fonts { get; } = new();

        public List<SKTypeface> Typefaces { get; } = new();

        public string SelectedFont { get; set; } = string.Empty;

        public int SelectedFontIndex { get; set; }

        public string SampleText { get; set; } = string.Empty;


        public ObservableCollection<string> Images { get; } = new();

        public double LineFitThreshold { get; set; }

        public double CurveFitThreshold { get; set; }

        public bool DisableLintFit { get; set; }

        public bool ShouldShowGrid { get; set; } = true;

        public Color GridColor { get; set; } = Color.FromArgb(255, 0, 0, 0);

        public bool ShouldShowBitmap { get; set; } = true;

        public Color BitmapColor { get; set; } = Color.FromArgb(128, 0, 255, 0);


        public bool ShouldShowInterpolation { get; set; } = true;

        public Color InterpolationColor { get; set; } = Color.FromArgb(128, 0, 0, 255);

        public bool ShouldShowVector { get; set; } = true;

        public Color VectorColor { get; set; } = Color.FromArgb(128, 255, 0, 0);


        public MainWindow()
        {
            var options = new VectorizationOptions();
            LineFitThreshold = options.LineFitThreshold;
            CurveFitThreshold = options.CurveFitThreshold;
            DisableLintFit = options.DisableLineFit;

            InitializeComponent();

            MyWindow.Title = "フォントをベクトル化するデモ";
        }

        private async Task CreateVectorsAsync(string selectedFont, string sampleText)
        {
            Images.Clear();

            var typefaceIndex = SelectedFontIndex;

            var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var imagesFolder = Path.Combine(root, "Mm0205.ImageVectorization.Demo", "images");
            if (!Directory.Exists(imagesFolder))
            {
                Directory.CreateDirectory(imagesFolder);
            }
            foreach (var file in Directory.GetFiles(imagesFolder))
            {
                File.Delete(file);
            }


            var paths = await Task.Run(() =>
              {
                  var result = new List<string>();

                  using var font = new SKFont(Typefaces[typefaceIndex], 64);

                  var enumerator = StringInfo.GetTextElementEnumerator(sampleText);
                  while (enumerator.MoveNext())
                  {
                      var text = enumerator.Current as string;

                      using var bmp = new SKBitmap(64, 64);
                      var image = SKImage.FromBitmap(bmp);

                      using var canvas = new SKCanvas(bmp);
                      canvas.Clear();

                      using var textPaint = new SKPaint(font)
                      {
                          TextSize = 64,
                          IsAntialias = false,
                          SubpixelText = false,
                          HintingLevel = SKPaintHinting.NoHinting,
                          IsAutohinted = false,
                          IsDither = true,
                      };

                      var bounds = new SKRect();
                      textPaint.MeasureText(text, ref bounds);

                      var yOffset = (64 - bounds.Height) / 2;
                      var xOffset = (64 - bounds.Width) / 2;

                      canvas.DrawText(text, xOffset - bounds.Left, yOffset + -bounds.Top, textPaint);

                      var data = new byte[64, 64];
                      for (var y = 0; y < 64; y++)
                      {
                          for (var x = 0; x < 64; x++)
                          {
                              var pixel = bmp.GetPixel(x, y);
                              var a = pixel.Alpha;
                              var r = pixel.Red;
                              data[y, x] = a > 128 ? (byte)1 : (byte)0;
                              bmp.SetPixel(x, y, data[y, x] != 0 ? SKColors.Black : SKColors.Transparent);
                          }
                      }

                      var context = new VectorizationContext(data)
                      {
                          Options = new VectorizationOptions
                          {
                              CurveFitThreshold = CurveFitThreshold,
                              LineFitThreshold = LineFitThreshold,
                              DisableLineFit = DisableLintFit
                          }
                      };
                      new Vectorizer(context).Vectorize();

                      var invalidChars = Path.GetInvalidFileNameChars();
                      var fileName = text;
                      if (text!.Any(x => invalidChars.Contains(x)))
                      {
                          fileName = Guid.NewGuid().ToString();
                      }
                      using var fs = File.Open(Path.Combine(imagesFolder, fileName + ".png"), FileMode.Create, FileAccess.ReadWrite);
                      bmp.Encode(fs, SKEncodedImageFormat.Png, 100);

                      result.Add(SaveImage2(context, imagesFolder));
                  }

                  return result;
              });
            foreach (var file in paths)
            {
                Images.Add(file);
            }
        }

        private string SaveImage2(VectorizationContext context, string imagesFolder)
        {
            using var bmp = new SKBitmap(660, 660);
            DrawGrid(context, bmp);
            DrawBitmap(context, bmp);
            DrawInterpolation(context, bmp);
            DrawVectorOperations(context, bmp);


            var fileName = Guid.NewGuid() + ".png";
            var path = Path.Combine(imagesFolder, fileName);
            using var fs = File.Open(path, FileMode.Create, FileAccess.ReadWrite);
            bmp.Encode(fs, SKEncodedImageFormat.Png, 100);
            return path;
        }

        private void DrawGrid(VectorizationContext context, SKBitmap bmp)
        {
            if (!ShouldShowGrid)
            {
                return;
            }

            using var canvas = new SKCanvas(bmp);
            canvas.Clear(SKColors.White);
            using var paint = new SKPaint
            {
                Color = new SKColor(GridColor.R, GridColor.G, GridColor.B, GridColor.A),
                IsStroke = true
            };

            for (var y = 0; y < bmp.Height; y += 10)
            {
                for (var x = 0; x < bmp.Width; x += 10)
                {
                    var rec = new SKRect(x, y, x + 10, y + 10);
                    canvas.DrawRect(rec, paint);
                }
            }
        }

        private void DrawBitmap(VectorizationContext context, SKBitmap bmp)
        {
            if (!ShouldShowBitmap)
            {
                return;
            }

            using var canvas = new SKCanvas(bmp);

            using var paint = new SKPaint
            {
                Color = new SKColor(BitmapColor.R, BitmapColor.G, BitmapColor.B, BitmapColor.A),
                IsStroke = false
            };

            for (var y = 0; y < context.SourceImage.GetLength(0); y++)
            {
                for (var x = 0; x < context.SourceImage.GetLength(1); x++)
                {
                    if (context.SourceImage[y, x] == 0)
                    {
                        continue;
                    }
                    canvas.DrawRect(10 + x * 10, 10 + y * 10, 10, 10, paint);
                }
            }
        }

        private void DrawInterpolation(VectorizationContext context, SKBitmap bmp)
        {
            if (!ShouldShowInterpolation)
            {
                return;
            }

            using var canvas = new SKCanvas(bmp);

            using var paint2 = new SKPaint
            {
                Color = new SKColor(InterpolationColor.R, InterpolationColor.G, InterpolationColor.B, InterpolationColor.A),
                IsStroke = true
            };


            foreach (var path in context.InterpolatedPaths)
            {
                var imagePath = new SKPath();
                var first = true;
                foreach (var point in path.Points)
                {
                    var x = 10 + (float)point.Point.X * 10;
                    var y = 10 + (float)point.Point.Y * 10;

                    if (first)
                    {
                        imagePath.MoveTo(x, y);
                        first = false;
                    }
                    else
                    {
                        imagePath.LineTo(x, y);
                    }
                    canvas.DrawCircle(x, y, 2, paint2);
                }
                imagePath.Close();
                canvas.DrawPath(imagePath, paint2);
            }
        }

        private void DrawVectorOperations(VectorizationContext context, SKBitmap bmp)
        {
            if (!ShouldShowVector)
            {
                return;
            }

            using var canvas = new SKCanvas(bmp);

            var svgPath = new SKPath
            {
                FillType = SKPathFillType.EvenOdd,
            };

            foreach (var vectorPath in context.ApproximatePaths)
            {
                var first = true;
                foreach (var operation in vectorPath.Operations)
                {
                    if (operation is LineTo lineTo)
                    {
                        if (first)
                        {
                            svgPath.MoveTo(10 + (float)lineTo.From.X * 10, 10 + (float)lineTo.From.Y * 10);
                            first = false;
                        }

                        svgPath.LineTo(10 + (float)lineTo.To.X * 10, 10 + (float)lineTo.To.Y * 10);
                    }
                    else if (operation is CurveTo curveTo)
                    {
                        if (first)
                        {
                            svgPath.MoveTo(10 + (float)curveTo.P0.X * 10, 10 + (float)curveTo.P0.Y * 10);
                            first = false;
                        }

                        svgPath.QuadTo(10 + (float)curveTo.P1.X * 10, 10 + (float)curveTo.P1.Y * 10,
                            10 + (float)curveTo.P2.X * 10, 10 + (float)curveTo.P2.Y * 10);
                    }
                }

                svgPath.Close();
            }

            using var paint = new SKPaint
            {
                Color = new SKColor(VectorColor.R, VectorColor.G, VectorColor.B, VectorColor.A)
            };
            canvas.DrawPath(svgPath, paint);
        }

        private string SaveImage(VectorizationContext context, string folderPath)
        {
            var xml = new XmlDocument();
            var svg = xml.CreateElement("svg", "http://www.w3.org/2000/svg");
            xml.AppendChild(svg);

            var width = context.SourceImage.GetLength(1) * 10.0;
            var hw = width / 2.0;
            var widthAttribute = xml.CreateAttribute("width");
            widthAttribute.Value = width.ToString(CultureInfo.InvariantCulture);
            svg.Attributes.Append(widthAttribute);

            var height = context.SourceImage.GetLength(0) * 10.0;
            var hh = height / 2.0;
            var heightAttribute = xml.CreateAttribute("height");
            heightAttribute.Value = height.ToString(CultureInfo.InvariantCulture);
            svg.Attributes.Append(heightAttribute);

            var viewBoxAttribute = xml.CreateAttribute("viewBox");
            viewBoxAttribute.Value = $"-10 -10 {width + 10} {height + 10}";
            svg.Attributes.Append(viewBoxAttribute);

            SaveApproximateOperations2(context, xml, svg);


            var fileName = Guid.NewGuid() + ".svg";
            var path = Path.Combine(folderPath, fileName);
            xml.Save(path);
            return path;
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

        private void FontComboBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            Fonts.Clear();
            Typefaces.Clear();


            foreach (var file in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Fonts)))
            {
                for (var i = 0; i < 100; i++)
                {
                    var typeface = SKTypeface.FromFile(file, i);
                    if (typeface == null)
                    {
                        break;
                    }
                    Typefaces.Add(typeface);
                    Fonts.Add(typeface.FamilyName + $" {i}");
                }
            }

        }

        private void FontComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontComboBox.SelectedItem is not string fontFamily)
            {
                return;
            }
            if (!string.IsNullOrWhiteSpace(fontFamily))
            {
                TextBoxSampleText.FontFamily = new FontFamily(fontFamily);
            }
        }

        private async void ButtonExecute_OnClick(object sender, RoutedEventArgs e)
        {
            MyProgressRing.IsActive = true;
            ButtonExecute.IsEnabled = false;
            try
            {
                if (string.IsNullOrWhiteSpace(SelectedFont))
                {
                    await ShowErrorAsync("フォントを指定して下さい。");
                    FontComboBox.Focus(FocusState.Programmatic);
                    return;
                }

                if (string.IsNullOrWhiteSpace(SampleText))
                {
                    await ShowErrorAsync("対象の文字列を指定して下さい。");
                    FontComboBox.Focus(FocusState.Programmatic);
                    return;
                }

                await CreateVectorsAsync(SelectedFont, SampleText);
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex.ToString());
            }
            finally
            {
                MyProgressRing.IsActive = false;
                ButtonExecute.IsEnabled = true;
            }
        }

        private async Task ShowErrorAsync(string message)
        {
            var errorDialog = new ContentDialog()
            {
                XamlRoot = MyWindow.Content.XamlRoot,
                Title = "エラー",
                Content = message,
                CloseButtonText = "Ok"
            };

            await errorDialog.ShowAsync();
        }
    }
}

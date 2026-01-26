using System;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using AForge;
using AForge.Imaging.Filters;

namespace Crybot.TradingUtils
{
    public class BitmapAnalyzer
    {
        private readonly Color _bullishSignalColor;
        private readonly Color _bearishSignalColor;
        private static Bitmap _chartImage;

        public BitmapAnalyzer()
        {
            _bullishSignalColor = Color.FromArgb(255, 7, 231, 233);
            _bearishSignalColor = Color.FromArgb(255, 255, 0, 255);
        }

        public async Task<SignalType> GetLastSignal(string url, MarketInfo marketInfo)
        {
            SignalType lastSignal = SignalType.None;
            try
            {
                await DownloadChartImage(url, marketInfo);
                var bullishFilter = GetFilterForBullishSignal();
                var bearishFilter = GetFilterForBearishSignal();
                var bearishImage = GetImageForFilter(bearishFilter);
                var bullishImage = GetImageForFilter(bullishFilter);

                var bullishPixels = GetPixelInfo(bullishImage, _bullishSignalColor);
                var bearishPixels = GetPixelInfo(bearishImage, _bearishSignalColor);
                if (bullishPixels.LastIndex == 0 && bearishPixels.LastIndex == 0)
                    return SignalType.None;
                if (bullishPixels.LastIndex > bearishPixels.LastIndex)
                    lastSignal = SignalType.Bullish;
                else
                    lastSignal = SignalType.Bearish;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return lastSignal;
        }

        private double GetColorDistance(Color e1, Color e2)
        {
            var rmean = (e1.R + (long)e2.R) / 2;
            var r = e1.R - (long)e2.R;
            var g = e1.G - (long)e2.G;
            var b = e1.B - (long)e2.B;
            return Math.Sqrt((((512 + rmean) * r * r) >> 8) + 4 * g * g + (((767 - rmean) * b * b) >> 8));
        }

        private static Bitmap GetImageForFilter(HSLFiltering bearishFilter)
        {
            var bmp = DropOtherColors(bearishFilter);
            return bmp;
        }

        private PixelInfo GetPixelInfo(Bitmap bmp, Color searchedColor)
        {
            var pixelInfo = new PixelInfo();
            var colorArray = new Color[bmp.Height, bmp.Width];
            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    colorArray[i, j] = bmp.GetPixel(j, i);
                }
            }
            int foundPixels = 0;
            for (var i = 0; i < bmp.Height; i++)
            {
                var lineSignals = 0;
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color currentColor = colorArray[i, j];
                    if (currentColor.Name == "ff000000")
                    {
                        continue;
                    }
                    if (ColorMatches(searchedColor, currentColor))
                    {
                        if (j - pixelInfo.LastIndex > 1)
                            lineSignals++;

                        if (j > pixelInfo.LastIndex)
                            pixelInfo.LastIndex = j;
                        foundPixels++;
                    }
                }

                if (lineSignals > 0)
                {
                    foundPixels = lineSignals;
                    break;
                }

            }

            pixelInfo.Count = foundPixels;
            return pixelInfo;
        }

        private bool ColorMatches(Color searchedColor, Color currentColor)
        {
            return currentColor.ToArgb() == searchedColor.ToArgb() || GetColorDistance(currentColor, searchedColor) < 25;
        }

        private static Bitmap DropOtherColors(HSLFiltering filter)
        {
            Bitmap coloredImage = filter.Apply(_chartImage);
            return coloredImage;
        }

        private static async Task DownloadChartImage(string url, MarketInfo marketInfo)
        {
            var httpClient = new HttpClient();
            var streamResponse = await httpClient.GetStreamAsync($"{url}?market={marketInfo.Market}&timeout={Environment.GetEnvironmentVariable("timeout")}&chartUrl={marketInfo.ChartUrl}");
            _chartImage = new Bitmap(streamResponse);
        }

        private static HSLFiltering GetFilterForBearishSignal()
        {
            HSLFiltering filter = new HSLFiltering();
            filter.Saturation = new Range(0.98f, 1f);
            filter.Luminance = new Range(0.48f, 0.52f);
            filter.Hue = new IntRange(290, 310);
            return filter;
        }

        private static HSLFiltering GetFilterForBullishSignal()
        {
            HSLFiltering filter = new HSLFiltering();
            filter.Saturation = new Range(0.85f, 1f);
            filter.Luminance = new Range(0.4f, 0.50f);
            filter.Hue = new IntRange(180, 240);
            return filter;
        }
    }
}

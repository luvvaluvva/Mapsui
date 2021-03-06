using System;
using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Logging;
using Mapsui.Providers;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class RasterRenderer
    {
        public static void Draw(SKCanvas canvas, IViewport viewport, IStyle style, IFeature feature,
            IDictionary<object, BitmapInfo> tileCache, long currentIteration)
        {
            try
            {
                var raster = (IRaster)feature.Geometry;

                BitmapInfo bitmapInfo;

                if (!tileCache.Keys.Contains(raster))
                {
                    bitmapInfo = BitmapHelper.LoadBitmap(raster.Data);
                    tileCache[raster] = bitmapInfo;
                }
                else
                {
                    bitmapInfo = tileCache[raster];
                }

                bitmapInfo.IterationUsed = currentIteration;
                tileCache[raster] = bitmapInfo;
                var destination = WorldToScreen(viewport, feature.Geometry.GetBoundingBox());
                BitmapHelper.RenderRaster(canvas, bitmapInfo.Bitmap, RoundToPixel(destination).ToSkia());
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex.Message, ex);
            }
        }

        private static BoundingBox WorldToScreen(IViewport viewport, BoundingBox boundingBox)
        {
            var first = viewport.WorldToScreen(boundingBox.Min);
            var second = viewport.WorldToScreen(boundingBox.Max);
            return new BoundingBox
            (
                Math.Min(first.X, second.X),
                Math.Min(first.Y, second.Y),
                Math.Max(first.X, second.X),
                Math.Max(first.Y, second.Y)
            );
        }

        private static BoundingBox RoundToPixel(BoundingBox boundingBox)
        {
            return new BoundingBox(
                (float)Math.Round(boundingBox.Left),
                (float)Math.Round(Math.Min(boundingBox.Top, boundingBox.Bottom)),
                (float)Math.Round(boundingBox.Right),
                (float)Math.Round(Math.Max(boundingBox.Top, boundingBox.Bottom)));
        }
    }
}

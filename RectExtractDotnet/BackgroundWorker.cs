using OpenCvSharp;
using System.Threading.Tasks;

namespace RectExtractDotnet
{
    class BackgroundWorker
    {
        public static async Task<Rect> WatershedOn(Mat image, Mat marker)
        {
            Rect rect = new Rect();

            await Task.Run(() =>
            {
                var _marker = marker.Clone();
                image.Watershed(_marker);
                Mat area = _marker.Equals(1);
                rect = area.FindNonZero().BoundingRect();
                _marker.Dispose();
            });

            return rect;
        }
    }
}

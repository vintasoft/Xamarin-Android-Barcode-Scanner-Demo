using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Text;
using Android.Views;

using System.Drawing;

using Vintasoft.XamarinBarcode;
using Vintasoft.XamarinBarcode.BarcodeInfo;
using Color = Android.Graphics.Color;
using Region = Vintasoft.XamarinBarcode.Region;

namespace BarcodeScannerDemo
{
    /// <summary>
    /// The overlay view, which draws information about all barcodes.
    /// </summary>
    public class BarcodeScannerOverlayView : View
    {

        #region Constants

        /// <summary>
        /// The size of cross in screen center.
        /// </summary>
        const int CrossSize = 10;

        #endregion



        #region Fields

        /// <summary>
        /// The barcode scanner.
        /// </summary>
        CameraBarcodeScanner _barcodeScanner;

        /// <summary>
        /// The camera controller.
        /// </summary>
        CameraController _cameraController;


        /// <summary>
        /// The barcodes.
        /// </summary>
        IBarcodeInfo[] _barcodes;

        /// <summary>
        /// The recognized barcode image.
        /// </summary>
        ImageSource _recognizedBarcodeImage;


        /// <summary>
        /// Holds the style and color information about how to draw geometries, text and bitmaps.
        /// </summary>
        Paint _paint;

        /// <summary>
        /// Transparent value step for cross.
        /// </summary>
        int _crossAlphaStep = 16;

        /// <summary>
        /// Transparent value for cross.
        /// </summary>
        int _crossAlpha = 64;

        /// <summary>
        /// The path to represent compound geometry.
        /// </summary>
        Path _path = new Path();

        /// <summary>
        /// The canvas transform.
        /// </summary>
        Matrix _canvasTransform = null;


        /// <summary>
        /// The "Click for detailed information" message.
        /// </summary>
        string _clickForDetailedInfoMessage = null;

        /// <summary>
        /// Drawing text size in pixels.
        /// </summary>
        float _textSizeInPixels;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instanse of <see cref="BarcodeScannerOverlayView"/> class.
        /// </summary>
        /// <param name="context">A context.</param>
        /// <param name="cameraController">A camera controller.</param>
        /// <param name="barcodeScanner">A barcode scanner.</param>
        internal BarcodeScannerOverlayView(Context context, CameraController cameraController, CameraBarcodeScanner barcodeScanner)
            : base(context)
        {
            // create new instanse of paint class 
            _paint = new Paint(PaintFlags.AntiAlias);
            // set camera controller
            _cameraController = cameraController;
            // set barcode scanner
            _barcodeScanner = barcodeScanner;
            // subscribe to framer scan finished event
            _barcodeScanner.FrameScanFinished += BarcodeScanner_FrameScanFinished;
            // get message from resource
            _clickForDetailedInfoMessage = Resources.GetString(Resource.String.click_for_detailed_info_message);
            // get text size from resource
            _textSizeInPixels = Resources.GetDimensionPixelSize(Resource.Dimension.drawingFontSize);
        }

        #endregion



        #region Properties

        IBarcodeInfo _recognizedBarcode;
        /// <summary>
        /// Gets the recognized barcode.
        /// </summary>
        internal IBarcodeInfo RecognizedBarcode
        {
            get
            {
                return _recognizedBarcode;
            }
        }

        RectF _scanRect = new RectF();
        /// <summary>
        /// Gets the scanning rectangle.
        /// </summary>
        internal RectF ScanRect
        {
            get
            {
                return _scanRect;
            }
        }

        RectF _transformedScanRect = new RectF();
        /// <summary>
        /// Gets the scan rectangle, which is transformed using canvas transformation.
        /// </summary>
        internal RectF TransformedScanRect
        {
            get
            {
                // get source scan rectangle
                _transformedScanRect.Set(_scanRect);

                // if transform is not empty
                if (_canvasTransform != null)
                {
                    // transform rectangle
                    _canvasTransform.MapRect(_transformedScanRect);
                }

                return _transformedScanRect;
            }
        }

        bool _autofocusing = false;
        /// <summary>
        /// Gets or sets a value which indicates whether the camera is autofocusing now.
        /// </summary>
        internal bool Autofocusing
        {
            get
            {
                return _autofocusing;
            }
            set
            {
                _autofocusing = value;
            }
        }

        bool _autofocusSuccess = false;
        /// <summary>
        /// Gets or sets a value which indicates whether the camera was autofocused successfully.
        /// </summary>
        internal bool AutofocusSuccess
        {
            get
            {
                return _autofocusSuccess;
            }
            set
            {
                _autofocusSuccess = value;
            }
        }

        #endregion



        #region Methods

        #region PROTECTED

        /// <summary>
        /// Draws all scanning information.
        /// </summary>
        /// <param name="canvas">A canvas.</param>
        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            if (_barcodeScanner == null)
                return;

            int frameWidth = _cameraController.CapturedPreviewFrameSize.Width;
            int frameHeight = _cameraController.CapturedPreviewFrameSize.Height;

            Rectangle scanRect = _barcodeScanner.ScannerSettings.ScanRectangle;

            // get the camera preview size
            int width = _cameraController.PreviewSurfaceSize.Width;
            int height = _cameraController.PreviewSurfaceSize.Height;
            if (width == 0 || height == 0)
            {
                width = canvas.Width;
                height = canvas.Height;
            }

            // get the scale parameters
            float scaleX = width / (float)frameWidth;
            float scaleY = height / (float)frameHeight;

            // modify canvas transformation
            canvas.Save();

            using (Matrix m = new Matrix())
            {
                m.PostScale(1, 1, width / 2, height / 2);
                canvas.Concat(m);
            }
            canvas.Scale(scaleX, scaleY);
            _canvasTransform = canvas.Matrix;

            // get recognized barcodes
            IBarcodeInfo recognizedBarcode = _recognizedBarcode;
            ImageSource recognizedBarcodeImage = null;
            if (recognizedBarcode != null)
                recognizedBarcodeImage = _recognizedBarcodeImage;
            if (recognizedBarcodeImage == null)
                recognizedBarcode = null;
            // get all barcodes
            IBarcodeInfo[] barcodes = _barcodes;


            _paint.StrokeWidth = 1 / scaleX;
            // if recognized barcode is not empty
            // and recognized barcode image is not empty
            if (recognizedBarcode != null && recognizedBarcodeImage != null)
            {
                _paint.Alpha = 255;
                AndroidBitmapSource androidBitmap = new AndroidBitmapSource((GrayscaleImageSource)recognizedBarcodeImage);
                using (Bitmap bitmap = androidBitmap.GetBitmapAndDispose())
                {
                    // draw recognized barcode image 
                    canvas.DrawBitmap(bitmap, null, new RectF(0, 0, recognizedBarcodeImage.Width, recognizedBarcodeImage.Height), _paint);
                }

                if (recognizedBarcode.Region != null)
                {
                    _paint.Color = Color.Black;
                    _paint.Alpha = 64;
                    // fill the scan rectangle using transparent gray color
                    FillInvertedRect(canvas, _paint, frameWidth, frameHeight, recognizedBarcode.Region.Rectangle);
                }

                // draw message "click for detailed information" inside scan rectangle
                DrawText(canvas, scanRect, _clickForDetailedInfoMessage, _textSizeInPixels / scaleX, Color.Orange, 255, false);
            }

            if (scanRect.Width == 0 || scanRect.Height == 0)
                scanRect = new Rectangle(0, 0, _cameraController.CapturedPreviewFrameSize.Width, _cameraController.CapturedPreviewFrameSize.Height);

            if (scanRect.Width > 0 && scanRect.Height > 0)
            {
                _paint.Color = Color.Black;
                _paint.Alpha = 128;
                // fill the area around scan rectangle using transparent gray color
                FillInvertedRect(canvas, _paint, frameWidth, frameHeight, scanRect);
            }

            // if recognized barcode is not empty
            if (recognizedBarcode != null)
            {
                // draw barcode info
                DrawBarcodeInfo(canvas, _paint, recognizedBarcode, scaleX);
            }
            else
            {
                // if barcode array is not empty
                if (barcodes != null && barcodes.Length > 0)
                {
                    // for each barcode
                    foreach (IBarcodeInfo barcodeInfo in barcodes)
                        // draw barcode info
                        DrawBarcodeInfo(canvas, _paint, barcodeInfo, scaleX);
                }
            }

            bool hasRecognizedBarcode = recognizedBarcode != null;
            if (!hasRecognizedBarcode && barcodes != null)
            {
                foreach (IBarcodeInfo barcodeInfo in barcodes)
                {
                    if (barcodeInfo.Confidence >= 95 || barcodeInfo.Confidence == -1)
                        hasRecognizedBarcode = true;
                }
            }

            // get size of cross in center of the screen
            float crossSize = CrossSize / scaleX;

            // if recognized barcode is empty
            // and barcode array is not empty
            if (recognizedBarcode == null && barcodes != null)
            {
                DrawLineWithCross(canvas, scanRect, scaleX, crossSize);
            }

            // draw border around scan rectangle
            DrawScanRectangleBorder(canvas, scanRect, scaleX, barcodes, hasRecognizedBarcode);

            canvas.Restore();
        }

        #endregion


        #region INTERNAL

        /// <summary>
        /// Returns a value, which determines whether point lies on recognize surface.
        /// </summary>
        /// <param name="x">Point X coordinate.</param>
        /// <param name="y">Point Y coordinate.</param>
        /// <returns>
        /// <b>true</b> - if point on recognize surface; otherwise - <b>false</b>.
        /// </returns>
        internal bool GetIsPointOnRecognizeSurface(float x, float y)
        {
            return TransformedScanRect.Contains(x, y);
        }

        /// <summary>
        /// Sets the recognized barcode.
        /// </summary>
        /// <param name="barcode">The barcode.</param>
        /// <param name="barcodeImage">The barcode image.</param>
        internal void SetRecognizedBarcode(IBarcodeInfo barcode, ImageSource barcodeImage)
        {
            if (barcode == null || _recognizedBarcode != barcode)
            {
                _recognizedBarcode = barcode;
                _recognizedBarcodeImage = barcodeImage;
                _barcodes = null;
                Invalidate();
            }
        }

        /// <summary>
        /// Removes recognized barcode.
        /// </summary>
        internal void RemoveRecognizedBarcode()
        {
            _recognizedBarcode = null;
            _recognizedBarcodeImage = null;
            _barcodes = null;
            Invalidate();
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Scan is finished.
        /// </summary>
        private void BarcodeScanner_FrameScanFinished(object sender, FrameScanFinishedEventArgs e)
        {
            ((Activity)Context).RunOnUiThread(() =>
            {
                OnFrameScanFinished(e);
            });
        }

        /// <summary>
        /// Frame scanning is finished.
        /// </summary>
        private void OnFrameScanFinished(FrameScanFinishedEventArgs e)
        {
            bool needInvalidate = _barcodes == null || _barcodes.Length > 0 || e.FoundBarcodes.Length > 0;
            _barcodes = e.FoundBarcodes;
            if (needInvalidate)
                Invalidate();
            else
                Invalidate(GetCrossRect());
        }


        #region Drawing

        /// <summary>
        /// Draws line with cross in center.
        /// </summary>
        /// <param name="canvas">A canvas.</param>
        /// <param name="scanRect">A scan rectangle.</param>
        /// <param name="scale">A scale parameter.</param>
        /// <param name="crossSize">A cross size.</param>
        private void DrawLineWithCross(Canvas canvas, Rectangle scanRect, float scale, float crossSize)
        {
            // draw red cross line through scan rectangle
            _paint.StrokeWidth = 2 / scale;
            canvas.Translate(scanRect.X, scanRect.Y);
            _paint.Color = Color.Red;
            _paint.Alpha = 128;
            canvas.DrawLine(0, scanRect.Height / 2, scanRect.Width / 2 - crossSize * 2, scanRect.Height / 2, _paint);
            canvas.DrawLine(scanRect.Width / 2 + crossSize * 2, scanRect.Height / 2, scanRect.Width, scanRect.Height / 2, _paint);

            // draw cross in center of the screen
            _paint.StrokeWidth = 3 / scale;
            if (Autofocusing)
                _paint.Color = Color.Yellow;
            else if (AutofocusSuccess)
                _paint.Color = Color.Lime;
            else
                _paint.Color = Color.Red;
            if (_crossAlpha + _crossAlphaStep > 255 || _crossAlpha + _crossAlphaStep < 64)
                _crossAlphaStep = -_crossAlphaStep;
            _crossAlpha += _crossAlphaStep;
            _paint.Alpha = _crossAlpha;
            canvas.DrawLine(scanRect.Width / 2 - crossSize, scanRect.Height / 2, scanRect.Width / 2 + crossSize, scanRect.Height / 2, _paint);
            canvas.DrawLine(scanRect.Width / 2, scanRect.Height / 2 - crossSize, scanRect.Width / 2, scanRect.Height / 2 + crossSize, _paint);

            canvas.Translate(-scanRect.X, -scanRect.Y);
        }

        /// <summary>
        /// Draws border around scan rectangle.
        /// </summary>
        /// <param name="canvas">A canvas.</param>
        /// <param name="scanRect">A scan rectangle.</param>
        /// <param name="scale">A scale parameter.</param>
        /// <param name="barcodes">A barcode list.</param>
        /// <param name="hasRecognizedBarcode">Indicates whether view has recognized barcode.</param>
        private void DrawScanRectangleBorder(Canvas canvas, Rectangle scanRect, float scale, IBarcodeInfo[] barcodes, bool hasRecognizedBarcode)
        {
            // if barcode list is empty and there is no recognized barcode
            if (barcodes == null && !hasRecognizedBarcode)
            {
                _paint.StrokeWidth = 1 / scale;
                _paint.Color = Color.Red;
            }
            // if there is no recognized barcode
            else if (!hasRecognizedBarcode)
            {
                _paint.StrokeWidth = 2 / scale;
                _paint.Color = Color.Lime;
            }
            // if there is recognized barcode
            else
            {
                _paint.StrokeWidth = 5 / scale;
                _paint.Color = Color.Lime;
            }
            _paint.Alpha = 128;
            _paint.SetStyle(Paint.Style.Stroke);
            _scanRect.Set(scanRect.Left, scanRect.Top, scanRect.Right, scanRect.Bottom);
            // draw rectangle
            canvas.DrawRect(_scanRect, _paint);
            _paint.SetStyle(Paint.Style.Fill);
        }

        /// <summary>
        /// Draws text.
        /// </summary>
        /// <param name="canvas">A canvas.</param>
        /// <param name="rect">A draw bounding rectangle.</param>
        /// <param name="text">A text to draw.</param>
        /// <param name="size">A text size.</param>
        /// <param name="color">A text color.</param>
        /// <param name="alpha">A text alpha.</param>
        /// <param name="verticalCentering">A value that indicates whether text should be centered vertically.</param>
        private static void DrawText(Canvas canvas, Rectangle rect, string text, float size, Color color, int alpha, bool verticalCentering)
        {
            canvas.Save();

            TextPaint textPaint = new TextPaint();
            textPaint.Color = color;
            textPaint.Alpha = alpha;
            textPaint.AntiAlias = true;
            textPaint.TextSize = size;
            using (StaticLayout textLayout = new StaticLayout(text, textPaint, rect.Width, Android.Text.Layout.Alignment.AlignCenter, 1.0f, 0.0f, false))
            {
                if (verticalCentering)
                    canvas.Translate(rect.X, rect.Y + (rect.Height - textLayout.Height) / 2);
                else
                    canvas.Translate(rect.X, rect.Y + rect.Height - textLayout.Height);

                textLayout.Draw(canvas);
            }
            canvas.Restore();
        }

        /// <summary>
        /// Returns a bounding rectangle of cross in center of screen.
        /// </summary>
        /// <returns>A cross bounding rectangle.</returns>
        private Rect GetCrossRect()
        {
            float scaleX = Width / (float)_cameraController.CapturedPreviewFrameSize.Width;
            float scaleY = Height / (float)_cameraController.CapturedPreviewFrameSize.Height;
            Rectangle rect = _barcodeScanner.ScannerSettings.ScanRectangle;
            if (rect.Width == 0 || rect.Height == 0)
                rect = new Rectangle(0, 0, _cameraController.CapturedPreviewFrameSize.Width, _cameraController.CapturedPreviewFrameSize.Height);
            return new Rect(
                (int)((rect.X + rect.Width / 2 - CrossSize) * scaleX), (int)((rect.Y + rect.Height / 2 - CrossSize) * scaleY),
                (int)((rect.X + rect.Width / 2 + CrossSize) * scaleX), (int)((rect.Y + rect.Height / 2 + CrossSize) * scaleY));
        }

        /// <summary>
        /// Fills inverted rectangle.
        /// </summary>
        /// <param name="canvas">A canvas.</param>
        /// <param name="paint">A paint information.</param>
        /// <param name="width">A width.</param>
        /// <param name="height">A height.</param>
        /// <param name="rect">A rectangle.</param>
        private void FillInvertedRect(Canvas canvas, Paint paint, int width, int height, Rectangle rect)
        {
            canvas.DrawRect(0, 0, width, rect.Top, paint);
            canvas.DrawRect(0, rect.Top, rect.Left, rect.Bottom, paint);
            canvas.DrawRect(rect.Right, rect.Top, width, rect.Bottom, paint);
            canvas.DrawRect(0, rect.Bottom, width, height, paint);
        }

        /// <summary>
        /// Draws the barcode information.
        /// </summary>
        /// <param name="canvas">A canvas.</param>
        /// <param name="paint">A paint information.</param>
        /// <param name="barcodeInfo">A barcode.</param>
        /// <param name="scale">A scale value.</param>
        private void DrawBarcodeInfo(Canvas canvas, Paint paint, IBarcodeInfo barcodeInfo, float scale)
        {
            int alpha;
            // get barcode region
            Region barcodeRegion = barcodeInfo.Region;
            if (barcodeRegion == null)
                return;

            // determines whether barcode region should be filled
            bool fill;
            // if confidence of barcode recognition is "-1"
            if (barcodeInfo.Confidence == -1)
            {
                alpha = 48;
                paint.Color = Color.LimeGreen;
                fill = true;
            }
            // if confidence of barcode recognition is no less than "95"
            else if (barcodeInfo.Confidence >= 95)
            {
                alpha = 64;
                paint.Color = Color.LimeGreen;
                fill = true;
            }
            // if confidence of barcode recognition between 0 and 95
            else
            {
                paint.Color = Color.Red;
                BarcodeInfo2D barcodeInfo2D = barcodeInfo as BarcodeInfo2D;
                if (barcodeInfo2D != null && barcodeInfo2D.GetReferencePoints().Length > 0)
                {
                    alpha = 64;
                    fill = false;
                }
                else
                {
                    alpha = 32;
                    fill = true;
                }
            }
            // if barcode region should be filled
            if (fill)
            {
                paint.Alpha = alpha;
                // fill barcode region
                FillRegion(canvas, barcodeInfo.Region, paint);
            }
            paint.Alpha = alpha * 3;
            // if confidence of barcode recognition is no less than "95"
            if (barcodeInfo.Confidence >= 95)
                paint.Color = Color.Orange;
            // draw barcode reference points
            DrawBarcodeReferencePoints(canvas, barcodeInfo, paint);

            // if need draw barcode type
            if (barcodeInfo.BarcodeInfoClass == BarcodeInfoClass.Barcode2D || barcodeInfo.Confidence == -1 || barcodeInfo.Confidence >= 95)
            {
                // get barcode type name
                string text = Utils.GetBarcodeTypeString(barcodeInfo);

                // draw barcode type name
                DrawText(canvas, barcodeInfo.Region.Rectangle, text, _textSizeInPixels / scale, paint.Color, 255, true);
            }
        }

        /// <summary>
        /// Draws barcode referenced points.
        /// </summary>
        /// <param name="canvas">The canvas.</param>
        /// <param name="barcodeInfo">The barcode.</param>
        /// <param name="paint">The paint information.</param>
        private void DrawBarcodeReferencePoints(Canvas canvas, IBarcodeInfo barcodeInfo, Paint paint)
        {
            BarcodeInfo2D barcodeInfo2D = barcodeInfo as BarcodeInfo2D;
            // if barcode is 2D barcode
            if (barcodeInfo2D != null)
            {
                float cellSize = (float)(barcodeInfo2D.CellWidth + barcodeInfo2D.CellHeight) / 2;
                if (cellSize > 15)
                    cellSize = 15;
                foreach (System.Drawing.PointF point in barcodeInfo2D.GetReferencePoints())
                    canvas.DrawCircle(point.X, point.Y, cellSize / 2, paint);
            }
        }

        /// <summary>
        /// Fills region.
        /// </summary>
        /// <param name="canvas">The canvas.</param>
        /// <param name="region">The region to draw.</param>
        /// <param name="paint">The paint information.</param>
        private void FillRegion(Canvas canvas, Region region, Paint paint)
        {
            _path.Reset();
            _path.MoveTo(region.LeftTop.X, region.LeftTop.Y);
            _path.LineTo(region.RightTop.X, region.RightTop.Y);
            _path.LineTo(region.RightBottom.X, region.RightBottom.Y);
            _path.LineTo(region.LeftBottom.X, region.LeftBottom.Y);
            _path.Close();
            canvas.DrawPath(_path, paint);
        }

        #endregion

        #endregion

        #endregion

    }
}
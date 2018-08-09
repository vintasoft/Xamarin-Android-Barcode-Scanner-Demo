using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Views;

using System;
using System.Drawing;

using Vintasoft.XamarinBarcode;

using static Android.Views.ViewGroup;
using Camera = Android.Hardware.Camera;

namespace BarcodeScannerDemo
{
    /// <summary>
    /// The surface view, which shows the camera preview.
    /// </summary>
    public class BarcodeScannerSurfaceView : SurfaceView, ISurfaceHolderCallback
    {

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
        /// Indicates that callback for holder is added.
        /// </summary>
        bool _addedHolderCallback = false;

        /// <summary>
        /// The barcode scanner overlay view.
        /// </summary>
        BarcodeScannerOverlayView _barcodeScannerOveraly;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="BarcodeScannerSurfaceView"/> class.
        /// </summary>
        /// <param name="context">A context.</param>
        /// <param name="cameraController">A camera controller.</param>
        /// <param name="barcodeScanner">A barcode scanner.</param>
        /// <param name="barcodeScannerOveraly">A barcode scanner overlay view.</param>
        internal BarcodeScannerSurfaceView(Context context, CameraController cameraController, CameraBarcodeScanner barcodeScanner, BarcodeScannerOverlayView barcodeScannerOveraly)
            : base(context)
        {
            _barcodeScanner = barcodeScanner;
            _barcodeScannerOveraly = barcodeScannerOveraly;
            _cameraController = cameraController;
            Init();
        }

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// The surface is changed.
        /// </summary>
        /// <param name="holder">A surface holder.</param>
        /// <param name="format">A new pixel format of the surface.</param>
        /// <param name="width">A surface width.</param>
        /// <param name="height">A surface height.</param>
        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
        {
            int displayWidth;
            int displayHeight;
            Activity activity = Context as Activity;

            // if activity is not empty
            if (activity != null)
            {
                // get display size
                Android.Util.DisplayMetrics displayMetrics = new Android.Util.DisplayMetrics();
                activity.WindowManager.DefaultDisplay.GetMetrics(displayMetrics);
                displayWidth = displayMetrics.WidthPixels;
                displayHeight = displayMetrics.HeightPixels;
            }
            // if activity is empty
            else
            {
                // get width
                if (_barcodeScannerOveraly.Width < width)
                    displayWidth = _barcodeScannerOveraly.Width;
                else
                    displayWidth = width;

                // get height
                if (_barcodeScannerOveraly.Height < height)
                    displayHeight = _barcodeScannerOveraly.Height;
                else
                    displayHeight = height;
            }

            SurfaceCreated(holder, displayWidth, displayHeight);
        }

        /// <summary>
        /// The surface is created.
        /// </summary>
        /// <param name="holder">A surface holder.</param>
        public void SurfaceCreated(ISurfaceHolder holder)
        {
            SurfaceCreated(holder, Width, Height);
        }

        /// <summary>
        /// The surface is created.
        /// </summary>
        /// <param name="holder">A surface holder.</param>
        /// <param name="width">A surface width.</param>
        /// <param name="height">A surface height.</param>
        public void SurfaceCreated(ISurfaceHolder holder, int width, int height)
        {
            try
            {
                // apply camera settings
                _cameraController.ApplyCameraSettings();

                // get preview frame size
                System.Drawing.Size previewFrameSize = _cameraController.CapturedPreviewFrameSize;
                // get layout parameters
                LayoutParams lp = LayoutParameters;
                // get camera size ratio
                float cameraWH = previewFrameSize.Width / (float)previewFrameSize.Height;
                // get surface size ratio
                float surfaceWH = width / (float)height;
                // if camera size ratio is greater than surface size ratio
                if (cameraWH > surfaceWH)
                {
                    // set new layout width
                    lp.Width = (int)Math.Round(width * cameraWH / surfaceWH);
                    lp.Height = height;
                }
                else
                {
                    lp.Width = width;
                    // set new layout height
                    lp.Height = (int)Math.Round(height * surfaceWH / cameraWH);
                }
                // set new layout parameters
                LayoutParameters = lp;

                // set preview surface parametrs
                _cameraController.SetPreviewSurfaceParams(new System.Drawing.Size(lp.Width, lp.Height), new System.Drawing.Size(width, height));

                // start camera preview
                StartCamera();
            }
            catch (Java.Lang.RuntimeException)
            {
                Android.Util.Log.Error("CAMERA_PARAMS_ERROR", "Failed to set camera parameters");
            }
        }

        /// <summary>
        /// The surface is destroyed.
        /// </summary>
        /// <param name="holder">The surface holder.</param>
        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            // stop camera preview
            StopCamera();
        }

        #endregion


        #region PROTECTED

        /// <summary>
        /// Surface view size is changed.
        /// </summary>
        /// <param name="newWidth">The new width.</param>
        /// <param name="newHeight">The new height.</param>
        /// <param name="oldWidth">The old width.</param>
        /// <param name="oldHeight">The old height.</param>
        protected override void OnSizeChanged(int newWidth, int newHeight, int oldWidth, int oldHeight)
        {
            base.OnSizeChanged(newWidth, newHeight, oldWidth, oldHeight);
        }

        /// <summary>
        /// View is attached to the window.
        /// </summary>
        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();

            Init();
        }

        /// <summary>
        /// Window visibility is changed.
        /// </summary>
        /// <param name="visibility">A visibility value.</param>
        protected override void OnWindowVisibilityChanged(ViewStates visibility)
        {
            base.OnWindowVisibilityChanged(visibility);
            if (visibility == ViewStates.Visible)
                Init();
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Stops camera preview.
        /// </summary>
        private void StopCamera()
        {
            if (_cameraController.Camera != null)
                _cameraController.Camera.StopPreview();
        }

        /// <summary>
        /// Starts camera preview.
        /// </summary>
        /// <exception cref="Exception">Thrown if <see cref="CameraController.Camera"/> is <b>null</b>.</exception>
        private void StartCamera()
        {
            // if camera is not empty
            if (_cameraController.Camera != null)
            {
                // get border size
                int border = _cameraController.CapturedPreviewFrameVisibleSize.Width / 8;
                // set scan rectangle
                _barcodeScanner.ScannerSettings.ScanRectangle = new Rectangle(
                    border, border, _cameraController.CapturedPreviewFrameVisibleSize.Width - border * 2, _cameraController.CapturedPreviewFrameVisibleSize.Height - border * 2);

                // set surface for live preview to camera
                _cameraController.Camera.SetPreviewDisplay(Holder);
                // start camera preview
                _cameraController.StartPreview();
            }
            else
            {
                throw new Exception("StartCamera");
            }
        }

        /// <summary>
        /// Initializes holder callback.
        /// </summary>
        private void Init()
        {
            if (!_addedHolderCallback)
            {
                Holder.AddCallback(this);
                _addedHolderCallback = true;
            }
        }

        #endregion

        #endregion

    }
}
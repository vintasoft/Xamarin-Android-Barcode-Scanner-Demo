using Android.Hardware;
using Android.OS;

using System;
using System.Collections.Generic;
using System.Drawing;

namespace BarcodeScannerDemo
{
    /// <summary>
    /// The class that controls camera.
    /// </summary>
    public class CameraController : Java.Lang.Object, Camera.IAutoFocusCallback
    {

        #region Constants

        /// <summary>
        /// A limit value for camera preview width autodetection.
        /// </summary>
        const int AUTO_CAMERA_PREVIEW_WIDTH_LIMIT = 1280;

        /// <summary>
        /// A limit value for camera preview height autodetection.
        /// </summary>
        const int AUTO_CAMERA_PREVIEW_HEIGHT_LIMIT = 800;

        /// <summary>
        /// Determines whether single buffer should be allocated to use in camera preview.
        /// </summary>
        internal const bool USE_PREVIEW_BUFFER = false;

        #endregion



        #region Fields

        /// <summary>
        /// Represents the method that is used to deliver copies of preview frames as they displayed.
        /// </summary>
        Camera.IPreviewCallback _previewCallbackDelegate;

        #endregion



        #region Constructor

        /// <summary>
        /// Initializes a new instance of <see cref="CameraController"/> class.
        /// </summary>
        /// <param name="previewCallback">A preview callback method.</param>
        internal CameraController(Camera.IPreviewCallback previewCallback)
        {
            _previewCallbackDelegate = previewCallback;
        }

        #endregion



        #region Properties

        BarcodeScannerOverlayView _barcodeOveralyView;
        /// <summary>
        /// Gets or sets the barcode scanner overlay view.
        /// </summary>
        internal BarcodeScannerOverlayView BarcodeOveralyView
        {
            get
            {
                return _barcodeOveralyView;
            }
            set
            {
                _barcodeOveralyView = value;
            }
        }

        Camera _camera;
        /// <summary>
        /// Gets the camera.
        /// </summary>
        internal Camera Camera
        {
            get
            {
                return _camera;
            }
        }


        IList<Camera.Size> _cameraPreviewSizes = null;
        /// <summary>
        /// Gets a list with supported camera preview sizes.
        /// </summary>
        internal IList<Camera.Size> CameraPreviewSizes
        {
            get
            {
                if (_camera != null && _cameraPreviewSizes == null)
                {
                    Camera.Parameters parameters = _camera.GetParameters();
                    _cameraPreviewSizes = parameters.SupportedPreviewSizes;
                }
                return _cameraPreviewSizes;
            }
        }

        Camera.Size _cameraPreviewSize = null;
        /// <summary>
        /// Gets the current camera preview size.
        /// </summary>
        internal Camera.Size СameraPreviewSize
        {
            get
            {
                return _cameraPreviewSize;
            }
        }

        Size _capturedPreviewFrameSize = new Size(0, 0);
        /// <summary>
        /// Gets the size of preview frame, which is captured from camera.
        /// </summary>
        internal Size CapturedPreviewFrameSize
        {
            get
            {
                return _capturedPreviewFrameSize;
            }
        }

        Size _capturedPreviewFrameVisibleSize = new Size(0, 0);
        /// <summary>
        /// Gets the visible size of captured frame, which is captured from camera.
        /// </summary>
        internal Size CapturedPreviewFrameVisibleSize
        {
            get
            {
                return _capturedPreviewFrameVisibleSize;
            }
        }


        Size _previewSurfaceSize = new Size(0, 0);
        /// <summary>
        /// Gets the size of the surface, which is used to preview the captured frames.
        /// </summary>
        internal Size PreviewSurfaceSize
        {
            get
            {
                return _previewSurfaceSize;
            }
        }

        Size _previewVisibleSurfaceSize = new Size(0, 0);
        /// <summary>
        /// Gets the visible size of the surface, which is used to preview the captured frames.
        /// </summary>
        internal Size PreviewVisibleSurfaceSize
        {
            get
            {
                return _previewVisibleSurfaceSize;
            }
        }


        bool _autofocus = true;
        /// <summary>
        /// Gets or sets a value which indicates that autofocus is enabled.
        /// </summary>
        internal bool Autofocus
        {
            get
            {
                return _autofocus;
            }
            set
            {
                _autofocus = value;
            }
        }

        bool _continuousAutofocus = true;
        /// <summary>
        /// Gets or sets a value which indicates that continuous autofocus is enabled.
        /// </summary>
        internal bool ContinuousAutofocus
        {
            get
            {
                return _continuousAutofocus;
            }
            set
            {
                _continuousAutofocus = value;
            }
        }

        bool _autofocusOnTouch = true;
        /// <summary>
        /// Gets or sets a value which indicates that autofocus on touch is enabled.
        /// </summary>
        internal bool AutofocusOnTouch
        {
            get
            {
                return _autofocusOnTouch;
            }
            set
            {
                _autofocusOnTouch = value;
            }
        }

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Auto focus is completed.
        /// </summary>
        /// <param name="success">Indicates, whether focus is successful.</param>
        /// <param name="camera">A camera.</param>
        public void OnAutoFocus(bool success, Camera camera)
        {
            if (BarcodeOveralyView != null)
            {
                BarcodeOveralyView.Autofocusing = false;
                BarcodeOveralyView.AutofocusSuccess = success;
            }
        }

        #endregion


        #region PROTECTED

        /// <summary>
        /// Raises the <see cref="PreviewSurfaceParamsIsChanged"/> event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnPreviewSurfaceParamsIsChanged(EventArgs e)
        {
            if (PreviewSurfaceParamsIsChanged != null)
                PreviewSurfaceParamsIsChanged(this, e);
        }

        #endregion


        #region INTERNAL

        /// <summary>
        /// Requests an auto focus.
        /// </summary>
        internal void RequestAutoFocus()
        {
            if (Camera == null)
                return;

            if (!Autofocus)
                return;

            try
            {
                BarcodeOveralyView.Autofocusing = true;
                Camera.CancelAutoFocus();
                Camera.AutoFocus(this);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Sets the preview surface parameters.
        /// </summary>
        /// <param name="previewSurfaceSize">A preview surface size.</param>
        /// <param name="previewVisibleSurfaceSize">A preview visible surface size.</param>
        internal void SetPreviewSurfaceParams(Size previewSurfaceSize, Size previewVisibleSurfaceSize)
        {
            _previewSurfaceSize = previewSurfaceSize;
            _previewVisibleSurfaceSize = previewVisibleSurfaceSize;

            _capturedPreviewFrameVisibleSize = new Size(
                (int)Math.Round(_previewVisibleSurfaceSize.Width * (float)CapturedPreviewFrameSize.Width / PreviewSurfaceSize.Width),
                (int)Math.Round(_previewVisibleSurfaceSize.Height * (float)CapturedPreviewFrameSize.Height / PreviewSurfaceSize.Height));

            OnPreviewSurfaceParamsIsChanged(new EventArgs());
        }

        /// <summary>
        /// Opens the camera.
        /// </summary>
        internal void OpenCamera()
        {
            // get number of cameras
            int cameraCount = Camera.NumberOfCameras;
            // determines whether camera is found
            bool isCameraFound = false;

            Camera.CameraInfo camInfo = new Camera.CameraInfo();
            // for each camera
            for (int i = 0; i < cameraCount; i++)
            {
                // get camera information
                Camera.GetCameraInfo(i, camInfo);
                // if camera is back camera
                if (camInfo.Facing == CameraFacing.Back)
                {
                    // open camera
                    _camera = Camera.Open(i);
                    // indicate that camera is found
                    isCameraFound = true;
                    break;
                }
            }

            // if camera is not found
            if (!isCameraFound)
            {
                // open the first camera
                _camera = Camera.Open(0);
            }
        }

        /// <summary>
        /// Applies settings to camera.
        /// </summary>
        internal void ApplyCameraSettings()
        {
            // if camera is not empty
            if (_camera != null)
            {
                // get camera parameters
                Camera.Parameters parameters = _camera.GetParameters();
                
                // get supported preview sizes
                IList<Camera.Size> sizes = parameters.SupportedPreviewSizes;
                
                // set preview color format
                parameters.PreviewFormat = Android.Graphics.ImageFormatType.Nv21;

                // get supported focus modes
                IList<string> supportedFocusModes = parameters.SupportedFocusModes;
                // if autofocus is enabled
                if (Autofocus)
                {
                    // set focus mode auto
                    if (supportedFocusModes.Contains(Camera.Parameters.FocusModeAuto))
                        parameters.FocusMode = Camera.Parameters.FocusModeAuto;
                }
                else
                {
                    if (supportedFocusModes.Contains(Camera.Parameters.FocusModeMacro))
                        parameters.FocusMode = Camera.Parameters.FocusModeMacro;
                    if (supportedFocusModes.Contains(Camera.Parameters.FocusModeEdof))
                        parameters.FocusMode = Camera.Parameters.FocusModeEdof;
                }

                if (parameters.SupportedPreviewFpsRange != null && parameters.SupportedPreviewFpsRange.Count > 0)
                {
                    int[] selectedFps = parameters.SupportedPreviewFpsRange[0];
                    // this will make sure we select a range with the lowest minimum FPS
                    // and maximum FPS which still has the lowest minimum
                    // This should help maximize performance / support for hardware
                    foreach (int[] fpsRange in parameters.SupportedPreviewFpsRange)
                    {
                        if (fpsRange[0] <= selectedFps[0] && fpsRange[1] > selectedFps[1])
                            selectedFps = fpsRange;
                    }
                    // set the preview FPS range
                    parameters.SetPreviewFpsRange(selectedFps[0], selectedFps[1]);
                }

                // get current camera preview size
                Camera.Size selectedPreviewSize = parameters.PreviewSize;
                // if camera preview size is empty
                if (_cameraPreviewSize == null)
                {
                    // for each supported preview size
                    foreach (Camera.Size previewSize in parameters.SupportedPreviewSizes)
                    {
                        // search the maximum preview size which width is not greater than AUTO_CAMERA_PREVIEW_WIDTH_LIMIT constant and
                        // height is not greater than AUTO_CAMERA_PREVIEW_HEIGHT_LIMIT constant

                        if (previewSize.Width > AUTO_CAMERA_PREVIEW_WIDTH_LIMIT &&
                            previewSize.Height > AUTO_CAMERA_PREVIEW_HEIGHT_LIMIT)
                        {
                            continue;
                        }

                        if ((selectedPreviewSize.Width <= previewSize.Width && selectedPreviewSize.Height <= previewSize.Height) ||
                            selectedPreviewSize.Width > AUTO_CAMERA_PREVIEW_WIDTH_LIMIT || selectedPreviewSize.Height > AUTO_CAMERA_PREVIEW_HEIGHT_LIMIT)
                        {
                            selectedPreviewSize = previewSize;
                        }
                    }
                }
                // if camera preview size is not empty
                else
                {
                    // select camera preview size
                    selectedPreviewSize = _cameraPreviewSize;
                }
                // set camera preview size
                parameters.SetPreviewSize(selectedPreviewSize.Width, selectedPreviewSize.Height);

                // set camera parameters
                _camera.SetParameters(parameters);

                // get updated camera parameters
                parameters = _camera.GetParameters();

                // set preview frame size
                _capturedPreviewFrameSize = new Size(parameters.PreviewSize.Width, parameters.PreviewSize.Height);
            }
        }

        /// <summary>
        /// Starts preview.
        /// </summary>
        internal void StartPreview()
        {
            // if camera uses preview buffer
            if (USE_PREVIEW_BUFFER)
            {
                // get camera parameters
                Camera.Parameters previewParameters = Camera.GetParameters();
                // get camera preview size
                Camera.Size previewSize = previewParameters.PreviewSize;
                // get bits per pixel for camera preview format
                int bitsPerPixel = Android.Graphics.ImageFormat.GetBitsPerPixel(previewParameters.PreviewFormat);
                // get buffer size
                int bufferSize = (previewSize.Width * previewSize.Height * bitsPerPixel) / 8;
                // add camera buffer
                _camera.AddCallbackBuffer(new byte[bufferSize]);
                _camera.SetPreviewCallbackWithBuffer(_previewCallbackDelegate);
            }
            else
            {
                _camera.SetPreviewCallback(_previewCallbackDelegate);
            }

            // start camera preview
            _camera.StartPreview();

            // if autofocus is enabled
            if (Autofocus)
                // request autofocus
                RequestAutoFocus();
        }

        /// <summary>
        /// Stops preview.
        /// </summary>
        internal void StopPreview()
        {
            // stop camerta preview
            _camera.StopPreview();

            if (USE_PREVIEW_BUFFER)
                _camera.SetPreviewCallbackWithBuffer(null);
            else
                _camera.SetPreviewCallback(null);
        }

        /// <summary>
        /// Closes camera.
        /// </summary>
        internal void CloseCamera()
        {
            // is camera is not empty
            if (_camera != null)
            {
                // stop camera preview
                StopPreview();
                // release camera
                _camera.Release();
                _camera = null;
            }
        }

        /// <summary>
        /// Sets on/off flashlight.
        /// </summary>
        /// <param name="isEnabled">Indicates whether flashlight should be enabled.</param>
        internal void SetOnOffFlashlight(bool isEnabled)
        {
            // get camera parameters
            Camera.Parameters parameters = Camera.GetParameters();
            if (isEnabled)
            {
                // set flash mode torch
                parameters.FlashMode = Camera.Parameters.FlashModeTorch;
            }
            else
            {
                // set flash mode off
                parameters.FlashMode = Camera.Parameters.FlashModeOff;
            }
            // set camera parameters
            Camera.SetParameters(parameters);
            Camera.StartPreview();
        }

        /// <summary>
        /// Sets the camera preview size.
        /// </summary>
        /// <param name="previewSizeIndex">Index of preview size.</param>
        internal void SetCameraPreviewSizeSettings(int previewSizeIndex)
        {
            // if index is -1
            if (previewSizeIndex == -1)
            {
                // set camera preview size null
                _cameraPreviewSize = null;
                return;
            }
            try
            {
                // if camera is not empty
                // and camera preview size list is not empty
                // and index is correct
                if (Camera != null && CameraPreviewSizes != null && CameraPreviewSizes.Count > previewSizeIndex)
                {
                    // if new camera preview size is not equal to previous one
                    if (_cameraPreviewSize != CameraPreviewSizes[previewSizeIndex])
                    {
                        // set new camera preview size
                        _cameraPreviewSize = CameraPreviewSizes[previewSizeIndex];
                        // stop camera preview
                        Camera.StopPreview();
                        // apply settings
                        ApplyCameraSettings();
                        // start camera preview
                        Camera.StartPreview();
                    }
                }
            }
            catch
            {
            }
        }

        #endregion

        #endregion



        #region Events

        /// <summary>
        /// Occurs when preview surface parameters are changed.
        /// </summary>
        internal event EventHandler PreviewSurfaceParamsIsChanged;

        #endregion

    }
}
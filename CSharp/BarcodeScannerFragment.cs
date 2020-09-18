using Android.App;
using Android.Content;
using Android.Hardware;
using Android.Media;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Text.Method;
using Android.Views;
using Android.Widget;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using Vintasoft.XamarinBarcode;
using Vintasoft.XamarinBarcode.BarcodeInfo;
using Vintasoft.XamarinBarcode.SymbologySubsets;
using Vintasoft.XamarinBarcode.SymbologySubsets.AAMVA;
using Vintasoft.XamarinBarcode.SymbologySubsets.GS1;
using Vintasoft.XamarinBarcode.SymbologySubsets.RoyalMailMailmark;
using Vintasoft.XamarinBarcode.SymbologySubsets.XFACompressed;

namespace BarcodeScannerDemo
{
    /// <summary>
    /// The barcode scanner fragment, that contains main user interface.
    /// </summary>
    public class BarcodeScannerFragment : Android.Support.V4.App.Fragment, Camera.IPreviewCallback
    {

        #region Constants

        /// <summary>
        /// The FPS measuring time in milliseconds.
        /// </summary>
        const float FPS_MEASURING_TIME = 2000;

        /// <summary>
        /// The Autofocus interval in milliseconds.
        /// </summary>
        const int AUTOFOCUS_INTERVAL = 2500;

        /// <summary>
        /// The vibration time.
        /// </summary>
        const int VIBRATE_TIME = 200;

        /// <summary>
        /// Determines whether scanning should be processed in background thread.
        /// </summary>
        const bool SCAN_IN_BACKGROUND_THREAD = true;

        #endregion



        #region Fields

        /// <summary>
        /// Main activity.
        /// </summary>
        MainActivity _mainActivity;

        /// <summary>
        /// The barcode scanner surface view.
        /// </summary>
        BarcodeScannerSurfaceView _barcodeScannerSurfaceView;

        /// <summary>
        /// The barcode scanner overlay view.
        /// </summary>
        BarcodeScannerOverlayView _barcodeScannerOverlay;

        /// <summary>
        /// The camera barcode scanner.
        /// </summary>
        CameraBarcodeScanner _barcodeScanner;

        /// <summary>
        /// The main frame layout.
        /// </summary>
        FrameLayout _frameLayout;

        /// <summary>
        /// The dictionary with recognized barcodes: barcode info => image.
        /// </summary>
        Dictionary<IBarcodeInfo, ImageSource> _newRecognizedBarcodes = new Dictionary<IBarcodeInfo, ImageSource>();

        /// <summary>
        /// The last time when camera was autofocused.
        /// </summary>
        DateTime _lastAutofocusTime;

        /// <summary>
        /// The last time when barcode was recognized.
        /// </summary>
        DateTime _lastRecognizedBarcodeTime;

        /// <summary>
        /// Indicates whether scanning is in progress.
        /// </summary>
        bool _frameScanning = false;

        /// <summary>
        /// Determines that "Vibrate" permission is granted.
        /// </summary>
        bool _isVibratePermissionGranted = false;


        #region Help Info

        /// <summary>
        /// The text view with help information.
        /// </summary>
        TextView _helpInfoTextView;

        /// <summary>
        /// The timer.
        /// </summary>
        Stopwatch _timer = new Stopwatch();

        /// <summary>
        /// The current FPS.
        /// </summary>
        int _fps;

        /// <summary>
        /// The scan iterations count.
        /// </summary>
        int _scanIterations = 0;

        #endregion


        #region User Interface

        /// <summary>
        /// The layout which contains title, settings layouts and help info text view.
        /// </summary>
        LinearLayout _titleWithHelpInfoLayout = null;

        /// <summary>
        /// The title layout.
        /// </summary>
        LinearLayout _titleLayout = null;

        /// <summary>
        /// The settins layout.
        /// </summary>
        LinearLayout _settingsLayout = null;

        /// <summary>
        /// The right buttons layout.
        /// </summary>
        LinearLayout _rightButtonsLayout = null;

        /// <summary>
        /// The title button.
        /// </summary>
        ImageButton _titleButton = null;

        /// <summary>
        /// The settings button.
        /// </summary>
        ImageButton _settingsButton = null;

        /// <summary>
        /// The flashlight button.
        /// </summary>
        ImageButton _flashlightButton = null;

        /// <summary>
        /// The history button.
        /// </summary>
        ImageButton _historyButton = null;

        /// <summary>
        /// The clear history button.
        /// </summary>
        ImageButton _clearHistoryButton = null;

        /// <summary>
        /// The about button.
        /// </summary>
        ImageButton _aboutButton = null;

        /// <summary>
        /// The text views layout.
        /// </summary>
        LinearLayout _textViewsLayout = null;

        /// <summary>
        /// The recognized barcode text view.
        /// </summary>
        TextView _recognizedBarcodeTextView = null;


        #region Messages

        /// <summary>
        /// Message with information about recognized barcodes.
        /// </summary>
        string _recognizedBarcodesMessage = null;

        /// <summary>
        /// The help info message. 
        /// </summary>
        string _helpInfoMessage = null;

        /// <summary>
        /// The help info barcode recognized message.
        /// </summary>
        string _helpInfoBarcodeRecognizedMessage = null;

        /// <summary>
        /// The scan mode "Automatic" message.
        /// </summary>
        string _scanModeAutoMessage = null;

        /// <summary>
        /// The scan mode "Treshold" message.
        /// </summary>
        string _scanModeTresholdMessage = null;

        /// <summary>
        /// The scan mode "Treshold (Auto)" message.
        /// </summary>
        string _scanModeTresholdAutoMessage = null;

        /// <summary>
        /// The frame scanning time message.
        /// </summary>
        string _frameScanTimeMessage = null;

        /// <summary>
        /// The FPS message.
        /// </summary>
        string _fpsMessage = null;

        #endregion


        /// <summary>
        /// The media player, which is used for playing the sound signal.
        /// </summary>
        MediaPlayer _mediaPlayer = null;

        /// <summary>
        /// The vibrator.
        /// </summary>
        Vibrator _vibrator = null;

        #endregion


        #region Settings

        /// <summary>
        /// Determines whether frame scanning should be stoped.
        /// </summary>
        bool _stopScan = true;

        /// <summary>
        /// Determines whether sound signal should be played.
        /// </summary>
        bool _soundSignal = false;

        /// <summary>
        /// Determines whether vibration signal should be played.
        /// </summary>
        bool _vibration = false;

        /// <summary>
        /// Determines whether barcode value should be copied to the clipboard.
        /// </summary>
        bool _copyToClipboard = false;

        /// <summary>
        /// The FPS limit.
        /// </summary>
        int _fpsLimitSettings = 0;

        /// <summary>
        /// The start frame scan time.
        /// </summary>
        DateTime _startLastScanTime = DateTime.Now;

        /// <summary>
        /// The text encoding name.
        /// </summary>
        string _textEncodingName = "-1";

        /// <summary>
        /// Determines whether image should be flipped before scan.
        /// </summary>
        bool _flipImage = false;

        #endregion

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="BarcodeScannerFragment"/> class.
        /// </summary>
        public BarcodeScannerFragment()
            : base()
        {
            _cameraController = new CameraController(this, false);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="BarcodeScannerFragment"/> class.
        /// </summary>
        protected BarcodeScannerFragment(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
            _cameraController = new CameraController(this, false);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="BarcodeScannerFragment"/> class.
        /// </summary>
        /// <param name="recognizedBarcodes">A reference to the recognized barcode list.</param>
        /// <param name="isFlashlightPermissionGranted">Determines that "Flashlight" permission is granted.</param>
        /// <param name="isVibratePermissionGranted">Determines that "Vibrate" permission is granted.</param>
        internal BarcodeScannerFragment(MainActivity mainActivity, List<IBarcodeInfo> recognizedBarcodes, bool isFlashlightPermissionGranted, bool isVibratePermissionGranted)
        {
            _mainActivity = mainActivity;
            _recognizedBarcodes = recognizedBarcodes;
            _isVibratePermissionGranted = isVibratePermissionGranted;
            _cameraController = new CameraController(this, isFlashlightPermissionGranted);
        }

        #endregion



        #region Properties

        CameraController _cameraController;
        /// <summary>
        /// Gets the camera controller.
        /// </summary>
        internal CameraController CameraController
        {
            get
            {
                return _cameraController;
            }
        }

        List<IBarcodeInfo> _recognizedBarcodes;
        /// <summary>
        /// Gets the recognized barcode list.
        /// </summary>
        internal List<IBarcodeInfo> RecognizedBarcodes
        {
            get
            {
                return _recognizedBarcodes;
            }
        }

        bool _reconstructStructuredAppendBarcodes = true;
        /// <summary>
        /// Gets or sets a value, which indicates that application should reconstruct composite structured append barcodes.
        /// </summary>
        internal bool ReconstructStructuredAppendBarcodes
        {
            get
            {
                return _reconstructStructuredAppendBarcodes;
            }
            set
            {
                _reconstructStructuredAppendBarcodes = value;
            }
        }

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Called to do initial creation of a fragment.
        /// </summary>
        /// <param name="savedInstanceState">The saved instance state if the fragment is being re-created from a previous saved state.</param>
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // create barcode scanner
            _barcodeScanner = CreateCameraBarcodeScanner();

            // create camera controller
            _cameraController.PreviewSurfaceParamsIsChanged += CameraController_PreviewSurfaceParamsIsChanged;
            // subcribe to the barcode scanner events
            _barcodeScanner.FrameScanFinished += BarcodeScanner_FrameScanFinished;
            // create the barcode scanner overlay view
            _barcodeScannerOverlay = new BarcodeScannerOverlayView(_mainActivity, _cameraController, _barcodeScanner);
            // set the identifier for the barcode scanner overlay view
            _barcodeScannerOverlay.Id = 123;
            // create the barcode scanner surface view
            _barcodeScannerSurfaceView = new BarcodeScannerSurfaceView(_mainActivity, _cameraController, _barcodeScanner, _barcodeScannerOverlay);
            // set the identifier for the barcode scanner surface view
            _barcodeScannerSurfaceView.Id = 124;

            _cameraController.BarcodeOveralyView = _barcodeScannerOverlay;

            // get sound
            try
            {
                _mediaPlayer = MediaPlayer.Create(_mainActivity, Resource.Raw.barcode_recognized_sound);
            }
            catch (Exception ex)
            {
                _mediaPlayer = null;
                Toast.MakeText(_mainActivity, string.Format("Media player {0}", ex.Message), ToastLength.Short).Show();
            }

            // get vibrator
            try
            {
                _vibrator = (Vibrator)_mainActivity.GetSystemService(Android.Content.Context.VibratorService);
            }
            catch (Exception ex)
            {
                _vibrator = null;
                Toast.MakeText(_mainActivity, string.Format("Vibrator {0}", ex.Message), ToastLength.Short).Show();
            }
        }

        /// <summary>
        /// Called to have the fragment instantiate its user interface view.
        /// </summary>
        /// <param name="inflater">The <see cref="LayoutInflater"/> object that can be used to inflate any view in the fragment.</param>
        /// <param name="container">
        /// If non-null, this is the parent view that the fragment's UI should be attached to. 
        /// The fragment should not add the view itself, but this can be used to generate the LayoutParams of the view.
        /// </param>
        /// <param name="savedInstanceState">
        /// If non-null, this fragment is being re-constructed from a previous saved state as given here.
        /// </param>
        /// <returns>
        /// The created view.
        /// </returns>
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (_frameLayout == null)
            {
                // get frame layout
                _frameLayout = (FrameLayout)inflater.Inflate(Resource.Layout.barcode_reader_fragment_layout, container, false);

                // get layout params
                LinearLayout.LayoutParams layoutParams = GetChildLayoutParams();
                // add views to the frame layout
                _frameLayout.AddView(_barcodeScannerSurfaceView, layoutParams);
                _frameLayout.AddView(_barcodeScannerOverlay, layoutParams);

                // subscribe to the touch event
                _frameLayout.Touch += Frame_Touch;

                // get settings button
                _settingsButton = _frameLayout.FindViewById<ImageButton>(Resource.Id.settingsButton);
                _settingsButton.Click += SettingsButton_Click;

                // get flashlight button
                _flashlightButton = _frameLayout.FindViewById<ImageButton>(Resource.Id.flashlightButton);
                _flashlightButton.Click += FlashlightButton_Click;

                // get history button
                _historyButton = _frameLayout.FindViewById<ImageButton>(Resource.Id.historyButton);
                _historyButton.Click += HistoryButton_Click;

                // get history button
                _clearHistoryButton = _frameLayout.FindViewById<ImageButton>(Resource.Id.clearHistoryButton);
                _clearHistoryButton.Click += ClearHistoryButton_Click;

                // get about button
                _aboutButton = _frameLayout.FindViewById<ImageButton>(Resource.Id.aboutButton);
                _aboutButton.Click += AboutButton_Click;

                // get title button
                _titleButton = _frameLayout.FindViewById<ImageButton>(Resource.Id.titleIcon);
                _titleButton.Click += TitleButton_Click;

                // get buttons layout
                _rightButtonsLayout = _frameLayout.FindViewById<LinearLayout>(Resource.Id.rightButtonsLayout);

                // get help info text view
                _helpInfoTextView = _frameLayout.FindViewById<TextView>(Resource.Id.helpInfoTextView);
                _helpInfoMessage = Resources.GetString(Resource.String.help_info_message);
                _helpInfoBarcodeRecognizedMessage = Resources.GetString(Resource.String.help_info_barcode_recognized_message);
                _scanModeAutoMessage = Resources.GetString(Resource.String.scan_mode_auto_message);
                _scanModeTresholdMessage = Resources.GetString(Resource.String.scan_mode_treshold_message);
                _scanModeTresholdAutoMessage = Resources.GetString(Resource.String.scan_mode_treshold_auto_message);
                _fpsMessage = Resources.GetString(Resource.String.fps_message);
                _frameScanTimeMessage = Resources.GetString(Resource.String.frame_scan_time_message);

                // get regonized barcode text view
                _recognizedBarcodeTextView = _frameLayout.FindViewById<TextView>(Resource.Id.regocnizedBarcodeTextView);
                _recognizedBarcodeTextView.Click += RegonizedBarcodeTextView_Click;
                _recognizedBarcodesMessage = Resources.GetString(Resource.String.barcodes_recognized_message);
                _recognizedBarcodeTextView.MovementMethod = LinkMovementMethod.Instance;

                // get text view layout
                _textViewsLayout = _frameLayout.FindViewById<LinearLayout>(Resource.Id.textViewLayout);

                // get layout with icon and title
                _titleLayout = _frameLayout.FindViewById<LinearLayout>(Resource.Id.titleLayout);
                // get layout with "settings" and "about" buttons
                _settingsLayout = _frameLayout.FindViewById<LinearLayout>(Resource.Id.settingsLayout);
                // get layout with previous two and help info text view
                _titleWithHelpInfoLayout = _frameLayout.FindViewById<LinearLayout>(Resource.Id.titleWithHelpInfoLayout);

                // bring buttons and text views to front
                BringButtonsAndTextViewsToFront();

                UpdateRecognizedBarcodesInfo();
            }

            _lastAutofocusTime = DateTime.Now;
            _lastRecognizedBarcodeTime = DateTime.Now;

            return _frameLayout;
        }

        /// <summary>
        /// Called when the Fragment is visible to the user.
        /// </summary>
        public override void OnStart()
        {
            base.OnStart();

            // get layout parameters
            LinearLayout.LayoutParams layoutParams = GetChildLayoutParams();
            if (_frameLayout.FindViewById(_barcodeScannerSurfaceView.Id) == null)
            {
                // add scanner surface view to frame
                _frameLayout.AddView(_barcodeScannerSurfaceView, layoutParams);
            }
            if (_frameLayout.FindViewById(_barcodeScannerOverlay.Id) == null)
            {
                // add scanner overlay view to frame
                _frameLayout.AddView(_barcodeScannerOverlay, layoutParams);
            }

            BringButtonsAndTextViewsToFront();
            UpdateRecognizedBarcodesInfo();
        }

        /// <summary>
        /// Called when the Fragment is no longer started.
        /// </summary>
        public override void OnStop()
        {
            _frameLayout.RemoveView(_barcodeScannerSurfaceView);
            _frameLayout.RemoveView(_barcodeScannerOverlay);

            base.OnStop();
        }

        /// <summary>
        /// Called when the Fragment is visible to the user and actively running.
        /// </summary>
        public override void OnResume()
        {
            base.OnResume();

            bool isScanShouldBeStarting = false;
            try
            {
                isScanShouldBeStarting = SetSettings();
            }
            catch (Exception ex)
            {
                Toast.MakeText(_mainActivity, string.Format("Set settings error: {0}", ex.Message), ToastLength.Short).Show();
            }
            // if scan should be starting
            if (isScanShouldBeStarting)
            {
                // remove information about recognized barcode from overlay view
                _barcodeScannerOverlay.RemoveRecognizedBarcode();
                // start the barcode scanning
                _barcodeScanner.StartScanning();
                return;
            }

            // if overlay view does not have information about recognized barcode
            if (_barcodeScannerOverlay.RecognizedBarcode == null)
                // start the barcode scanning
                _barcodeScanner.StartScanning();
        }

        /// <summary>
        /// Called when the Fragment is no longer resumed.
        /// </summary>
        public override void OnPause()
        {
            // stop the barcode scanning
            _barcodeScanner.StopScanning();
            // turn off flashlight
            _cameraController.SetOnOffFlashlight(false);
            _flashlightButton.SetImageResource(Resource.Mipmap.flash_off);
            base.OnPause();
        }

        /// <summary>
        /// Preview frame is displayed.
        /// </summary>
        /// <param name="data">The content of preview frame in the format defined by <see cref="Android.Graphics.ImageFormat"/>.</param>
        /// <param name="camera">A camera.</param>
        public void OnPreviewFrame(byte[] data, Camera camera)
        {
            if (_frameScanning)
                return;

            // if the barcode scanning is not starting
            if (!_barcodeScanner.IsScanningStarted)
            {
                // if camera should use preview buffer
                if (CameraController.USE_PREVIEW_BUFFER)
                    // add preview buffer to the camera
                    camera.AddCallbackBuffer(data);
                return;
            }

            // if FPS is limited
            if (_fpsLimitSettings != -1)
            {
                // get the last frame scan time
                double lastScanTime = (DateTime.Now - _startLastScanTime).TotalMilliseconds;
                // if FPS for last frame scan is greater than FPS limit
                if ((1000.0 / lastScanTime) > _fpsLimitSettings * 1.25)
                {
                    // skip frame scan
                    return;
                }
            }

            // get start frame scan time
            _startLastScanTime = DateTime.Now;

            // indicate that frame scanning is started
            _frameScanning = true;
            try
            {
                // create new gray scale image source
                GrayscaleImageSource image = new GrayscaleImageSource(_cameraController.CapturedPreviewFrameSize.Width, _cameraController.CapturedPreviewFrameSize.Height, data);
                // if scanning should be processed in background thread
                if (SCAN_IN_BACKGROUND_THREAD)
                {
                    // start new thread
                    Task.Factory.StartNew(() =>
                    {
                        // scan image
                        ScanImage(camera, data, image);
                    });
                }
                // if scanning should be processed in main thread
                else
                {
                    // scan image
                    ScanImage(camera, data, image);
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(_mainActivity, string.Format("OnPreviewFrame: {0}", ex.Message), ToastLength.Long).Show();
                camera.StopPreview();
            }
        }

        /// <summary>
        /// Recognizes barcodes on specified image.
        /// </summary>
        /// <param name="camera">The camera.</param>
        /// <param name="prewiewBuffer">The preview buffer.</param>
        /// <param name="image">The image.</param>
        private void ScanImage(Camera camera, byte[] prewiewBuffer, GrayscaleImageSource image)
        {
            try
            {
                // flip image on X axis, if need
                if (_flipImage)
                    image.FlipX();

                // scan image
                _barcodeScanner.ScanFrame(image);
            }
            catch (Exception ex)
            {
                _mainActivity.RunOnUiThread(() =>
                {
                    Toast.MakeText(_mainActivity, string.Format("Scan frame: {0}", ex.Message), ToastLength.Long).Show();
                    // stop camera preview
                    camera.StopPreview();
                });
            }
            finally
            {
                // if camera should use preview buffer
                if (CameraController.USE_PREVIEW_BUFFER)
                    // add preview buffer to the camera
                    camera.AddCallbackBuffer(prewiewBuffer);
                // indicate that frame scanning is stoped
                _frameScanning = false;
            }
        }

        #endregion


        #region INTERNAL

        /// <summary>
        /// Back key is pressed.
        /// </summary>
        /// <returns>
        /// <b>false</b> - indicates that this event is not handled and should be propagated; otherwise - <b>true</b>.
        /// </returns>
        internal bool BackKeyPressed()
        {
            // if overlay view has information about recognized barcode
            if (_barcodeScannerOverlay.RecognizedBarcode != null)
            {
                // if there are new recognized barcodes
                if (_newRecognizedBarcodes.Count > 0)
                {
                    ShowNextRecognizedBarcode();
                }
                else
                {
                    // remove information about recognized barcode from overlay view
                    _barcodeScannerOverlay.RemoveRecognizedBarcode();
                    // update information about recognized barcode
                    UpdateRecognizedBarcodesInfo();
                    // start the barcode scanning
                    _barcodeScanner.StartScanning();
                }
                return true;
            }
            return false;
        }

        #endregion


        #region PRIVATE

        #region Interface

        /// <summary>
        /// Opens URL "www.vintasoft.com" in the browser.
        /// </summary>
        private void TitleButton_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(Resources.GetString(Resource.String.vintasoft_barcodes_url)));
            StartActivity(intent);
        }

        /// <summary>
        /// Shows the "About" window.
        /// </summary>
        private void AboutButton_Click(object sender, EventArgs e)
        {
            // show the About dialog
            _mainActivity.ShowAboutDialog();
        }

        /// <summary>
        /// Turns on/off flashlight.
        /// </summary>
        private void FlashlightButton_Click(object sender, EventArgs e)
        {
            // get camera parameters
            Camera.Parameters paramters = _cameraController.Camera.GetParameters();
            // if flashlight is on
            if (paramters.FlashMode == Camera.Parameters.FlashModeTorch)
            {
                // turn off flashlight
                _cameraController.SetOnOffFlashlight(false);
                _flashlightButton.SetImageResource(Resource.Mipmap.flash_off);
            }
            else
            {
                // turn on flashlight
                _cameraController.SetOnOffFlashlight(true);
                _flashlightButton.SetImageResource(Resource.Mipmap.flash_on);
            }

        }

        /// <summary>
        /// Shows the history.
        /// </summary>
        private void HistoryButton_Click(object sender, EventArgs e)
        {
            ShowHistory();
        }

        /// <summary>
        /// Clears the history.
        /// </summary>
        private void ClearHistoryButton_Click(object sender, EventArgs e)
        {
            // cleare recognized barcode list
            RecognizedBarcodes.Clear();
            ChangeClearButtonState(false);

            // remove information about recognized barcode from overlay view
            _barcodeScannerOverlay.RemoveRecognizedBarcode();
            // update information about recognized barcode
            UpdateRecognizedBarcodesInfo();
            // start the barcode scanning
            _barcodeScanner.StartScanning();
        }

        /// <summary>
        /// Changes state of "Clear History" button.
        /// </summary>
        /// <param name="state">
        /// A state. 
        /// <b>True</b> - if button should be enabled.
        /// <b>False</b> - if button should be disabled.
        /// </param>
        private void ChangeClearButtonState(bool state)
        {
            _clearHistoryButton.Enabled = state;
            if (state)
                _clearHistoryButton.SetImageResource(Resource.Mipmap.clear_history_enabled);
            else
                _clearHistoryButton.SetImageResource(Resource.Mipmap.clear_history_disabled);
        }

        /// <summary>
        /// Shows the application settings.
        /// </summary>
        private void SettingsButton_Click(object sender, EventArgs e)
        {
            _mainActivity.SwitchToSettings(this, _cameraController);
        }

        /// <summary>
        /// Bring buttons and text views to the front.
        /// </summary>
        private void BringButtonsAndTextViewsToFront()
        {
            _rightButtonsLayout.BringToFront();
            _textViewsLayout.BringToFront();
            _titleWithHelpInfoLayout.BringToFront();
            _titleLayout.BringToFront();
            _settingsLayout.BringToFront();

        }

        /// <summary>
        /// Handles <event cref="View.Touch"/> event.
        /// </summary>
        private void Frame_Touch(object sender, View.TouchEventArgs e)
        {
            // if motion action is up
            if (e.Event.Action == MotionEventActions.Up)
            {
                // if touch point lies on recognize surface
                if (_barcodeScannerOverlay.GetIsPointOnRecognizeSurface(e.Event.RawX, e.Event.RawY))
                {
                    // if overlay view has information about recognized barcode
                    if (_barcodeScannerOverlay.RecognizedBarcode != null)
                    {
                        // get extended information about barcode
                        string info = _mainActivity.GetExtendedBarcodeInfoString(_barcodeScannerOverlay.RecognizedBarcode, _textEncodingName);
                        // get barcode type
                        string barcodeType = Utils.GetBarcodeTypeString(_barcodeScannerOverlay.RecognizedBarcode);
                        // show dialog
                        _mainActivity.ShowInfoDialog(barcodeType, info, true, _barcodeScannerOverlay.RecognizedBarcode.Value);
                    }

                    // if autofocus on touch is enabled
                    else if (_cameraController.AutofocusOnTouch)
                    {
                        // request autofocus
                        _cameraController.RequestAutoFocus();
                    }
                }
            }
        }

        /// <summary>
        /// Shows history if overlay view does not have information about recognized barcode.
        /// </summary>
        private void RegonizedBarcodeTextView_Click(object sender, EventArgs e)
        {
            if (_barcodeScannerOverlay.RecognizedBarcode == null)
            {
                ShowHistory();
            }
        }

        /// <summary>
        /// Shows history.
        /// </summary>
        private void ShowHistory()
        {
            _mainActivity.SwitchToHistory(this);
        }

        /// <summary>
        /// Sets height to the bottom layout.
        /// </summary>
        private void CameraController_PreviewSurfaceParamsIsChanged(object sender, EventArgs e)
        {
            float borderHeight = _cameraController.CapturedPreviewFrameVisibleSize.Width / 8;

            int frameHeight = _cameraController.CapturedPreviewFrameSize.Height;
            // get the camera preview size
            int height = _cameraController.PreviewSurfaceSize.Height;

            // get the scale parameters
            float scaleY = height / (float)frameHeight;

            int maxHeight = (int)(_cameraController.CapturedPreviewFrameVisibleSize.Width / 8 * scaleY);
            // set layout height for centering text view
            _textViewsLayout.LayoutParameters.Height = maxHeight;
            // set text view max height to correct displaying
            _recognizedBarcodeTextView.SetMaxHeight(maxHeight);
        }

        #endregion


        /// <summary>
        /// Sets settings from <see cref="SettingsFragment"/>.
        /// </summary>
        /// <returns>
        /// <b>true</b> - if scanning should be started immediately; otherwise - <b>false</b>.
        /// </returns>
        private bool SetSettings()
        {
            // get preferences
            ISharedPreferences preferences = PreferenceManager.GetDefaultSharedPreferences(_mainActivity);

            bool isScanShouldBeStarting = false;

            // base settings
            _cameraController.Autofocus = preferences.GetBoolean("checkbox_autofocus", false);
            if (_cameraController.Autofocus)
            {
                _cameraController.ContinuousAutofocus = preferences.GetBoolean("checkbox_continuous_autofocus", false);
                _cameraController.AutofocusOnTouch = preferences.GetBoolean("checkbox_autofocus_on_touch", false);
            }
            else
            {
                _cameraController.ContinuousAutofocus = false;
                _cameraController.AutofocusOnTouch = false;
            }

            string cameraPreviewSizeIndexString = preferences.GetString("list_camera_preview_sizes", "-1");
            int cameraPreviewSizeIndex = Convert.ToInt32(cameraPreviewSizeIndexString);
            System.Drawing.Size previousPreviewSize = _cameraController.CapturedPreviewFrameSize;
            _cameraController.SetCameraPreviewSizeSettings(cameraPreviewSizeIndex);
            // if new camera preview size is not equal to previous camera preview size
            if (_cameraController.CapturedPreviewFrameSize != previousPreviewSize)
                // indicate that scanning should be started
                isScanShouldBeStarting = true;

            _stopScan = preferences.GetBoolean("checkbox_stop_scan", true);
            _soundSignal = preferences.GetBoolean("checkbox_sound_signal", false);
            _vibration = preferences.GetBoolean("checkbox_vibration", false);
            _copyToClipboard = preferences.GetBoolean("checkbox_copy_to_clipboard", false);

            _barcodeScanner.RecognitionMode = GetRecognitionModeFormString(preferences.GetString("list_recognition_mode", "adaptive"));
            _barcodeScanner.ScannerSettings.InvertImageColors = preferences.GetBoolean("checkbox_invert_image", false);
            _flipImage = preferences.GetBoolean("checkbox_flip_image", false);
            _barcodeScannerOverlay.IsImageFlipped = _flipImage;
            ReconstructStructuredAppendBarcodes = preferences.GetBoolean("checkbox_reconstruct_structured_append_barcodes", true);
            _textEncodingName = preferences.GetString("list_encodings", "-1");
            if (_textEncodingName == "-1")
            {
                try
                {
                    // set UTF8 (65001) as default encoding
                    BinaryValueItem.TextEncoding = System.Text.Encoding.UTF8;
                }
                catch (Exception ex)
                {
                    Toast.MakeText(_mainActivity, string.Format("TextEncoding = Unicode: {0}", ex.Message), ToastLength.Short).Show();
                }
            }
            else
            {
                BinaryValueItem.TextEncoding = null;
            }

            string fpsLimitSettingsString = preferences.GetString("list_fps_limit", "-1");
            _fpsLimitSettings = Convert.ToInt32(fpsLimitSettingsString);
            _barcodeScanner.RecognitionMode = GetRecognitionModeFormString(preferences.GetString("list_recognition_mode", "adaptive"));

            // clear subsets
            List<BarcodeSymbologySubset> barcodeSubsets = _barcodeScanner.ScannerSettings.ScanBarcodeSubsets;
            barcodeSubsets.Clear();

            BarcodeType barcodeTypes = BarcodeType.None;
            // 2D barcodes settings
            if (preferences.GetBoolean("checkbox_2D_barcodes_Aztec", false))
            {
                barcodeTypes |= BarcodeType.Aztec;
                barcodeSubsets.Add(new XFACompressedAztecBarcodeSymbology());
            }
            if (preferences.GetBoolean("checkbox_2D_barcodes_DataMatrix", false))
            {
                barcodeTypes |= BarcodeType.DataMatrix;
                barcodeSubsets.Add(new PpnBarcodeSymbology());
                barcodeSubsets.Add(new MailmarkCmdmType29BarcodeSymbology());
                barcodeSubsets.Add(new MailmarkCmdmType7BarcodeSymbology());
                barcodeSubsets.Add(new MailmarkCmdmType9BarcodeSymbology());
                barcodeSubsets.Add(new XFACompressedDataMatrixBarcodeSymbology());
            }
            if (preferences.GetBoolean("checkbox_2D_barcodes_DotCode", false))
            {
                barcodeTypes |= BarcodeType.DotCode;
            }
            if (preferences.GetBoolean("checkbox_2D_barcodes_HanXin", false))
                barcodeTypes |= BarcodeType.HanXinCode;
            if (preferences.GetBoolean("checkbox_2D_barcodes_PDF417", false))
            {
                barcodeTypes |= BarcodeType.PDF417 | BarcodeType.PDF417Compact;
                barcodeSubsets.Add(new XFACompressedPDF417BarcodeSymbology());
                barcodeSubsets.Add(new AamvaBarcodeSymbology());
            }
            if (preferences.GetBoolean("checkbox_2D_barcodes_PDF417Compact", false))
                barcodeTypes |= BarcodeType.PDF417Compact;
            if (preferences.GetBoolean("checkbox_2D_barcodes_MicroPDF417", false))
                barcodeTypes |= BarcodeType.MicroPDF417;
            if (preferences.GetBoolean("checkbox_2D_barcodes_QR", false))
            {
                barcodeTypes |= BarcodeType.QR;
                barcodeSubsets.Add(new XFACompressedQRCodeBarcodeSymbology());
                barcodeSubsets.Add(new SwissQRCodeBarcodeSymbology());
            }
            if (preferences.GetBoolean("checkbox_2D_barcodes_MicroQR", false))
                barcodeTypes |= BarcodeType.MicroQR;
            if (preferences.GetBoolean("checkbox_2D_barcodes_MaxiCode", false))
                barcodeTypes |= BarcodeType.MaxiCode;


            // GS1 barcode settings
            if (preferences.GetBoolean("checkbox_GS1_barcodes_Aztec", false))
                barcodeSubsets.Add(new GS1AztecBarcodeSymbology());
            if (preferences.GetBoolean("checkbox_GS1_barcodes_DataMatrix", false))
                barcodeSubsets.Add(new GS1DataMatrixBarcodeSymbology());
            if (preferences.GetBoolean("checkbox_GS1_barcodes_DotCode", false))
                barcodeSubsets.Add(new GS1DotCodeBarcodeSymbology());
            if (preferences.GetBoolean("checkbox_GS1_barcodes_QR", false))
                barcodeSubsets.Add(new GS1QRBarcodeSymbology());
            if (preferences.GetBoolean("checkbox_GS1_barcodes_GS1_128", false))
                barcodeSubsets.Add(new GS1_128BarcodeSymbology());
            if (preferences.GetBoolean("checkbox_GS1_barcodes_GS1_128_CC", false))
            {
                barcodeSubsets.Add(new GS1_128CCABarcodeSymbology());
                barcodeSubsets.Add(new GS1_128CCBBarcodeSymbology());
                barcodeSubsets.Add(new GS1_128CCCBarcodeSymbology());
            }
            if (preferences.GetBoolean("checkbox_GS1_barcodes_GS1DataBar", false))
                barcodeSubsets.Add(new GS1DataBarBarcodeSymbology());
            if (preferences.GetBoolean("checkbox_GS1_barcodes_GS1DataBar_CC", false))
            {
                barcodeSubsets.Add(new GS1DataBarCCABarcodeSymbology());
                barcodeSubsets.Add(new GS1DataBarCCBBarcodeSymbology());
            }
            if (preferences.GetBoolean("checkbox_GS1_barcodes_GS1DataBarLimited", false))
                barcodeSubsets.Add(new GS1DataBarLimitedBarcodeSymbology());
            if (preferences.GetBoolean("checkbox_GS1_barcodes_GS1DataBarLimited_CC", false))
            {
                barcodeSubsets.Add(new GS1DataBarLimitedCCABarcodeSymbology());
                barcodeSubsets.Add(new GS1DataBarLimitedCCBBarcodeSymbology());
            }
            if (preferences.GetBoolean("checkbox_GS1_barcodes_GS1DataBarExpanded", false))
                barcodeSubsets.Add(new GS1DataBarExpandedBarcodeSymbology());
            if (preferences.GetBoolean("checkbox_GS1_barcodes_GS1DataBarExpanded_CC", false))
            {
                barcodeSubsets.Add(new GS1DataBarExpandedCCABarcodeSymbology());
                barcodeSubsets.Add(new GS1DataBarExpandedCCBBarcodeSymbology());
            }
            if (preferences.GetBoolean("checkbox_GS1_barcodes_GS1DataBarStacked", false))
                barcodeSubsets.Add(new GS1DataBarStackedBarcodeSymbology());
            if (preferences.GetBoolean("checkbox_GS1_barcodes_GS1DataBarStacked_CC", false))
            {
                barcodeSubsets.Add(new GS1DataBarStackedCCABarcodeSymbology());
                barcodeSubsets.Add(new GS1DataBarStackedCCBBarcodeSymbology());
            }
            if (preferences.GetBoolean("checkbox_GS1_barcodes_GS1DataBarExpandedStacked", false))
                barcodeSubsets.Add(new GS1DataBarExpandedStackedBarcodeSymbology());
            if (preferences.GetBoolean("checkbox_GS1_barcodes_GS1DataBarExpandedStacked_CC", false))
            {
                barcodeSubsets.Add(new GS1DataBarExpandedStackedCCABarcodeSymbology());
                barcodeSubsets.Add(new GS1DataBarExpandedStackedCCBBarcodeSymbology());
            }
            if (preferences.GetBoolean("checkbox_GS1_barcodes_EANUPC_CC", false))
            {
                barcodeSubsets.Add(new EAN13CCABarcodeSymbology());
                barcodeSubsets.Add(new EAN13CCBBarcodeSymbology());
                barcodeSubsets.Add(new EAN8CCABarcodeSymbology());
                barcodeSubsets.Add(new EAN8CCBBarcodeSymbology());
                barcodeSubsets.Add(new UPCACCABarcodeSymbology());
                barcodeSubsets.Add(new UPCACCBBarcodeSymbology());
                barcodeSubsets.Add(new UPCECCABarcodeSymbology());
                barcodeSubsets.Add(new UPCECCBBarcodeSymbology());
            }


            // 1D barcodes settings
            if (preferences.GetBoolean("checkbox_1D_barcodes_Code11", false))
                barcodeTypes |= BarcodeType.Code11;
            // Code 39
            if (preferences.GetBoolean("checkbox_1D_barcodes_Code39", false))
                barcodeTypes |= BarcodeType.Code39;
            if (preferences.GetBoolean("checkbox_1D_barcodes_Code39Ex", false))
                barcodeSubsets.Add(new Code39ExtendedBarcodeSymbology());
            if (preferences.GetBoolean("checkbox_1D_barcodes_Code32", false))
                barcodeSubsets.Add(new Code32BarcodeSymbology());
            if (preferences.GetBoolean("checkbox_1D_barcodes_VIN", false))
                barcodeSubsets.Add(new VinSymbology());
            if (preferences.GetBoolean("checkbox_1D_barcodes_PZN", false))
                barcodeSubsets.Add(new PznBarcodeSymbology());
            if (preferences.GetBoolean("checkbox_1D_barcodes_DHLAWB", false))
                barcodeSubsets.Add(new DhlAwbBarcodeSymbology());
            if (preferences.GetBoolean("checkbox_1D_barcodes_NumlyNumber", false))
                barcodeTypes |= BarcodeType.Code39;
            //
            if (preferences.GetBoolean("checkbox_1D_barcodes_Code93", false))
                barcodeTypes |= BarcodeType.Code93;
            if (preferences.GetBoolean("checkbox_1D_barcodes_Codabar", false))
                barcodeTypes |= BarcodeType.Codabar;
            // Code 128
            if (preferences.GetBoolean("checkbox_1D_barcodes_Code128", false))
                barcodeTypes |= BarcodeType.Code128;
            if (preferences.GetBoolean("checkbox_1D_barcodes_SSCC18", false))
                barcodeSubsets.Add(new Sscc18BarcodeSymbology());
            if (preferences.GetBoolean("checkbox_1D_barcodes_FedExGround96", false))
                barcodeSubsets.Add(new FedExGround96BarcodeSymbology());
            if (preferences.GetBoolean("checkbox_1D_barcodes_VICS", false))
            {
                barcodeSubsets.Add(new VicsBolBarcodeSymbology());
                barcodeSubsets.Add(new VicsScacProBarcodeSymbology());
            }
            if (preferences.GetBoolean("checkbox_1D_barcodes_SwissPostParcel", false))
                barcodeSubsets.Add(new SwissPostParcelBarcodeSymbology());
            //
            if (preferences.GetBoolean("checkbox_1D_barcodes_EAN8", false))
            {
                barcodeTypes |= BarcodeType.EAN8 | BarcodeType.EAN8Plus2 | BarcodeType.EAN8Plus5;
                barcodeSubsets.Add(new EanVelocityBarcodeSymbology());
                barcodeSubsets.Add(new Jan8BarcodeSymbology());
                barcodeSubsets.Add(new Jan8Plus2BarcodeSymbology());
                barcodeSubsets.Add(new Jan8Plus5BarcodeSymbology());
            }
            if (preferences.GetBoolean("checkbox_1D_barcodes_EAN13", false))
            {
                barcodeTypes |= BarcodeType.EAN13 | BarcodeType.EAN13Plus2 | BarcodeType.EAN13Plus5;
                barcodeSubsets.Add(new Jan13BarcodeSymbology());
                barcodeSubsets.Add(new Jan13Plus2BarcodeSymbology());
                barcodeSubsets.Add(new Jan13Plus5BarcodeSymbology());
                barcodeSubsets.Add(new IsbnBarcodeSymbology());
                barcodeSubsets.Add(new IsbnPlus2BarcodeSymbology());
                barcodeSubsets.Add(new IsbnPlus5BarcodeSymbology());
                barcodeSubsets.Add(new IssnBarcodeSymbology());
                barcodeSubsets.Add(new IssnPlus2BarcodeSymbology());
                barcodeSubsets.Add(new IssnPlus5BarcodeSymbology());
                barcodeSubsets.Add(new IsmnBarcodeSymbology());
                barcodeSubsets.Add(new IsmnPlus2BarcodeSymbology());
                barcodeSubsets.Add(new IsmnPlus5BarcodeSymbology());
            }
            if (preferences.GetBoolean("checkbox_1D_barcodes_UPCA", false))
                barcodeTypes |= BarcodeType.UPCA | BarcodeType.UPCAPlus2 | BarcodeType.UPCAPlus5;
            if (preferences.GetBoolean("checkbox_1D_barcodes_UPCE", false))
                barcodeTypes |= BarcodeType.UPCE | BarcodeType.UPCEPlus2 | BarcodeType.UPCEPlus5;
            if (preferences.GetBoolean("checkbox_1D_barcodes_Standard2of5", false))
                barcodeTypes |= BarcodeType.Standard2of5;
            // Interleaved 2 of 5
            if (preferences.GetBoolean("checkbox_1D_barcodes_Interleaved2of5", false))
                barcodeTypes |= BarcodeType.Interleaved2of5;
            if (preferences.GetBoolean("checkbox_1D_barcodes_ITF14", false))
                barcodeSubsets.Add(new Itf14BarcodeSymbology());
            if (preferences.GetBoolean("checkbox_1D_barcodes_OPC", false))
                barcodeSubsets.Add(new OpcBarcodeSymbology());
            if (preferences.GetBoolean("checkbox_1D_barcodes_DeutschePostIdentcode", false))
                barcodeSubsets.Add(new DeutschePostIdentcodeBarcodeSymbology());
            if (preferences.GetBoolean("checkbox_1D_barcodes_DeutschePostLeitcode", false))
                barcodeSubsets.Add(new DeutschePostLeitcodeBarcodeSymbology());
            //
            if (preferences.GetBoolean("checkbox_1D_barcodes_IATA2of5", false))
                barcodeTypes |= BarcodeType.IATA2of5;
            if (preferences.GetBoolean("checkbox_1D_barcodes_Matrix2of5", false))
                barcodeTypes |= BarcodeType.Matrix2of5;
            if (preferences.GetBoolean("checkbox_1D_barcodes_Telepen", false))
                barcodeTypes |= BarcodeType.Telepen;
            if (preferences.GetBoolean("checkbox_1D_barcodes_PatchCode", false))
                barcodeTypes |= BarcodeType.PatchCode;
            if (preferences.GetBoolean("checkbox_1D_barcodes_RSS14", false))
                barcodeTypes |= BarcodeType.RSS14;
            if (preferences.GetBoolean("checkbox_1D_barcodes_RSSLimited", false))
                barcodeTypes |= BarcodeType.RSSLimited;
            if (preferences.GetBoolean("checkbox_1D_barcodes_RSSExpanded", false))
                barcodeTypes |= BarcodeType.RSSExpanded;
            if (preferences.GetBoolean("checkbox_1D_barcodes_Pharmacode", false))
                barcodeTypes |= BarcodeType.Pharmacode;
            if (preferences.GetBoolean("checkbox_1D_barcodes_MSI", false))
                barcodeTypes |= BarcodeType.MSI;

            // 1D stacked linear barcodes settings
            if (preferences.GetBoolean("checkbox_1D_Code16K", false))
                barcodeTypes |= BarcodeType.Code16K;
            if (preferences.GetBoolean("checkbox_1D_RSS14_stacked", false))
                barcodeTypes |= BarcodeType.RSS14Stacked;
            if (preferences.GetBoolean("checkbox_1D_RSSExpanded_stacked", false))
                barcodeTypes |= BarcodeType.RSSExpandedStacked;

            // 1D postal 2/4-state barcodes settings
            if (preferences.GetBoolean("checkbox_postal_1D_AustralianPost", false))
                barcodeTypes |= BarcodeType.AustralianPost;
            if (preferences.GetBoolean("checkbox_postal_1D_DutchKIX", false))
                barcodeTypes |= BarcodeType.DutchKIX;
            if (preferences.GetBoolean("checkbox_postal_1D_RoyalMail", false))
                barcodeTypes |= BarcodeType.RoyalMail;
            if (preferences.GetBoolean("checkbox_postal_1D_Postnet", false))
                barcodeTypes |= BarcodeType.Postnet;
            if (preferences.GetBoolean("checkbox_postal_1D_Planet", false))
                barcodeTypes |= BarcodeType.Planet;
            if (preferences.GetBoolean("checkbox_postal_1D_IntelligentMail", false))
                barcodeTypes |= BarcodeType.IntelligentMail;
            if (preferences.GetBoolean("checkbox_postal_1D_Mailmark4C", false))
                barcodeTypes |= BarcodeType.Mailmark4StateC;
            if (preferences.GetBoolean("checkbox_postal_1D_Mailmark4L", false))
                barcodeTypes |= BarcodeType.Mailmark4StateL;

            _barcodeScanner.ScannerSettings.ScanBarcodeTypes = barcodeTypes;

            return isScanShouldBeStarting;
        }

        /// <summary>
        /// Returns <see cref="CameraBarcodeScannerMode"/> from string value.
        /// </summary>
        /// <param name="recognitionMode">A string value.</param>
        /// <returns>
        /// Recognition mode.
        /// </returns>
        private CameraBarcodeScannerMode GetRecognitionModeFormString(string recognitionMode)
        {
            string[] recognitionModeNames = Resources.GetStringArray(Resource.Array.entryvalues_list_recognition_mode);

            if (recognitionMode == recognitionModeNames[1])
                return CameraBarcodeScannerMode.Balanced;

            if (recognitionMode == recognitionModeNames[2])
                return CameraBarcodeScannerMode.BestQuality;

            return CameraBarcodeScannerMode.Adaptive;
        }

        /// <summary>
        /// Creates a barcode scanner.
        /// </summary>
        /// <returns>The barcode scanner.</returns>
        private CameraBarcodeScanner CreateCameraBarcodeScanner()
        {
            // create barode scanner
            CameraBarcodeScanner barcodeScanner = new CameraBarcodeScanner();

            // set base scan settings
            barcodeScanner.RecognitionMode = CameraBarcodeScannerMode.Adaptive;
            barcodeScanner.ThresholdRange = 0.3f;
            barcodeScanner.ThresholdCount = 10;

            barcodeScanner.ScannerSettings.ScanDirection = ScanDirection.Horizontal | ScanDirection.Vertical;
            barcodeScanner.ScannerSettings.ScanInterval = 5;
            barcodeScanner.ScannerSettings.MinConfidence = -1;

            barcodeScanner.ScannerSettings.MatrixBarcodeMaxCellSize = 25;
            barcodeScanner.ScannerSettings.QRMaxAxialNonuniformity = 0.4f;

            barcodeScanner.ScannerSettings.BarcodeCharacteristics = BarcodeCharacteristics.NormalSizeBarcodes;

            barcodeScanner.ScannerSettings.MaximumThreadCount = 1;

            // set verify barcode delegate
            barcodeScanner.ScannerSettings.VerifyBarcodeMethod = new VerifyBarcodeDelegate(VerifyBarcode);

            return barcodeScanner;
        }

        /// <summary>
        /// Verifies barcode.
        /// </summary>
        /// <param name="reader">A barcode reader.</param>
        /// <param name="barcodeInfo">A barcode.</param>
        private void VerifyBarcode(BarcodeReader reader, IBarcodeInfo barcodeInfo)
        {
            // if barcode recognition confidence is not less than 95
            if (barcodeInfo.Confidence >= 95)
                // get time when barcode was recognized
                _lastRecognizedBarcodeTime = DateTime.Now;

            // if barcodes should be recognized automaticaly
            if (reader.Settings.AutomaticRecognition)
            {
                // for each recognized barcode
                foreach (IBarcodeInfo recognizedBarcode in RecognizedBarcodes)
                {
                    // if new barcode is equal to the recognized barcode
                    if (recognizedBarcode.StructureEqualsTo(barcodeInfo))
                    {
                        // set confidence "-1" to new recognized barcode
                        barcodeInfo.Confidence = -1;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// BarcodeScanner.FrameScanFinished event handler.
        /// </summary>
        private void BarcodeScanner_FrameScanFinished(object sender, FrameScanFinishedEventArgs e)
        {
            _mainActivity.RunOnUiThread(() =>
            {
                OnFrameScanFinished(e);
            });
        }

        /// <summary>
        /// Returns information about scanner mode.
        /// </summary>
        /// <param name="frame">An image frame.</param>
        /// <param name="scanSettings">A scanner settings.</param>
        private string GetScannerModeInfo(ImageSource frame, ReaderSettings scanSettings)
        {
            string scanMode;
            if (scanSettings.AutomaticRecognition)
                scanMode = _scanModeAutoMessage;
            else if (scanSettings.ThresholdMode == ThresholdMode.Automatic)
                scanMode = string.Format(_scanModeTresholdAutoMessage, scanSettings.Threshold);
            else
                scanMode = string.Format(_scanModeTresholdMessage, scanSettings.Threshold);
            string result = string.Format(_helpInfoBarcodeRecognizedMessage, frame.Width, frame.Height, scanSettings.ScanInterval, scanMode);

            return result;
        }

        /// <summary>
        /// Returns information about FPS.
        /// </summary>
        private string GetFpsInfo()
        {
            if (_fps > 0)
                return string.Format(_fpsMessage, Math.Round(_fps / (FPS_MEASURING_TIME / 1000f)));
            return string.Format(_fpsMessage, "?");
        }

        /// <summary>
        /// Barcode scanning is finished.
        /// </summary>
        private void OnFrameScanFinished(FrameScanFinishedEventArgs e)
        {
            _scanIterations++;

            // if timer is not running
            if (!_timer.IsRunning)
                // start timer
                _timer.Start();
            // if elapsed time is not less than FPS_MEASURING_TIME
            if (_timer.ElapsedMilliseconds >= FPS_MEASURING_TIME)
            {
                // restart timer
                _timer.Restart();
                // get scan iterations
                _fps = _scanIterations;
                // reset scan iterations
                _scanIterations = 0;
            }

            UpdateStateInfo(e.Frame);

            List<IBarcodeInfo> newBarcodes = new List<IBarcodeInfo>();
            // for each new barcode
            foreach (IBarcodeInfo newBarcodeInfo in e.RecognizedBarcodes)
            {
                // if new barcode has not already recognized
                if (!ContainsBarcode(RecognizedBarcodes, newBarcodeInfo) &&
                    !ContainsBarcode(newBarcodes, newBarcodeInfo))
                {
                    // add new barcode to the new barcodes list
                    newBarcodes.Add(newBarcodeInfo);
                }
            }

            // if there are new barcodes
            if (newBarcodes.Count > 0)
            {
                // if sound signal should be played
                if (_soundSignal && _mediaPlayer != null)
                {
                    try
                    {
                        // play sound signal
                        _mediaPlayer.Start();
                    }
                    catch (Exception ex)
                    {
                        Toast.MakeText(_mainActivity, string.Format("Media player {0}", ex.Message), ToastLength.Short).Show();
                    }
                }

                // if "Vibrate" permission is granted
                if (_isVibratePermissionGranted)
                {
                    // if vibration should be played
                    if (_vibration && _vibrator != null)
                    {
                        try
                        {
                            _vibrator.Vibrate(VibrationEffect.CreateOneShot(VIBRATE_TIME, VibrationEffect.DefaultAmplitude));
                        }
                        catch (Exception ex)
                        {
                            Toast.MakeText(_mainActivity, string.Format("Vibrator {0}", ex.Message), ToastLength.Short).Show();
                        }
                    }
                }

                // add new barcodes to recognized barcode list
                RecognizedBarcodes.InsertRange(0, newBarcodes);

                // if application should reconstruct composite structured append barcodes
                if (ReconstructStructuredAppendBarcodes)
                {
                    IEnumerable<IBarcodeInfo> unusedBarcodes;
                    // reconstract barcodes
                    StructuredAppendBarcodeInfo[] structuredAppendBarcodes = StructuredAppendBarcodeInfo.ReconstructFrom(RecognizedBarcodes, out unusedBarcodes);
                    // for each reconstracted barcode
                    foreach (StructuredAppendBarcodeInfo barcode in structuredAppendBarcodes)
                    {
                        // if recognized barcode list does not contain reconstracted barcode
                        if (!ContainsBarcode(RecognizedBarcodes, barcode))
                        {
                            // add reconstracted barcode to recognized barcode list
                            RecognizedBarcodes.Insert(0, barcode);
                            newBarcodes.Insert(0, barcode);
                        }
                    }
                }

                // if frame scanning should be stoped
                if (_stopScan)
                {
                    // for each new barcode
                    foreach (IBarcodeInfo barcode in newBarcodes)
                        // add new barcode to new recognized barcode list
                        _newRecognizedBarcodes.Add(barcode, e.Frame);
                }
                else
                {
                    if (_copyToClipboard && RecognizedBarcodes.Count > 0)
                    {
                        // get recognized barcode
                        IBarcodeInfo info = RecognizedBarcodes[0];
                        // if recognized barcode is not empty
                        if (info != null)
                        {
                            // get barcode value
                            string text = info.Value;
                            // get clipboard
                            ClipboardManager clipboard = (ClipboardManager)_mainActivity.GetSystemService(Android.Content.Context.ClipboardService);
                            // create data for clipboard
                            ClipData clip = ClipData.NewPlainText("barcodeValue", text);
                            // put data to clipboard
                            clipboard.PrimaryClip = clip;
                        }
                    }
                }

                ChangeClearButtonState(true);

                // get scanner info
                string scannerInfo = GetScannerModeInfo(e.Frame, e.ScanSettings);
                // show scanner info
                _helpInfoTextView.Text = string.Format(_frameScanTimeMessage, scannerInfo, e.ScanTime.TotalMilliseconds);

                // if activity was started from another application
                if (_mainActivity.IntentSource == Intents.IntentSource.NAITIVE_APP_INTENT)
                {
                    // get first new recognized barcode
                    IBarcodeInfo barcode = newBarcodes[0];
                    Intent intent = _mainActivity.Intent;
                    intent.AddFlags(ActivityFlags.ClearWhenTaskReset);
                    // return barcode value to the app
                    intent.PutExtra(Intents.Scan.RESULT, barcode.Value);
                    // return barcode type in string representation to the app
                    intent.PutExtra(Intents.Scan.RESULT_FORMAT, Utils.GetBarcodeTypeString(barcode));
                    _mainActivity.SetResult(Result.Ok, intent);
                    // finish barcode scanner activity
                    _mainActivity.Finish();
                }

                // if recognized barcode is showing
                if (ShowNextRecognizedBarcode())
                {
                    // stop the barcode scanning
                    _barcodeScanner.StopScanning();
                }
                else
                {
                    UpdateRecognizedBarcodesInfo();
                }
            }

            // if camera autofocus is enabled
            // and the barcode scanning is started
            if (_cameraController.ContinuousAutofocus && _barcodeScanner.IsScanningStarted)
            {
                // if elapsed time since last barcode recognition is greater than autofocus unterval time
                if ((DateTime.Now - _lastRecognizedBarcodeTime).TotalMilliseconds > AUTOFOCUS_INTERVAL)
                {
                    // if elapsed time since last autofocusing is greater than autofocus unterval time
                    if ((DateTime.Now - _lastAutofocusTime).TotalMilliseconds > AUTOFOCUS_INTERVAL)
                    {
                        _lastAutofocusTime = DateTime.Now;
                        _barcodeScannerOverlay.Autofocusing = true;
                        // request autofocus
                        _cameraController.RequestAutoFocus();
                    }
                }
            }
        }

        /// <summary>
        /// Updates the state information.
        /// </summary>
        /// <param name="frame">The scanning frame.</param>
        private void UpdateStateInfo(ImageSource frame)
        {
            _helpInfoTextView.Text = string.Format(_helpInfoMessage, GetFpsInfo(), frame.Width, frame.Height);
        }

        /// <summary>
        /// Updates recognized barcode info.
        /// </summary>
        private void UpdateRecognizedBarcodesInfo()
        {
            if (RecognizedBarcodes != null)
                _recognizedBarcodeTextView.Text = string.Format(_recognizedBarcodesMessage, RecognizedBarcodes.Count);
            else
                _recognizedBarcodeTextView.Text = "";
            _recognizedBarcodeTextView.ScrollTo(0, 0);
        }

        /// <summary>
        /// Returns a value, which determines whether <paramref name="barcodes"/> list contains <paramref name="newBarcodeInfo"/>.
        /// </summary>
        /// <param name="barcodes">Barcode list.</param>
        /// <param name="newBarcodeInfo">New barcode.</param>
        /// <returns></returns>
        private bool ContainsBarcode(List<IBarcodeInfo> barcodes, IBarcodeInfo newBarcodeInfo)
        {
            foreach (IBarcodeInfo info in barcodes)
            {
                if (info.StructureEqualsTo(newBarcodeInfo))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Shows information about next recognized barcode.
        /// </summary>
        /// <returns>
        /// <b>true</b> - if unshown barcode is remained; otherwise - <b>false</b>.
        /// </returns>
        private bool ShowNextRecognizedBarcode()
        {
            // recognized barcode
            IBarcodeInfo barcode = null;
            // recognized barcode image
            ImageSource frame = null;
            foreach (IBarcodeInfo inf in _newRecognizedBarcodes.Keys)
            {
                barcode = inf;
                frame = _newRecognizedBarcodes[inf];
            }
            // if there is no recognized barcode
            if (barcode == null)
                return false;

            // remove last shown barcode
            _newRecognizedBarcodes.Remove(barcode);

            // determines whether barcode information should be shown
            bool needShowInfo = true;
            // if application should reconstruct composite structured append barcodes
            if (ReconstructStructuredAppendBarcodes)
            {
                // if value items is not empty
                // and first value item is structed append character
                ValueItemBase[] valueItems = barcode.ValueItems;
                if (valueItems != null && valueItems.Length > 1 && valueItems[0] is StructuredAppendCharacter)
                {
                    // indicate that barcode information shouldn't be shown
                    needShowInfo = false;
                }
            }

            // if barcode information shouldn't be shown
            if (!needShowInfo)
                // show next recognized barcode
                return ShowNextRecognizedBarcode();

            // set recognized barcode to the overlay view
            _barcodeScannerOverlay.SetRecognizedBarcode(barcode, frame);
            // show recognized barcode information
            ShowRecognizedBarcodesInfo();

            return true;
        }

        /// <summary>
        /// Shows recognized barcode info.
        /// </summary>
        private void ShowRecognizedBarcodesInfo()
        {
            string text = "";
            // get recognized barcode
            IBarcodeInfo info = _barcodeScannerOverlay.RecognizedBarcode;


            // if recognized barcode is not empty
            if (info != null)
            {
                try
                {
                    // get barcode value
                    text = Utils.GetEncodedBarcodeValue(info, _textEncodingName);
                }
                catch (NotSupportedException)
                {
                    Toast.MakeText(
                        _mainActivity,
                        string.Format(Resources.GetString(Resource.String.unsupported_encoding_message), Utils.AvailableEncodings[_textEncodingName].Name),
                        ToastLength.Long).Show();

                    text = Utils.GetEncodedBarcodeValue(info, "-1");
                }

                // if barcode value should be copyed
                if (_copyToClipboard)
                {
                    // get clipboard
                    ClipboardManager clipboard = (ClipboardManager)_mainActivity.GetSystemService(Android.Content.Context.ClipboardService);
                    // create data for clipboard
                    ClipData clip = ClipData.NewPlainText("barcodeValue", text);
                    // put data to clipboard
                    clipboard.PrimaryClip = clip;
                }
            }

            // show recognized barcode value
            _recognizedBarcodeTextView.Text = text;
            _recognizedBarcodeTextView.ScrollTo(0, 0);
        }

        /// <summary>
        /// Returns child layout parameters.
        /// </summary>
        /// <returns>Child layout parameters.</returns>
        private LinearLayout.LayoutParams GetChildLayoutParams()
        {
            var layoutParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);

            layoutParams.Weight = 1;
            return layoutParams;
        }

        #endregion

        #endregion

    }
}
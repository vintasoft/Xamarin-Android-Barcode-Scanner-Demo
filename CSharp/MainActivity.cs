using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Text.Method;
using Android.Util;
using Android.Views;
using Android.Widget;

using Java.Util;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Vintasoft.XamarinBarcode;
using Vintasoft.XamarinBarcode.BarcodeInfo;

namespace BarcodeScannerDemo
{
    /// <summary>
    /// The main activity.
    /// </summary>
    [Activity(Label = "VintaSoft Barcode Scanner", MainLauncher = true, Icon = "@mipmap/icon",
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.KeyboardHidden,
        Theme = "@style/AppTheme",
        ScreenOrientation = ScreenOrientation.Landscape)]
    [IntentFilter(new[] { Intents.Scan.ACTION },
        Categories = new[] { Intent.CategoryDefault })]
    public class MainActivity : AppCompatActivity
    {

        #region Constants

        /// <summary>
        /// The request code for permissions.
        /// </summary>
        const int PERMISSION_REQUEST_CODE = 100;

        /// <summary>
        /// The barcode scanner fragment tag.
        /// </summary>
        string BARCODE_SCANNER_FRAGMENT_TAG = "BARCODESCANNERFRAGMENT";

        /// <summary>
        /// The history fragment tag.
        /// </summary>
        string HISTORY_FRAGMENT_TAG = "HISTORYFRAGMENT";

        /// <summary>
        /// The settings fragment tag.
        /// </summary>
        string SETTINGS_FRAGMENT_TAG = "SETTINGSFRAGMENT";

        #endregion



        #region Fields

        /// <summary>
        /// The barcode scanner fragment.
        /// </summary>
        BarcodeScannerFragment _barcodeScannerFragment;

        /// <summary>
        /// The history fragment.
        /// </summary>
        HistoryFragment _historyFragment;

        /// <summary>
        /// The settings fragment.
        /// </summary>
        SettingsFragment _settingsFragment;


        /// <summary>
        /// The info dialog.
        /// </summary>
        Android.Support.V7.App.AlertDialog _infoDialog = null;

        /// <summary>
        /// The positive button in info dialog.
        /// </summary>
        Button _infoDialogPositiveButton = null;

        /// <summary>
        /// The neutral button in info dialog.
        /// </summary>
        Button _infoDialogNeutralButton = null;

        /// <summary>
        /// A value to copy from info dialog.
        /// </summary>
        string _infoDialogValueToCopy = null;

        /// <summary>
        /// Determines that "Camera" permission is granted.
        /// </summary>
        bool _isCameraPermissionGranted = false;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="MainActivity"/> class.
        /// </summary>
        public MainActivity()
            : base()
        {
            VintasoftBarcode.VintasoftXamarinBarcodeLicense.Register();
        }

        #endregion



        #region Properties

        List<IBarcodeInfo> _recognizedBarcodes = new List<IBarcodeInfo>();
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

        Intents.IntentSource _intentSource = Intents.IntentSource.NONE;
        /// <summary>
        /// Gets the intent source.
        /// </summary>
        internal Intents.IntentSource IntentSource
        {
            get
            {
                return _intentSource;
            }
        }

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Called when a key was pressed down and not handled by any of the views inside of the activity.
        /// </summary>
        /// <param name="keyCode">The value in event.getKeyCode().</param>
        /// <param name="e">Description of the key event.</param>
        /// <returns>
        /// <b>false</b> - indicates that this event is not handled and should be propagated; otherwise - <b>true</b>.
        /// </returns>
        public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            switch (e.KeyCode)
            {
                // if Back key is pressed
                case Keycode.Back:
                    // get barcode scanner fragment
                    Android.Support.V4.App.Fragment barcodeScannerFragment = SupportFragmentManager.FindFragmentByTag(BARCODE_SCANNER_FRAGMENT_TAG);
                    // if barcode scanner fragment is found and barcode scanner fragment is visible
                    if (barcodeScannerFragment != null && barcodeScannerFragment.IsVisible)
                    {
                        // if this activity was called by another application
                        if (IntentSource == Intents.IntentSource.NAITIVE_APP_INTENT)
                        {
                            // set result - canceled
                            SetResult(Result.Canceled);
                            // finish activity
                            Finish();
                            return true;
                        }
                        if (_barcodeScannerFragment.BackKeyPressed())
                            return true;
                    }
                    else
                    {
                        // set default window settings
                        SetDefaultWindowSettings();
                    }
                    break;
            }

            return base.OnKeyDown(keyCode, e);
        }

        #endregion


        #region PROTECTED

        /// <summary>
        /// Called when the activity is starting.
        /// </summary>
        /// <param name="savedInstanceState">
        /// If the activity is being re-initialized after previously being shut down then
        /// this <see cref="Bundle"/> contains the data it most recently supplied in 
        /// <see cref="Android.App.Activity.OnSaveInstanceState(Android.OS.Bundle)"/>.
        /// Note: Otherwise it is <b>null</b>.
        /// </param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            // subscribe to the unhandled exception events
            AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironment_UnhandledExceptionRaiser;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            base.OnCreate(savedInstanceState);

            // add "fullscreen" window flag
            Window.AddFlags(WindowManagerFlags.Fullscreen);

            // set our view from the "main" layout resource
            SetContentView(Resource.Layout.main);

            try
            {
                // set the default values from an XML preference file
                PreferenceManager.SetDefaultValues(this, Resource.Xml.settings_page, false);

                // get preferences
                ISharedPreferences preferences = PreferenceManager.GetDefaultSharedPreferences(this);
                // get the language name
                string languageToLoad = preferences.GetString("list_languages", "auto");
                // if language name is not auto
                if (languageToLoad != "auto")
                {
                    // set choosen locale
                    Locale locale = new Locale(languageToLoad);
                    Locale.Default = locale;
                    Configuration config = new Configuration();
                    config.Locale = locale;
                    BaseContext.Resources.UpdateConfiguration(config, BaseContext.Resources.DisplayMetrics);

                    this.SetContentView(Resource.Layout.main);
                }
            }
            catch
            {
            }

            SupportActionBar.SetHomeButtonEnabled(false);
            SupportActionBar.SetDisplayHomeAsUpEnabled(false);

            // check licence
            CheckLicense();

            // if Android version is equal or higher than 6.0 (API 23)
            if (Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                if (CheckSelfPermission(Android.Manifest.Permission.Camera) != Permission.Granted ||
                    CheckSelfPermission(Android.Manifest.Permission.Flashlight) != Permission.Granted ||
                    CheckSelfPermission(Android.Manifest.Permission.Vibrate) != Permission.Granted)
                {
                    RequestPermissions(
                        new string[] { Android.Manifest.Permission.Camera, Android.Manifest.Permission.Flashlight, Android.Manifest.Permission.Vibrate },
                        PERMISSION_REQUEST_CODE);
                }
                else
                {
                    _isCameraPermissionGranted = true;

                    // create fragments
                    _barcodeScannerFragment = new BarcodeScannerFragment(this, RecognizedBarcodes, true, true);
                    _historyFragment = new HistoryFragment(this);
                    _settingsFragment = new SettingsFragment(this);

                    // show barcode scanner fragment
                    SwitchToBarcodeScanner(null);
                }
            }
            // if Android version is less than 6.0 (API 23)
            else
            {
                _isCameraPermissionGranted = true;

                // create fragments
                _barcodeScannerFragment = new BarcodeScannerFragment(this, RecognizedBarcodes, true, true);
                _historyFragment = new HistoryFragment(this);
                _settingsFragment = new SettingsFragment(this);

                // show barcode scanner fragment
                SwitchToBarcodeScanner(null);
            }
        }

        /// <summary>
        /// Checks active license.
        /// </summary>
        private void CheckLicense()
        {
            try
            {
                BarcodeReader reader = new BarcodeReader();
                reader.ReadBarcodes(new GrayscaleImageSource(1, 1, new byte[1]));
            }
            catch (Exception ex)
            {
                ShowInfoDialog(ex.GetType().ToString(), ex.Message);
            }
        }

        /// <summary>
        /// Called when request permissions result occured.
        /// </summary>
        /// <param name="requestCode">The request code.</param>
        /// <param name="permissions">The permissions.</param>
        /// <param name="grantResults">The grant results.</param>
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == PERMISSION_REQUEST_CODE)
            {
                bool isFlashlightPermissionGranted = false;
                if (grantResults[1] == Permission.Granted)
                {
                    isFlashlightPermissionGranted = true;
                }

                bool isVibratePermissionGranted = false;
                if (grantResults[2] == Permission.Granted)
                {
                    isVibratePermissionGranted = true;
                }

                if (grantResults[0] == Permission.Granted)
                {
                    // create fragments
                    _barcodeScannerFragment = new BarcodeScannerFragment(this, RecognizedBarcodes, isFlashlightPermissionGranted, isVibratePermissionGranted);
                    _historyFragment = new HistoryFragment(this);
                    _settingsFragment = new SettingsFragment(this);

                    _isCameraPermissionGranted = true;
                    // show barcode scanner fragment
                    SwitchToBarcodeScanner(null);
                    // open camera
                    OpenCamera();
                }
                else
                {
                    // close application
                    Finish();
                }
            }
        }

        /// <summary>
        /// Opens the camera device.
        /// </summary>
        private void OpenCamera()
        {
            if (_isCameraPermissionGranted)
            {
                try
                {
                    // open camera
                    _barcodeScannerFragment.CameraController.OpenCamera();
                }
                catch (Exception ex)
                {
                    Toast.MakeText(this, string.Format("Camera exception: {0}", ex.Message), ToastLength.Short).Show();
                }
            }
        }

        /// <summary>
        /// Activity is started.
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();           

            OpenCamera();
        }

        /// <summary>
        /// Activity is stoped.
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();

            if (_barcodeScannerFragment != null)
            {
                // close camera
                _barcodeScannerFragment.CameraController.CloseCamera();
            }
        }

        /// <summary>
        /// Activity is resumed.
        /// </summary>
        protected override void OnResume()
        {
            base.OnResume();

            // if intent is not empty
            if (Intent != null)
            {
                // get the intent action
                string action = Intent.Action;

                // if action is scan action or view action
                if (action == Intents.Scan.ACTION || action == Intent.ActionView)
                {
                    // specify that application is executed from another application
                    _intentSource = Intents.IntentSource.NAITIVE_APP_INTENT;

                    // show the About dialog
                    ShowAboutDialog();
                }
            }
        }

        #endregion


        #region INTERNAL

        /// <summary>
        /// Switches from <paramref name="lastAttachedFragment"/> to the <see cref="BarcodeScannerFragment"/> instance.
        /// </summary>
        /// <param name="lastAttachedFragment">Last attached fragment.</param>
        internal void SwitchToBarcodeScanner(Android.Support.V4.App.Fragment lastAttachedFragment)
        {
            // set default window settings
            SetDefaultWindowSettings();

            // create a new transaction
            Android.Support.V4.App.FragmentTransaction transaction = SupportFragmentManager.BeginTransaction();
            // if last attached fragment is not empty
            if (lastAttachedFragment != null)
                transaction.Detach(lastAttachedFragment);
            // if barcode scanner fragment does not exist
            if (SupportFragmentManager.FindFragmentByTag(BARCODE_SCANNER_FRAGMENT_TAG) == null)
                // add fragment to the container
                transaction.Add(Resource.Id.contentFrame, _barcodeScannerFragment, BARCODE_SCANNER_FRAGMENT_TAG);
            // if barcode scanner fragment exists
            else
                // attach fragment to the container
                transaction.Attach(_barcodeScannerFragment);
            // commit the transaction
            transaction.Commit();
        }

        /// <summary>
        /// Switches from <paramref name="lastAttachedFragment"/> to the <see cref="BarcodeScannerFragment"/> instance.
        /// </summary>
        /// <param name="lastAttachedFragment">Last attached fragment.</param>
        internal void SwitchToHistory(Android.Support.V4.App.Fragment lastAttachedFragment)
        {
            // clear window flags
            Window.ClearFlags(WindowManagerFlags.KeepScreenOn);

            // show action bar
            SupportActionBar.Show();
            // set action bar title
            SupportActionBar.SetTitle(Resource.String.history_title);

            // create a new transaction
            Android.Support.V4.App.FragmentTransaction transaction = SupportFragmentManager.BeginTransaction();
            // if last attached fragment is not empty
            if (lastAttachedFragment != null)
                transaction.Detach(lastAttachedFragment);
            // if history fragment does not exist
            if (SupportFragmentManager.FindFragmentByTag(HISTORY_FRAGMENT_TAG) == null)
                // add fragment to the container
                transaction.Add(Resource.Id.contentFrame, _historyFragment, HISTORY_FRAGMENT_TAG);
            // if history fragment exists
            else
                // attach fragment to the container
                transaction.Attach(_historyFragment);
            // add transaction to back stack
            transaction.AddToBackStack(null);
            // commit the transaction
            transaction.Commit();
        }

        /// <summary>
        /// Switches from <paramref name="lastAttachedFragment"/> to the <see cref="BarcodeScannerFragment"/> instance.
        /// </summary>
        /// <param name="lastAttachedFragment">Last attached fragment.</param>
        /// <param name="cameraController">A camera controller.</param>
        internal void SwitchToSettings(Android.Support.V4.App.Fragment lastAttachedFragment, CameraController cameraController)
        {
            // clear window flags
            Window.ClearFlags(WindowManagerFlags.KeepScreenOn);

            // show action bar
            SupportActionBar.Show();
            // set action bar title
            SupportActionBar.SetTitle(Resource.String.settings_title);

            // create a new transaction
            Android.Support.V4.App.FragmentTransaction transaction = SupportFragmentManager.BeginTransaction();
            // if last attached fragment is not empty
            if (lastAttachedFragment != null)
                transaction.Detach(lastAttachedFragment);
            // if settings fragment does not exist
            if (SupportFragmentManager.FindFragmentByTag(SETTINGS_FRAGMENT_TAG) == null)
            {
                // add fragment to the container
                transaction.Add(Resource.Id.contentFrame, _settingsFragment, SETTINGS_FRAGMENT_TAG);
                // set camera controller to the settings fragment
                _settingsFragment.CameraController = cameraController;
            }
            // if settings fragment exists
            else
            {
                // attach fragment to the container
                transaction.Attach(_settingsFragment);
            }
            // add transaction to back stack
            transaction.AddToBackStack(null);
            // commit the transaction
            transaction.Commit();
        }


        /// <summary>
        /// Returns a string with extended barcode info.
        /// </summary>
        /// <param name="barcodeInfo">A barcode info.</param>
        /// <param name="textEncodingName">A text encoding name.</param>
        /// <returns>
        /// A string with extended barcode info.
        /// </returns>
        internal string GetExtendedBarcodeInfoString(IBarcodeInfo barcodeInfo, string textEncodingName)
        {
            // if barcode info is empty
            if (barcodeInfo == null)
                return string.Empty;

            IBarcodeInfo info;
            if (barcodeInfo is BarcodeSubsetInfo)
            {
                info = ((BarcodeSubsetInfo)barcodeInfo).BaseBarcodeInfo;
            }
            else
            {
                info = barcodeInfo;
            }

            StringBuilder extendedInfo = new StringBuilder();

            // append barcode value
            try
            {
                extendedInfo.AppendLine(Utils.GetEncodedBarcodeValue(barcodeInfo, textEncodingName));
            }
            catch (NotSupportedException)
            {
                extendedInfo.AppendLine(Utils.GetEncodedBarcodeValue(barcodeInfo, "-1"));
            }
            extendedInfo.AppendLine();
            extendedInfo.AppendLine(Resources.GetString(Resource.String.metadata_message));
            extendedInfo.AppendLine();

            // if barcode is 2D barcode
            if (info is BarcodeInfo2D)
            {
                // get 2D barcode info
                BarcodeInfo2D info2D = (BarcodeInfo2D)info;
                // get matrix size
                extendedInfo.AppendLine(string.Format(Resources.GetString(Resource.String.matrix_size_message), info2D.MatrixWidth, info2D.MatrixHeight));
                // get cell size
                extendedInfo.AppendLine(string.Format(CultureInfo.InvariantCulture, Resources.GetString(Resource.String.cell_size_message),
                    info2D.CellWidth, info2D.CellHeight));
                if (info2D.IsMirrored)
                {
                    extendedInfo.AppendLine(string.Format(Resources.GetString(Resource.String.mirrored_message), info2D.IsMirrored));
                }
            }
            // if barcode is 1D barcode
            if (info is BarcodeInfo1D)
            {
                // get 1D barcode info
                BarcodeInfo1D info1D = (BarcodeInfo1D)info;
                // get narrow bar count
                extendedInfo.AppendLine(string.Format(Resources.GetString(Resource.String.narrow_bar_count_message), info1D.NarrowBarCount));
                // get narrow bar size
                extendedInfo.AppendLine(string.Format(CultureInfo.InvariantCulture, Resources.GetString(Resource.String.narrow_bar_size_message), info1D.NarrowBarSize));
            }

            if (info.SymbolComponents != null)
            {
                extendedInfo.AppendLine(string.Format(Resources.GetString(Resource.String.symbol_component_count_message), info.SymbolComponents.Length));
            }

            if (info is StructuredAppendBarcodeInfo)
            {
                extendedInfo.AppendLine(Resources.GetString(Resource.String.reconstructed_message));
            }
            else
            {
                switch (info.BarcodeType)
                {
                    // if barcode is PDF417
                    case BarcodeType.PDF417:
                    case BarcodeType.PDF417Compact:
                        // get PDF417 barcode info
                        PDF417Info pdf417Info = (PDF417Info)info;
                        // get rows count
                        extendedInfo.AppendLine(string.Format(Resources.GetString(Resource.String.rows_count_message), pdf417Info.RowsCount));
                        // get error correction level
                        extendedInfo.AppendLine(string.Format(Resources.GetString(Resource.String.error_correction_level_message),
                            Utils.PDF417ErrorCorrectionLevelToString(pdf417Info.ErrorCorrectionLevel)));
                        break;
                    // if barcode is MicroPDF417
                    case BarcodeType.MicroPDF417:
                        // get MicroPDF417 barcode info
                        MicroPDF417Info microPdf417Info = (MicroPDF417Info)info;
                        // get symbol type
                        extendedInfo.AppendLine(string.Format(Resources.GetString(Resource.String.symbol_type_message),
                            Utils.MicroPDF417SymbolTypeToString(microPdf417Info.SymbolType)));
                        break;
                    // if barcode if DataMatrix
                    case BarcodeType.DataMatrix:
                        // get DataMatrix barcode info
                        DataMatrixInfo dataMatrixInfo = (DataMatrixInfo)info;
                        // get symbol type
                        extendedInfo.AppendLine(string.Format(Resources.GetString(Resource.String.symbol_type_message),
                            Utils.DataMatrixSymbolTypeToString(dataMatrixInfo.Symbol.SymbolType)));
                        break;
                    // if barcode is QR
                    case BarcodeType.MicroQR:
                    case BarcodeType.QR:
                        // get QR barcode info
                        QRInfo qrInfo = (QRInfo)info;
                        // get symbol version
                        extendedInfo.AppendLine(string.Format(Resources.GetString(Resource.String.symbol_version_message),
                            Utils.QRSymbolVersionToString(qrInfo.Symbol.Version)));
                        // get error correction level
                        extendedInfo.AppendLine(string.Format(Resources.GetString(Resource.String.error_correction_level_message),
                            Utils.QRErrorCorrectionLevelToString(qrInfo.ErrorCorrectionLevel)));
                        break;
                    // if barcode is Aztec
                    case BarcodeType.Aztec:
                        // get Aztec barcode info
                        AztecInfo aztecInfo = (AztecInfo)info;
                        // get symbol type
                        extendedInfo.AppendLine(string.Format(Resources.GetString(Resource.String.symbol_type_message),
                            Utils.AztecSymbolTypeToString(aztecInfo.Symbol.SymbolType)));
                        // get data layers
                        extendedInfo.AppendLine(string.Format(Resources.GetString(Resource.String.data_layers_message), aztecInfo.Symbol.DataLayers));
                        // get error correction data
                        extendedInfo.AppendLine(string.Format(Resources.GetString(Resource.String.error_correction_data_message),
                            aztecInfo.Symbol.ErrorCorrectionData.ToString("F3")));
                        break;
                    // if barcode is HanXin
                    case BarcodeType.HanXinCode:
                        // get HanXin barcode info
                        HanXinCodeInfo hanXinInfo = (HanXinCodeInfo)info;
                        // get symbol version
                        extendedInfo.AppendLine(string.Format(Resources.GetString(Resource.String.symbol_version_message),
                            Utils.HanXinCodeSymbolVersionToString(hanXinInfo.Symbol.Version)));
                        // get error correction level
                        extendedInfo.AppendLine(string.Format(Resources.GetString(Resource.String.error_correction_level_message),
                            Utils.HanXinCodeErrorCorrectionLevelToString(hanXinInfo.Symbol.ErrorCorrectionLevel)));
                        break;
                    // if barcode is MaxiCode
                    case BarcodeType.MaxiCode:
                        // get MaxiCode barcode info
                        MaxiCodeInfo maxiCodeInfo = (MaxiCodeInfo)info;
                        // get encoding mode
                        extendedInfo.AppendLine(string.Format(Resources.GetString(Resource.String.encoding_mode_message),
                            Utils.MaxiCodeEncodingModeToString(maxiCodeInfo.EncodingMode)));
                        break;
                    // if barcode is RSS14
                    case BarcodeType.RSS14:
                    case BarcodeType.RSS14Stacked:
                    case BarcodeType.RSSExpanded:
                    case BarcodeType.RSSExpandedStacked:
                    case BarcodeType.RSSLimited:
                        // get RSS barcode info
                        RSSInfo rssInfo = (RSSInfo)info;
                        // get segments count
                        extendedInfo.AppendLine(string.Format(Resources.GetString(Resource.String.segments_count_message), rssInfo.SegmentsCount));
                        // get rows count
                        extendedInfo.AppendLine(string.Format(Resources.GetString(Resource.String.rows_count_message), rssInfo.RowsCount));
                        // get segments in row
                        extendedInfo.AppendLine(string.Format(Resources.GetString(Resource.String.segments_in_row_message), rssInfo.SegmentsInRow));
                        // get linkage flag
                        extendedInfo.AppendLine(string.Format(Resources.GetString(Resource.String.linkage_flag_message), rssInfo.LinkageFlag));
                        break;
                    // if barcode is UPCE
                    case BarcodeType.UPCE:
                        // get UPCE barcode info
                        UPCEANInfo upcEANInfo = (UPCEANInfo)info;
                        // get UPCE value
                        extendedInfo.AppendLine(string.Format(Resources.GetString(Resource.String.upce_value_message), upcEANInfo.UPCEValue));
                        break;
                }
            }

            extendedInfo.AppendLine(string.Format(CultureInfo.InvariantCulture, Resources.GetString(Resource.String.recognition_quality_message), info.ReadingQuality));

            return extendedInfo.ToString();
        }

        /// <summary>
        /// Shows a dialog.
        /// </summary>
        /// <param name="title">A dialog title.</param>
        /// <param name="value">A dialog value.</param>
        internal void ShowInfoDialog(string title, string value)
        {
            ShowInfoDialog(title, value, false, null);
        }

        /// <summary>
        /// Shows a dialog.
        /// </summary>
        /// <param name="title">A dialog title.</param>
        /// <param name="value">A dialog value.</param>
        /// <param name="canCopy">Indicates that "Copy value" is activated.</param>
        /// <param name="valueToCopy">Value to copy by button.</param>
        internal void ShowInfoDialog(string title, string value, bool canCopy, string valueToCopy)
        {
            // dialog builder
            using (Android.Support.V7.App.AlertDialog.Builder dialogBuilder = new Android.Support.V7.App.AlertDialog.Builder(new ContextThemeWrapper(this, Resource.Style.AlertDialogTheme)))
            {
                // create button
                dialogBuilder.SetPositiveButton(Resources.GetString(Resource.String.ok_button), (EventHandler<DialogClickEventArgs>)null);
                if (canCopy)
                {
                    dialogBuilder.SetNeutralButton(Resources.GetString(Resource.String.copy_value_button), (EventHandler<DialogClickEventArgs>)null);
                    _infoDialogValueToCopy = valueToCopy;
                }
                // create dialog
                _infoDialog = dialogBuilder.Create();

                _infoDialog.Window.SetBackgroundDrawableResource(Android.Resource.Drawable.ScreenBackgroundDarkTransparent);
                // set dialog title
                _infoDialog.SetTitle(title);
                // set dialog message
                _infoDialog.SetMessage(value);
                // show on screen
                _infoDialog.Show();

                // get display size
                DisplayMetrics displayMetrics = new DisplayMetrics();
                WindowManager.DefaultDisplay.GetMetrics(displayMetrics);
                int height = (int)(displayMetrics.HeightPixels * 3 / 4);

                // if dialog content height is greater than 3/4 of screen height
                if (_infoDialog.Window.Attributes.Height > height)
                    _infoDialog.Window.SetLayout(_infoDialog.Window.Attributes.Width, height);

                TextView dialogTextView = _infoDialog.FindViewById<TextView>(Android.Resource.Id.Message);
                // allow to select dialog text
                dialogTextView.SetTextIsSelectable(true);
                // allow to click links
                dialogTextView.MovementMethod = LinkMovementMethod.Instance;
                dialogTextView.LinksClickable = true;
                // add links
                Utils.MyLinkify.AddLinks(dialogTextView, Patterns.EmailAddress, null);
                Utils.MyLinkify.AddLinks(dialogTextView, Patterns.WebUrl, null, new Utils.MyLinkify(), null);

                if (canCopy)
                {
                    _infoDialogNeutralButton = _infoDialog.GetButton((int)DialogButtonType.Neutral);
                    _infoDialogNeutralButton.Click += NeutralButton_Click;
                    _infoDialogPositiveButton = _infoDialog.GetButton((int)DialogButtonType.Positive);
                    _infoDialogPositiveButton.Click += PositiveButton_Click;
                }
            }
        }

        /// <summary>
        /// Shows the About dialog.
        /// </summary>
        internal void ShowAboutDialog()
        {
            // get application name
            string applicationName = Resources.GetString(Resource.String.app_name);

            // get application version
            string applicationVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            // get about message template
            string aboutMessageTemplate = Resources.GetString(Resource.String.about_message);

            // show dialog about barcode scanner demo
            ShowInfoDialog(applicationName, string.Format(aboutMessageTemplate, applicationVersion));
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Sets default window settings.
        /// </summary>
        private void SetDefaultWindowSettings()
        {
            // hide action bar
            SupportActionBar.Hide();
            // add "keep screen on" window flag
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);
        }


        /// <summary>
        /// Copies the barcode value.
        /// </summary>
        private void NeutralButton_Click(object sender, EventArgs e)
        {
            // get clipboard
            ClipboardManager clipboard = (ClipboardManager)GetSystemService(ClipboardService);
            // create data for clipboard
            ClipData clip = ClipData.NewPlainText("barcodeValue", _infoDialogValueToCopy);
            // put data to clipboard
            clipboard.PrimaryClip = clip;
        }

        /// <summary>
        /// Closes the dialog.
        /// </summary>
        private void PositiveButton_Click(object sender, EventArgs e)
        {
            if (_infoDialogNeutralButton != null)
                _infoDialogNeutralButton.Click -= NeutralButton_Click;
            if (_infoDialogPositiveButton != null)
                _infoDialogPositiveButton.Click -= PositiveButton_Click;
            if (_infoDialog != null)
                _infoDialog.Dismiss();
            _infoDialogNeutralButton = null;
            _infoDialogPositiveButton = null;
            _infoDialog = null;
            _infoDialogValueToCopy = null;
        }


        #region Unhandled exceptions

        /// <summary>
        /// Handles an Unhandled exception, which occured when managed exception was translated into an Android throwable.
        /// </summary>
        private void AndroidEnvironment_UnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
        {
            LogUnhandledException(e.Exception);
        }

        /// <summary>
        /// Handles an Unhandled exception.
        /// </summary>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogUnhandledException((Exception)e.ExceptionObject);
        }

        /// <summary>
        /// Saves log file with unhandled exception info.s
        /// </summary>
        /// <param name="exception">An exception.</param>
        private void LogUnhandledException(Exception exception)
        {
            string errorMessage = string.Format("Time: {0}\r\nError: UnhandledExceptionMessage\r\n{1}\r\nStackTrace: {2}",
                DateTime.Now, exception.Message, exception.StackTrace);

            try
            {
                // copy to clipboard if possible
                // get clipboard
                ClipboardManager clipboard = (ClipboardManager)GetSystemService(Android.Content.Context.ClipboardService);
                // create data for clipboard
                ClipData clip = ClipData.NewPlainText("VintasoftBarcodeDemoError", errorMessage);
                // put data to clipboard
                clipboard.PrimaryClip = clip;
            }
            catch (Exception e)
            {
                Toast.MakeText(Application.ApplicationContext, string.Format("Logging Exception {0}", e.Message), ToastLength.Short).Show();
            }
        }

        #endregion

        #endregion

        #endregion

    }
}
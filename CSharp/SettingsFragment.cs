using Android.Content;
using Android.Hardware;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;
using System.Text;

namespace BarcodeScannerDemo
{
    /// <summary>
    /// The fragment that contains user interface for application settings.
    /// </summary>
    public class SettingsFragment : Android.Support.V7.Preferences.PreferenceFragmentCompat, ISharedPreferencesOnSharedPreferenceChangeListener
    {

        #region Fields

        /// <summary>
        /// Main activity.
        /// </summary>
        MainActivity _mainActivity;

        /// <summary>
        /// The camera preview size list preference.
        /// </summary>
        Android.Support.V7.Preferences.ListPreference _cameraPreviewSizesListPreference;

        /// <summary>
        /// Entries for the camera preview size list preference.
        /// </summary>
        List<string> _sizeEntries = new List<string>();

        /// <summary>
        /// Entry values for the camera preview size list preference.
        /// </summary>
        List<string> _sizeEntryValues = new List<string>();


        /// <summary>
        /// The list preference with available text encodings.
        /// </summary>
        Android.Support.V7.Preferences.ListPreference _textEncodingListPreference;

        /// <summary>
        /// Entries for the list preference with available text encodings.
        /// </summary>
        List<string> _encodingEntries = new List<string>();

        /// <summary>
        /// Entry values for the list preference with available text encodings.
        /// </summary>
        List<string> _encodingEntryValues = new List<string>();


        /// <summary>
        /// Previous language value.
        /// </summary>
        string _previousLanguageValue = "";

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="SettingsFragment"/> class.
        /// </summary>
        public SettingsFragment()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SettingsFragment"/> class.
        /// </summary>
        /// <param name="mainActivity">Main activity.</param>
        internal SettingsFragment(MainActivity mainActivity)
            : base()
        {
            _mainActivity = mainActivity;
        }

        #endregion



        #region Properties

        CameraController _cameraController;
        /// <summary>
        /// Gets or sets the camera controller.
        /// </summary>
        internal CameraController CameraController
        {
            get
            {
                return _cameraController;
            }
            set
            {
                _cameraController = value;
            }
        }

        #endregion



        #region Methods

        #region PUBLIC

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            AddPreferencesFromResource(Resource.Xml.settings_page);

            CreateCameraPreviewSizePreference();
            CreateTextEncodingPreference();
        }

        /// <summary>
        /// Called to have the fragment instantiate its user interface view.
        /// </summary>
        /// <param name="inflater">The <see cref="LayoutInflater"/> object that can be used to inflate any views in the fragment.</param>
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
            Context contextThemeWrapper = new ContextThemeWrapper(_mainActivity, Resource.Style.SettingsDialogTheme);
            LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);
            return base.OnCreateView(localInflater, container, savedInstanceState);
        }

        /// <summary>
        /// Preference is clicked.
        /// </summary>
        /// <param name="preference">The clicked preference.</param>
        public override bool OnPreferenceTreeClick(Android.Support.V7.Preferences.Preference preference)
        {
            // if "Reset settings" button is clicked
            if (preference.Key == "set_defaults_button")
            {
                try
                {
                    // get preferences
                    ISharedPreferences preferences = Android.Support.V7.Preferences.PreferenceManager.GetDefaultSharedPreferences(_mainActivity);
                    // save the previous language value
                    _previousLanguageValue = preferences.GetString("list_languages", "auto");
                    // clear the preferences
                    preferences.Edit().Clear().Commit();
                    // refresh the UI
                    RefreshUI();
                }
                catch (System.Exception ex)
                {
                    Toast.MakeText(_mainActivity, string.Format("Settings error: {0}", ex.Message), ToastLength.Short).Show();
                }

                Toast.MakeText(_mainActivity, Resource.String.default_settings_setted_message, ToastLength.Short).Show();
            }

            return base.OnPreferenceTreeClick(preference);
        }

        /// <summary>
        /// Called when the Fragment is visible to the user and actively running.
        /// </summary>
        public override void OnResume()
        {
            base.OnResume();

            try
            {
                Android.Support.V7.Preferences.PreferenceManager.GetDefaultSharedPreferences(_mainActivity).RegisterOnSharedPreferenceChangeListener(this);
            }
            catch (System.Exception ex)
            {
                Toast.MakeText(_mainActivity, string.Format("Settings error: {0}", ex.Message), ToastLength.Short).Show();
            }
        }

        /// <summary>
        /// Called when the Fragment is no longer resumed.
        /// </summary>
        public override void OnPause()
        {
            try
            {
                Android.Support.V7.Preferences.PreferenceManager.GetDefaultSharedPreferences(_mainActivity).UnregisterOnSharedPreferenceChangeListener(this);
            }
            catch (System.Exception ex)
            {
                Toast.MakeText(_mainActivity, string.Format("Settings error: {0}", ex.Message), ToastLength.Short).Show();
            }

            base.OnPause();
        }

        /// <summary>
        /// Called when a shared preference is changed, added, or removed.
        /// </summary>
        /// <param name="sharedPreferences">Changed shared preference.</param>
        /// <param name="key">The key of the preference that was changed, added, or removed.</param>
        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            // if language is changed
            if (key == "list_languages")
            {
                try
                {
                    // get new language value
                    string newLanguageValue = sharedPreferences.GetString(key, "auto");
                    // if new language differs from the previous language
                    if (newLanguageValue != _previousLanguageValue)
                    {
                        // notify user that application must be restarted
                        _mainActivity.ShowInfoDialog(
                            Resources.GetString(Resource.String.app_name),
                            Resources.GetString(Resource.String.language_change_message));
                        _previousLanguageValue = newLanguageValue;
                    }
                }
                catch
                {
                    // notify user that application must be restarted
                    Toast.MakeText(_mainActivity, Resource.String.language_change_message, ToastLength.Long).Show();
                }
            }
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Refreshes the preference fragment view.
        /// </summary>
        private void RefreshUI()
        {
            base.PreferenceScreen.RemoveAll();

            AddPreferencesFromResource(Resource.Xml.settings_page);

            // get the preference for camera preview sizes list
            _cameraPreviewSizesListPreference = FindPreference("list_camera_preview_sizes") as Android.Support.V7.Preferences.ListPreference;
            // if camera preview size list preference is found
            if (_cameraPreviewSizesListPreference != null)
            {
                // add the camera preview sizes to the list preference with camera preview sizes
                _cameraPreviewSizesListPreference.SetEntries(_sizeEntries.ToArray());
                _cameraPreviewSizesListPreference.SetEntryValues(_sizeEntryValues.ToArray());
                // set the "Auto" value as the default value
                _cameraPreviewSizesListPreference.SetValueIndex(0);
            }

            // get the preference for text encoding list
            _textEncodingListPreference = FindPreference("list_encodings") as Android.Support.V7.Preferences.ListPreference;

            // if text encoding list preference is found
            if (_textEncodingListPreference != null)
            {
                // add the encoding to the list preference with text encodings
                _textEncodingListPreference.SetEntries(_encodingEntries.ToArray());
                _textEncodingListPreference.SetEntryValues(_encodingEntryValues.ToArray());
                // set the "Default" value as the default value
                _textEncodingListPreference.SetValueIndex(0);
            }
        }

        /// <summary>
        /// Creates the text encoding preference.
        /// </summary>
        private void CreateTextEncodingPreference()
        {
            // clear information about text encodings
            _encodingEntries.Clear();
            _encodingEntryValues.Clear();

            // get the preference for text encoding list
            _textEncodingListPreference = FindPreference("list_encodings") as Android.Support.V7.Preferences.ListPreference;
            // if text encoding list preference is found
            if (_textEncodingListPreference != null)
            {
                // add "Default" value to the text encoding
                _encodingEntries.AddRange(_textEncodingListPreference.GetEntries());
                _encodingEntryValues.AddRange(_textEncodingListPreference.GetEntryValues());

                // for each available encoding
                foreach (KeyValuePair<string, EncodingInfo> encoding in Utils.AvailableEncodings)
                {
                    // add encoding name to the encoding list
                    _encodingEntries.Add(string.Format("{0}: {1}", encoding.Value.CodePage, encoding.Value.Name));
                    _encodingEntryValues.Add(encoding.Key);
                }

                // add information about available text encodings to the list preference with text encodings
                _textEncodingListPreference.SetEntries(_encodingEntries.ToArray());
                _textEncodingListPreference.SetEntryValues(_encodingEntryValues.ToArray());
            }
        }

        /// <summary>
        /// Creates the camera preview size preference.
        /// </summary>
        private void CreateCameraPreviewSizePreference()
        {
            // clear information about camera preview sizes
            _sizeEntries.Clear();
            _sizeEntryValues.Clear();

            // get the preference for camera preview sizes list
            _cameraPreviewSizesListPreference = FindPreference("list_camera_preview_sizes") as Android.Support.V7.Preferences.ListPreference;
            // if camera preview size list preference is found
            if (_cameraPreviewSizesListPreference != null)
            {
                // add "Auto" value to the camera preview sizes
                _sizeEntries.AddRange(_cameraPreviewSizesListPreference.GetEntries());
                _sizeEntryValues.AddRange(_cameraPreviewSizesListPreference.GetEntryValues());

                // if camera controller is not emtpy and camera preview size list is not empty
                if (CameraController != null && CameraController.CameraPreviewSizes != null)
                {
                    int sizeIndex = 0;
                    // for each camera preview size
                    foreach (Camera.Size size in CameraController.CameraPreviewSizes)
                    {
                        // add camera preview size to the camera preview sizes
                        _sizeEntries.Add(string.Format("{0}x{1}", size.Width, size.Height));
                        _sizeEntryValues.Add(sizeIndex.ToString());
                        sizeIndex++;
                    }
                }

                // add the camera preview sizes to the list preference with camera preview sizes
                _cameraPreviewSizesListPreference.SetEntries(_sizeEntries.ToArray());
                _cameraPreviewSizesListPreference.SetEntryValues(_sizeEntryValues.ToArray());

                // if camera preview size is not selected
                if (CameraController.СameraPreviewSize == null)
                    // set the "Auto" value as the default value
                    _cameraPreviewSizesListPreference.SetValueIndex(0);
            }
        }

        #endregion

        #endregion

    }
}
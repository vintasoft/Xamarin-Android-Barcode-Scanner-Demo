using Android.Content;
using Android.Hardware;
using Android.OS;
using Android.Preferences;

using System.Collections.Generic;
using System.Text;

namespace BarcodeScannerDemo
{
    /// <summary>
    /// The fragment that contains user interface for application settings.
    /// </summary>
    public class SettingsFragment : PreferenceFragment, ISharedPreferencesOnSharedPreferenceChangeListener
    {

        #region Fields

        /// <summary>
        /// The camera preview size list preference.
        /// </summary>
        ListPreference _cameraPreviewSizesListPreference;

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
        ListPreference _textEncodingListPreference;

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

        /// <summary>
        /// Called to do initial creation of a fragment.
        /// </summary>
        /// <param name="savedInstanceState">The saved instance state if the fragment is being re-created from a previous saved state.</param>
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            AddPreferencesFromResource(Resource.Xml.settings_page);

            CreateCameraPreviewSizePreference();
            CreateTextEncodingPreference();
        }

        /// <summary>
        /// Preference is clicked.
        /// </summary>
        /// <param name="preferenceScreen">The preference screen of <paramref name="preference"/>.</param>
        /// <param name="preference">The clicked preference.</param>
        public override bool OnPreferenceTreeClick(PreferenceScreen preferenceScreen, Preference preference)
        {
            if (preference is PreferenceScreen)
            {
                ((PreferenceScreen)preference).Dialog.Window.AddFlags(Android.Views.WindowManagerFlags.Fullscreen);
            }

            // if "Reset settings" button is clicked
            if (preference.Key == "set_defaults_button")
            {
                // get preferences
                ISharedPreferences preferences = PreferenceManager.GetDefaultSharedPreferences(Activity);
                // save the previous language value
                _previousLanguageValue = preferences.GetString("list_languages", "auto");
                // clear the preferences
                preferences.Edit().Clear().Commit();
                // refresh the UI
                RefreshUI();
            }

            return base.OnPreferenceTreeClick(preferenceScreen, preference);
        }

        /// <summary>
        /// Called when the Fragment is visible to the user and actively running.
        /// </summary>
        public override void OnResume()
        {
            base.OnResume();
            PreferenceManager.GetDefaultSharedPreferences(Activity).RegisterOnSharedPreferenceChangeListener(this);
        }

        /// <summary>
        /// Called when the Fragment is no longer resumed.
        /// </summary>
        public override void OnPause()
        {
            PreferenceManager.GetDefaultSharedPreferences(Activity).UnregisterOnSharedPreferenceChangeListener(this);
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
                // get new language value
                string newLanguageValue = sharedPreferences.GetString(key, "auto");
                // if new language differs from the previous language
                if (newLanguageValue != _previousLanguageValue)
                {
                    // notify user that application must be restarted
                    ((MainActivity)Activity).ShowInfoDialog(
                        Resources.GetString(Resource.String.app_name),
                        Resources.GetString(Resource.String.language_change_message));
                    _previousLanguageValue = newLanguageValue;
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
            PreferenceScreen.RemoveAll();

            AddPreferencesFromResource(Resource.Xml.settings_page);

            // get the preference for camera preview sizes list
            _cameraPreviewSizesListPreference = FindPreference("list_camera_preview_sizes") as ListPreference;
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
            _textEncodingListPreference = FindPreference("list_encodings") as ListPreference;

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
            _textEncodingListPreference = FindPreference("list_encodings") as ListPreference;
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
            _cameraPreviewSizesListPreference = FindPreference("list_camera_preview_sizes") as ListPreference;
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
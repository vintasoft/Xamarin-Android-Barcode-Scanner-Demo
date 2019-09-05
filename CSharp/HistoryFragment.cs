using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Text;
using Android.Views;
using Android.Widget;

using System;
using System.Collections.Generic;

using Vintasoft.XamarinBarcode;

namespace BarcodeScannerDemo
{
    /// <summary>
    /// The history fragment, that contains history user interface.
    /// </summary>
    public class HistoryFragment : ListFragment
    {

        #region NestedClasses

        /// <summary>
        /// The array adapter, which is associated with the ListView of the history fragment.
        /// </summary>
        internal class BarcodeArrayAdapter : ArrayAdapter<object>
        {

            #region Fields

            /// <summary>
            /// The resource layout identifier.
            /// </summary>
            int _resourceId;

            /// <summary>
            /// Determines whether the list with information about barcodes is empty.
            /// </summary>
            bool _isBarcodeInfoListEmpty;

            /// <summary>
            /// The title message for empty history.
            /// </summary>
            string _emptyTitle = "";

            /// <summary>
            /// The summary message for empty history.
            /// </summary>
            string _emptySummary = "";

            /// <summary>
            /// The text encoding name.
            /// </summary>
            string _textEncodingName = "-1";

            #endregion



            #region Constructors

            /// <summary>
            /// Initializes a new instance of <see cref="BarcodeArrayAdapter"/> class.
            /// </summary>
            /// <param name="context">A context.</param>
            /// <param name="values">A list with information about barcodes.</param>
            /// <param name="emptyTitle">A title message for empty history.</param>
            /// <param name="emptySummary">A summary message for empty history.</param>
            /// <param name="textEncodingName">A text encoding name.</param>
            internal BarcodeArrayAdapter(Context context, IList<object> values, string emptyTitle, string emptySummary, string textEncodingName)
                : base(context, Android.Resource.Layout.SimpleListItem2, values)
            {
                _emptyTitle = emptyTitle;
                _emptySummary = emptySummary;
                _textEncodingName = textEncodingName;

                // if barcode info list is empty
                if (values[0] is string)
                    _isBarcodeInfoListEmpty = true;

                _resourceId = Android.Resource.Layout.SimpleListItem2;
            }

            #endregion



            #region Methods

            /// <summary>
            /// Returns the view of single item in list view.
            /// </summary>
            /// <param name="position">An index.</param>
            /// <param name="convertView">A view to convert.</param>
            /// <param name="parent">A parent view group.</param>
            /// <returns>
            /// The item view.
            /// </returns>
            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                View view = convertView;
                if (view == null)
                    view = LayoutInflater.From(Context).Inflate(_resourceId, parent, false);
                
                // get the first text view
                TextView textView1 = view.FindViewById<TextView>(Android.Resource.Id.Text1);
                textView1.SetTextColor(Android.Graphics.Color.Orange);
                // set the single line property
                textView1.SetSingleLine(true);
                // set the max length of line
                textView1.SetMaxEms(20);
                // set ending of the line
                textView1.Ellipsize = TextUtils.TruncateAt.End;
                // get second text view
                TextView textView2 = view.FindViewById<TextView>(Android.Resource.Id.Text2);
               
                // if list with information about barcodes is NOT empty
                if (!_isBarcodeInfoListEmpty)
                {
                    IBarcodeInfo item = (IBarcodeInfo)GetItem(position);
                    textView1.Text = Utils.GetBarcodeTypeString(item);
                    try
                    { 
                        textView2.Text = Utils.GetEncodedBarcodeValue(item, _textEncodingName);
                    }
                    catch (NotSupportedException ex)
                    {
                        textView2.Text = Utils.GetEncodedBarcodeValue(item, "-1");
                    }
                }
                // if list with information about barcodes is empty
                else
                {
                    textView1.Text = _emptyTitle;
                    textView2.Text = _emptySummary;
                }
                return view;
            }
            
            #endregion

        }

        #endregion



        #region Fields

        /// <summary>
        /// The text encoding name.
        /// </summary>
        string _textEncodingName = "-1";

        #endregion



        #region Properties

        /// <summary>
        /// Gets the list with recognized barcodes.
        /// </summary>
        internal List<IBarcodeInfo> RecognizedBarcodes
        {
            get
            {
                if (Activity != null)
                    return ((MainActivity)Activity).RecognizedBarcodes;
                return null;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Called to do initial creation of a fragment.
        /// </summary>
        /// <param name="savedInstanceState">The saved instance state if the fragment is being re-created from a previous saved state.</param>
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
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
            Context contextThemeWrapper = new ContextThemeWrapper(Activity, Resource.Style.HistoryDialogTheme);
            LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);
            return base.OnCreateView(localInflater, container, savedInstanceState);
        }

        /// <summary>
        /// Called when the Fragment is visible to the user.
        /// </summary>
        public override void OnStart()
        {
            base.OnStart();

            // create list with information about recognized barcodes
            List<object> barcodeObjectList = new List<object>();
            // for each recognized barcode
            foreach (IBarcodeInfo info in RecognizedBarcodes)
                // add barcode info to the list
                barcodeObjectList.Add(info);

            // if the list is empty
            if (barcodeObjectList.Count == 0)
                barcodeObjectList.Add("Empty");

            _textEncodingName = "-1";
            try
            {
                // get preferences
                ISharedPreferences preferences = PreferenceManager.GetDefaultSharedPreferences(Activity);
                // get text encoding name from preferences
                _textEncodingName = preferences.GetString("list_encodings", "-1");
            }
            catch (Exception ex)
            {
                Toast.MakeText(Activity, string.Format("History error: {0}", ex.Message), ToastLength.Short).Show();
            }

            // create list adapter
            ListAdapter = new BarcodeArrayAdapter(
                this.Activity,
                barcodeObjectList, 
                Resources.GetString(Resource.String.title_history_empty_message),
                Resources.GetString(Resource.String.summary_history_empty_message),
                _textEncodingName);
        }

        /// <summary>
        /// This method will be called when an item in the list is selected.
        /// </summary>
        /// <param name="l">The ListView where the click happened.</param>
        /// <param name="v">The view that was clicked within the ListView.</param>
        /// <param name="position">The position of the view in the list.</param>
        /// <param name="id">The row id of the item that was clicked.</param>
        public override void OnListItemClick(ListView l, View v, int position, long id)
        {
            base.OnListItemClick(l, v, position, id);
            // if position (index) is correct
            if (position < RecognizedBarcodes.Count)
            {
                // get an extended information about barcode
                string info = ((MainActivity)Activity).GetExtendedBarcodeInfoString(RecognizedBarcodes[position], _textEncodingName);
                // get a barcode type
                string barcodeType = Utils.GetBarcodeTypeString(RecognizedBarcodes[position]);
                // show a dialog with an extended information about recognized barcode
                ((MainActivity)Activity).ShowInfoDialog(barcodeType, info, true, RecognizedBarcodes[position].Value);
            }
        }

        #endregion

    }
}
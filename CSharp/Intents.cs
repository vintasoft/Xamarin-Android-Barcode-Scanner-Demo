namespace BarcodeScannerDemo
{
    /// <summary>
    /// Contains auxiliary classes for working with intents.
    /// </summary>
    public static class Intents
    {

        /// <summary>
        /// Specifies available types of intent sources.
        /// </summary>
        public enum IntentSource
        {
            /// <summary>
            /// Intent source is native application.
            /// </summary>
            NAITIVE_APP_INTENT,

            /// <summary>
            /// No source.
            /// </summary>
            NONE
        }

        /// <summary>
        /// Contains constant strings for scan intent.
        /// </summary>
        public static class Scan
        {
            /// <summary>
            /// A scan action.
            /// </summary>
            public const string ACTION = "com.vintasoft.barcodescanner.SCAN";

            /// <summary>
            /// A scan result.
            /// </summary>
            public const string RESULT = "SCAN_RESULT";

            /// <summary>
            /// A scan result format.
            /// </summary>
            public const string RESULT_FORMAT = "SCAN_RESULT_FORMAT";
        }

    }
}
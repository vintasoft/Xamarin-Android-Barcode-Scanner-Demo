using Android.Text.Util;
using Android.Util;

using System.Collections.Generic;
using System.Text;

using Vintasoft.XamarinBarcode;
using Vintasoft.XamarinBarcode.BarcodeInfo;
using Vintasoft.XamarinBarcode.SymbologySubsets.AAMVA;

namespace BarcodeScannerDemo
{
    /// <summary>
    /// Contains auxiliary methods and classes.
    /// </summary>
    internal static class Utils
    {

        #region NestedClasses

        /// <summary>
        /// Linkify take a piece of text and a regular expression and turns all of the regex matches in the text into clickable links.
        /// </summary>
        internal class MyLinkify : Linkify, Linkify.IMatchFilter
        {

            /// <summary>
            /// Determines if substring  of <paramref name="s"/> should be turned into a link.
            /// </summary>
            /// <param name="s">A string.</param>
            /// <param name="start">A start index.</param>
            /// <param name="end">An end index.</param>
            /// <returns>
            /// <b>true</b> - if substring between start and end indexes should be turned into a link; otherwise - <b>false</b>.
            /// </returns>
            public bool AcceptMatch(Java.Lang.ICharSequence s, int start, int end)
            {
                if (Patterns.IpAddress.Matcher(s.SubSequenceFormatted(start, end)).Matches())
                    return false;
                return true;
            }

        }

        #endregion



        #region Fields

        /// <summary>
        /// The dictionary of available text encodings: encoding code page + encoding name => text encoding.
        /// </summary>
        internal static Dictionary<string, EncodingInfo> AvailableEncodings = new Dictionary<string, EncodingInfo>();

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a static fields of <see cref="Utils"/> class.
        /// </summary>
        static Utils()
        {
            EncodingInfo[] encodings = Encoding.GetEncodings();
            foreach (EncodingInfo encoding in encodings)
            {
                string key = encoding.CodePage + encoding.Name;
                if (!AvailableEncodings.ContainsKey(key))
                    AvailableEncodings.Add(key, encoding);
            }
        }

        #endregion



        #region Methods

        #region INTERNAL

        #region TextEncoding

        /// <summary>
        /// Returns an encoded barcode value of specified barcode info.
        /// </summary>
        /// <param name="info">A barcode.</param>
        /// <param name="textEncodingName">A name of choosen text encoding.</param>
        /// <returns>
        /// Encoded barcode value.
        /// </returns>
        internal static string GetEncodedBarcodeValue(IBarcodeInfo info, string textEncodingName)
        {
            if (info is AamvaBarcodeInfo)
            {
                AamvaBarcodeValue aamvaValue = ((AamvaBarcodeInfo)info).AamvaValue;
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(string.Format("Issuer identification number: {0}", aamvaValue.Header.IssuerIdentificationNumber));
                foreach (AamvaSubfile subfile in aamvaValue.Subfiles)
                {
                    foreach (AamvaDataElement dataElement in subfile.DataElements)
                    {
                        if (dataElement.Identifier.VersionLevel != AamvaVersionLevel.Undefined)
                            sb.AppendLine(string.Format("{0}={1} ({2})", dataElement.Identifier.ID, dataElement.Value, dataElement.Identifier.Description));
                        else
                            sb.AppendLine(string.Format("{0}={1}", dataElement.Identifier.ID, dataElement.Value));
                    }
                }
                return sb.ToString();
            }

            if (info is SwissQRCodeBarcodeInfo)
            {
                SwissQRCodeValueItem value = ((SwissQRCodeBarcodeInfo)info).DecodedValue;
                StringBuilder sb = new StringBuilder();

                sb.AppendLine(string.Format("Version: {0}", value.Version));
                sb.AppendLine(string.Format("CodingType: {0}", value.CodingType));

                if (!string.IsNullOrEmpty(value.IBAN))
                    sb.AppendLine(string.Format("IBAN: {0}", value.IBAN));

                if (!string.IsNullOrEmpty(value.CreditorAddressType))
                    sb.AppendLine(string.Format("Creditor address type: {0}", value.CreditorAddressType));
                if (!string.IsNullOrEmpty(value.CreditorName))
                    sb.AppendLine(string.Format("Creditor name: {0}", value.CreditorName));
                if (!string.IsNullOrEmpty(value.CreditorStreetOrAddressLine1))
                    sb.AppendLine(string.Format("Creditor street or address line 1: {0}", value.CreditorStreetOrAddressLine1));
                if (!string.IsNullOrEmpty(value.CreditorBuildingNumberOrAddressLine2))
                    sb.AppendLine(string.Format("Creditor building number or address line 2: {0}", value.CreditorBuildingNumberOrAddressLine2));
                if (!string.IsNullOrEmpty(value.CreditorTown))
                    sb.AppendLine(string.Format("Creditor town: {0}", value.CreditorTown));
                if (!string.IsNullOrEmpty(value.CreditorCountry))
                    sb.AppendLine(string.Format("Creditor country: {0}", value.CreditorCountry));


                if (!string.IsNullOrEmpty(value.Amount))
                    sb.AppendLine(string.Format("Amount: {0}", value.Amount));
                if (!string.IsNullOrEmpty(value.AmountCurrency))
                    sb.AppendLine(string.Format("Amount currency: {0}", value.AmountCurrency));

                if (!string.IsNullOrEmpty(value.UltimateDebtorAddressType))
                    sb.AppendLine(string.Format("Ultimate debtor address type: {0}", value.UltimateDebtorAddressType));
                if (!string.IsNullOrEmpty(value.UltimateDebtorName))
                    sb.AppendLine(string.Format("Ultimate debtor name: {0}", value.UltimateDebtorName));
                if (!string.IsNullOrEmpty(value.UltimateDebtorStreetOrAddressLine1))
                    sb.AppendLine(string.Format("Ultimate debtor street or address line 1: {0}", value.UltimateDebtorStreetOrAddressLine1));
                if (!string.IsNullOrEmpty(value.UltimateDebtorBuildingNumberOrAddressLine2))
                    sb.AppendLine(string.Format("Ultimate debtor building number or address line 2: {0}", value.UltimateDebtorBuildingNumberOrAddressLine2));
                if (!string.IsNullOrEmpty(value.UltimateDebtorTown))
                    sb.AppendLine(string.Format("Ultimate debtor town: {0}", value.UltimateDebtorTown));
                if (!string.IsNullOrEmpty(value.UltimateDebtorCountry))
                    sb.AppendLine(string.Format("Ultimate debtor country: {0}", value.UltimateDebtorCountry));

                if (!string.IsNullOrEmpty(value.PaymentReferenceType))
                    sb.AppendLine(string.Format("Payment reference type: {0}", value.PaymentReferenceType));
                if (!string.IsNullOrEmpty(value.PaymentReference))
                    sb.AppendLine(string.Format("Payment reference: {0}", value.PaymentReference));

                if (!string.IsNullOrEmpty(value.UnstructuredMessage))
                    sb.AppendLine(string.Format("Unstructured message: {0}", value.UnstructuredMessage));

                if (!string.IsNullOrEmpty(value.BillInformation))
                    sb.AppendLine(string.Format("Bill information: {0}", value.BillInformation));

                if (!string.IsNullOrEmpty(value.AlternativeSchemeParameters1))
                    sb.AppendLine(string.Format("Alternative scheme parameters 1: {0}", value.AlternativeSchemeParameters1));
                if (!string.IsNullOrEmpty(value.AlternativeSchemeParameters1))
                    sb.AppendLine(string.Format("Alternative scheme parameters 2: {0}", value.AlternativeSchemeParameters2));


                return sb.ToString();
            }

            if (textEncodingName == "-1")
                return info.Value;           

            if (info is BarcodeSubsetInfo ||
                info.BarcodeType == BarcodeType.Mailmark4StateC ||
                info.BarcodeType == BarcodeType.Mailmark4StateL)
                return info.Value;

            Encoding newEncoding = AvailableEncodings[textEncodingName].GetEncoding();
            return GetEncodedString(info.Value, newEncoding);
        }

        #endregion


        #region ToStringMethods

        /// <summary>
        /// Returns a string with barcode type.
        /// </summary>
        /// <param name="info">A barcode info.</param>
        /// <returns>
        /// A string with barcode type.
        /// </returns>
        internal static string GetBarcodeTypeString(IBarcodeInfo info)
        {
            if (info is BarcodeSubsetInfo)
                return ((BarcodeSubsetInfo)info).BarcodeSubset.ToString();
            return BarcodeTypeToString(info.BarcodeType);
        }

        /// <summary>
        /// Returns a string representation of <see cref="BarcodeType"/> enum value.
        /// </summary>
        /// <param name="item">An enum value.</param>
        /// <returns>A string representation of enum value.</returns>
        internal static string BarcodeTypeToString(BarcodeType item)
        {
            switch (item)
            {
                case BarcodeType.None:
                    return "None";

                case BarcodeType.Codabar:
                    return "Codabar";

                case BarcodeType.Code11:
                    return "Code11";

                case BarcodeType.Code128:
                    return "Code128";

                case BarcodeType.Standard2of5:
                    return "Standard2of5";

                case BarcodeType.Interleaved2of5:
                    return "Interleaved2of5";

                case BarcodeType.Code39:
                    return "Code39";

                case BarcodeType.Code93:
                    return "Code93";

                case BarcodeType.RSS14Stacked:
                    return "RSS14Stacked";

                case BarcodeType.RSSExpandedStacked:
                    return "RSSExpandedStacked";

                case BarcodeType.UPCE:
                    return "UPCE";

                case BarcodeType.Telepen:
                    return "Telepen";

                case BarcodeType.Postnet:
                    return "Postnet";

                case BarcodeType.Planet:
                    return "Planet";

                case BarcodeType.RoyalMail:
                    return "RoyalMail";

                case BarcodeType.AustralianPost:
                    return "AustralianPost";

                case BarcodeType.PatchCode:
                    return "PatchCode";

                case BarcodeType.PDF417:
                    return "PDF417";

                case BarcodeType.PDF417Compact:
                    return "PDF417Compact";

                case BarcodeType.EAN13:
                    return "EAN13";

                case BarcodeType.EAN8:
                    return "EAN8";

                case BarcodeType.UPCA:
                    return "UPCA";

                case BarcodeType.Plus5:
                    return "Plus5";

                case BarcodeType.UPCEPlus5:
                    return "UPCEPlus5";

                case BarcodeType.EAN13Plus5:
                    return "EAN13Plus5";

                case BarcodeType.EAN8Plus5:
                    return "EAN8Plus5";

                case BarcodeType.UPCAPlus5:
                    return "UPCAPlus5";

                case BarcodeType.Plus2:
                    return "Plus2";

                case BarcodeType.UPCEPlus2:
                    return "UPCEPlus2";

                case BarcodeType.EAN13Plus2:
                    return "EAN13Plus2";

                case BarcodeType.EAN8Plus2:
                    return "EAN8Plus2";

                case BarcodeType.UPCAPlus2:
                    return "UPCAPlus2";

                case BarcodeType.DataMatrix:
                    return "DataMatrix";

                case BarcodeType.DotCode:
                    return "DotCode";

                case BarcodeType.QR:
                    return "QR";

                case BarcodeType.IntelligentMail:
                    return "IntelligentMail";

                case BarcodeType.RSS14:
                    return "RSS14";

                case BarcodeType.RSSLimited:
                    return "RSSLimited";

                case BarcodeType.RSSExpanded:
                    return "RSSExpanded";

                case BarcodeType.Aztec:
                    return "Aztec";

                case BarcodeType.Pharmacode:
                    return "Pharmacode";

                case BarcodeType.MSI:
                    return "MSI";

                case BarcodeType.UnknownLinear:
                    return "UnknownLinear";

                case BarcodeType.MicroQR:
                    return "MicroQR";

                case BarcodeType.MaxiCode:
                    return "MaxiCode";

                case BarcodeType.DutchKIX:
                    return "DutchKIX";

                case BarcodeType.MicroPDF417:
                    return "MicroPDF417";

                case BarcodeType.Mailmark4StateC:
                    return "Mailmark4StateC";

                case BarcodeType.Mailmark4StateL:
                    return "Mailmark4StateL";

                case BarcodeType.IATA2of5:
                    return "IATA2of5";

                case BarcodeType.Matrix2of5:
                    return "Matrix2of5";

                case BarcodeType.Code16K:
                    return "Code16K";

                case BarcodeType.HanXinCode:
                    return "HanXinCode";

            }
            return null;
        }

        /// <summary>
        /// Returns a string representation of <see cref="AztecSymbolType"/> enum value.
        /// </summary>
        /// <param name="item">An enum value.</param>
        /// <returns>A string representation of enum value.</returns>
        internal static string AztecSymbolTypeToString(AztecSymbolType item)
        {
            switch (item)
            {
                case AztecSymbolType.Undefined:
                    return "Undefined";

                case AztecSymbolType.Rune:
                    return "Rune";

                case AztecSymbolType.Compact:
                    return "Compact";

                case AztecSymbolType.FullRange:
                    return "FullRange";

            }
            return null;
        }

        /// <summary>
        /// Returns a string representation of <see cref="DataMatrixSymbolType"/> enum value.
        /// </summary>
        /// <param name="item">An enum value.</param>
        /// <returns>A string representation of enum value.</returns>
        internal static string DataMatrixSymbolTypeToString(DataMatrixSymbolType item)
        {
            switch (item)
            {
                case DataMatrixSymbolType.Undefined:
                    return "Undefined";

                case DataMatrixSymbolType.Row10Col10:
                    return "Row10Col10";

                case DataMatrixSymbolType.Row12Col12:
                    return "Row12Col12";

                case DataMatrixSymbolType.Row14Col14:
                    return "Row14Col14";

                case DataMatrixSymbolType.Row16Col16:
                    return "Row16Col16";

                case DataMatrixSymbolType.Row18Col18:
                    return "Row18Col18";

                case DataMatrixSymbolType.Row20Col20:
                    return "Row20Col20";

                case DataMatrixSymbolType.Row22Col22:
                    return "Row22Col22";

                case DataMatrixSymbolType.Row24Col24:
                    return "Row24Col24";

                case DataMatrixSymbolType.Row26Col26:
                    return "Row26Col26";

                case DataMatrixSymbolType.Row32Col32:
                    return "Row32Col32";

                case DataMatrixSymbolType.Row36Col36:
                    return "Row36Col36";

                case DataMatrixSymbolType.Row40Col40:
                    return "Row40Col40";

                case DataMatrixSymbolType.Row44Col44:
                    return "Row44Col44";

                case DataMatrixSymbolType.Row48Col48:
                    return "Row48Col48";

                case DataMatrixSymbolType.Row52Col52:
                    return "Row52Col52";

                case DataMatrixSymbolType.Row64Col64:
                    return "Row64Col64";

                case DataMatrixSymbolType.Row72Col72:
                    return "Row72Col72";

                case DataMatrixSymbolType.Row80Col80:
                    return "Row80Col80";

                case DataMatrixSymbolType.Row88Col88:
                    return "Row88Col88";

                case DataMatrixSymbolType.Row96Col96:
                    return "Row96Col96";

                case DataMatrixSymbolType.Row104Col104:
                    return "Row104Col104";

                case DataMatrixSymbolType.Row120Col120:
                    return "Row120Col120";

                case DataMatrixSymbolType.Row132Col132:
                    return "Row132Col132";

                case DataMatrixSymbolType.Row144Col144:
                    return "Row144Col144";

                case DataMatrixSymbolType.Row8Col18:
                    return "Row8Col18";

                case DataMatrixSymbolType.Row8Col32:
                    return "Row8Col32";

                case DataMatrixSymbolType.Row12Col26:
                    return "Row12Col26";

                case DataMatrixSymbolType.Row12Col36:
                    return "Row12Col36";

                case DataMatrixSymbolType.Row16Col36:
                    return "Row16Col36";

                case DataMatrixSymbolType.Row16Col48:
                    return "Row16Col48";

            }
            return null;
        }

        /// <summary>
        /// Returns a string representation of <see cref="HanXinCodeErrorCorrectionLevel"/> enum value.
        /// </summary>
        /// <param name="item">An enum value.</param>
        /// <returns>A string representation of enum value.</returns>
        internal static string HanXinCodeErrorCorrectionLevelToString(HanXinCodeErrorCorrectionLevel item)
        {
            switch (item)
            {
                case HanXinCodeErrorCorrectionLevel.L1:
                    return "L1";

                case HanXinCodeErrorCorrectionLevel.L2:
                    return "L2";

                case HanXinCodeErrorCorrectionLevel.L3:
                    return "L3";

                case HanXinCodeErrorCorrectionLevel.L4:
                    return "L4";

            }
            return null;
        }

        /// <summary>
        /// Returns a string representation of <see cref="HanXinCodeSymbolVersion"/> enum value.
        /// </summary>
        /// <param name="item">An enum value.</param>
        /// <returns>A string representation of enum value.</returns>
        internal static string HanXinCodeSymbolVersionToString(HanXinCodeSymbolVersion item)
        {
            switch (item)
            {
                case HanXinCodeSymbolVersion.Version1:
                    return "Version1";

                case HanXinCodeSymbolVersion.Version2:
                    return "Version2";

                case HanXinCodeSymbolVersion.Version3:
                    return "Version3";

                case HanXinCodeSymbolVersion.Version4:
                    return "Version4";

                case HanXinCodeSymbolVersion.Version5:
                    return "Version5";

                case HanXinCodeSymbolVersion.Version6:
                    return "Version6";

                case HanXinCodeSymbolVersion.Version7:
                    return "Version7";

                case HanXinCodeSymbolVersion.Version8:
                    return "Version8";

                case HanXinCodeSymbolVersion.Version9:
                    return "Version9";

                case HanXinCodeSymbolVersion.Version10:
                    return "Version10";

                case HanXinCodeSymbolVersion.Version11:
                    return "Version11";

                case HanXinCodeSymbolVersion.Version12:
                    return "Version12";

                case HanXinCodeSymbolVersion.Version13:
                    return "Version13";

                case HanXinCodeSymbolVersion.Version14:
                    return "Version14";

                case HanXinCodeSymbolVersion.Version15:
                    return "Version15";

                case HanXinCodeSymbolVersion.Version16:
                    return "Version16";

                case HanXinCodeSymbolVersion.Version17:
                    return "Version17";

                case HanXinCodeSymbolVersion.Version18:
                    return "Version18";

                case HanXinCodeSymbolVersion.Version19:
                    return "Version19";

                case HanXinCodeSymbolVersion.Version20:
                    return "Version20";

                case HanXinCodeSymbolVersion.Version21:
                    return "Version21";

                case HanXinCodeSymbolVersion.Version22:
                    return "Version22";

                case HanXinCodeSymbolVersion.Version23:
                    return "Version23";

                case HanXinCodeSymbolVersion.Version24:
                    return "Version24";

                case HanXinCodeSymbolVersion.Version25:
                    return "Version25";

                case HanXinCodeSymbolVersion.Version26:
                    return "Version26";

                case HanXinCodeSymbolVersion.Version27:
                    return "Version27";

                case HanXinCodeSymbolVersion.Version28:
                    return "Version28";

                case HanXinCodeSymbolVersion.Version29:
                    return "Version29";

                case HanXinCodeSymbolVersion.Version30:
                    return "Version30";

                case HanXinCodeSymbolVersion.Version31:
                    return "Version31";

                case HanXinCodeSymbolVersion.Version32:
                    return "Version32";

                case HanXinCodeSymbolVersion.Version33:
                    return "Version33";

                case HanXinCodeSymbolVersion.Version34:
                    return "Version34";

                case HanXinCodeSymbolVersion.Version35:
                    return "Version35";

                case HanXinCodeSymbolVersion.Version36:
                    return "Version36";

                case HanXinCodeSymbolVersion.Version37:
                    return "Version37";

                case HanXinCodeSymbolVersion.Version38:
                    return "Version38";

                case HanXinCodeSymbolVersion.Version39:
                    return "Version39";

                case HanXinCodeSymbolVersion.Version40:
                    return "Version40";

                case HanXinCodeSymbolVersion.Version41:
                    return "Version41";

                case HanXinCodeSymbolVersion.Version42:
                    return "Version42";

                case HanXinCodeSymbolVersion.Version43:
                    return "Version43";

                case HanXinCodeSymbolVersion.Version44:
                    return "Version44";

                case HanXinCodeSymbolVersion.Version45:
                    return "Version45";

                case HanXinCodeSymbolVersion.Version46:
                    return "Version46";

                case HanXinCodeSymbolVersion.Version47:
                    return "Version47";

                case HanXinCodeSymbolVersion.Version48:
                    return "Version48";

                case HanXinCodeSymbolVersion.Version49:
                    return "Version49";

                case HanXinCodeSymbolVersion.Version50:
                    return "Version50";

                case HanXinCodeSymbolVersion.Version51:
                    return "Version51";

                case HanXinCodeSymbolVersion.Version52:
                    return "Version52";

                case HanXinCodeSymbolVersion.Version53:
                    return "Version53";

                case HanXinCodeSymbolVersion.Version54:
                    return "Version54";

                case HanXinCodeSymbolVersion.Version55:
                    return "Version55";

                case HanXinCodeSymbolVersion.Version56:
                    return "Version56";

                case HanXinCodeSymbolVersion.Version57:
                    return "Version57";

                case HanXinCodeSymbolVersion.Version58:
                    return "Version58";

                case HanXinCodeSymbolVersion.Version59:
                    return "Version59";

                case HanXinCodeSymbolVersion.Version60:
                    return "Version60";

                case HanXinCodeSymbolVersion.Version61:
                    return "Version61";

                case HanXinCodeSymbolVersion.Version62:
                    return "Version62";

                case HanXinCodeSymbolVersion.Version63:
                    return "Version63";

                case HanXinCodeSymbolVersion.Version64:
                    return "Version64";

                case HanXinCodeSymbolVersion.Version65:
                    return "Version65";

                case HanXinCodeSymbolVersion.Version66:
                    return "Version66";

                case HanXinCodeSymbolVersion.Version67:
                    return "Version67";

                case HanXinCodeSymbolVersion.Version68:
                    return "Version68";

                case HanXinCodeSymbolVersion.Version69:
                    return "Version69";

                case HanXinCodeSymbolVersion.Version70:
                    return "Version70";

                case HanXinCodeSymbolVersion.Version71:
                    return "Version71";

                case HanXinCodeSymbolVersion.Version72:
                    return "Version72";

                case HanXinCodeSymbolVersion.Version73:
                    return "Version73";

                case HanXinCodeSymbolVersion.Version74:
                    return "Version74";

                case HanXinCodeSymbolVersion.Version75:
                    return "Version75";

                case HanXinCodeSymbolVersion.Version76:
                    return "Version76";

                case HanXinCodeSymbolVersion.Version77:
                    return "Version77";

                case HanXinCodeSymbolVersion.Version78:
                    return "Version78";

                case HanXinCodeSymbolVersion.Version79:
                    return "Version79";

                case HanXinCodeSymbolVersion.Version80:
                    return "Version80";

                case HanXinCodeSymbolVersion.Version81:
                    return "Version81";

                case HanXinCodeSymbolVersion.Version82:
                    return "Version82";

                case HanXinCodeSymbolVersion.Version83:
                    return "Version83";

                case HanXinCodeSymbolVersion.Version84:
                    return "Version84";

                case HanXinCodeSymbolVersion.Undefined:
                    return "Undefined";

            }
            return null;
        }

        /// <summary>
        /// Returns a string representation of <see cref="MaxiCodeEncodingMode"/> enum value.
        /// </summary>
        /// <param name="item">An enum value.</param>
        /// <returns>A string representation of enum value.</returns>
        internal static string MaxiCodeEncodingModeToString(MaxiCodeEncodingMode item)
        {
            switch (item)
            {
                case MaxiCodeEncodingMode.Mode2:
                    return "Mode2";

                case MaxiCodeEncodingMode.Mode3:
                    return "Mode3";

                case MaxiCodeEncodingMode.Mode4:
                    return "Mode4";

                case MaxiCodeEncodingMode.Mode5:
                    return "Mode5";

                case MaxiCodeEncodingMode.Mode6:
                    return "Mode6";

            }
            return null;
        }

        /// <summary>
        /// Returns a string representation of <see cref="MicroPDF417SymbolType"/> enum value.
        /// </summary>
        /// <param name="item">An enum value.</param>
        /// <returns>A string representation of enum value.</returns>
        internal static string MicroPDF417SymbolTypeToString(MicroPDF417SymbolType item)
        {
            switch (item)
            {
                case MicroPDF417SymbolType.Undefined:
                    return "Undefined";

                case MicroPDF417SymbolType.Col1Row11:
                    return "Col1Row11";

                case MicroPDF417SymbolType.Col1Row14:
                    return "Col1Row14";

                case MicroPDF417SymbolType.Col1Row17:
                    return "Col1Row17";

                case MicroPDF417SymbolType.Col1Row20:
                    return "Col1Row20";

                case MicroPDF417SymbolType.Col1Row24:
                    return "Col1Row24";

                case MicroPDF417SymbolType.Col1Row28:
                    return "Col1Row28";

                case MicroPDF417SymbolType.Col2Row8:
                    return "Col2Row8";

                case MicroPDF417SymbolType.Col2Row11:
                    return "Col2Row11";

                case MicroPDF417SymbolType.Col2Row14:
                    return "Col2Row14";

                case MicroPDF417SymbolType.Col2Row17:
                    return "Col2Row17";

                case MicroPDF417SymbolType.Col2Row20:
                    return "Col2Row20";

                case MicroPDF417SymbolType.Col2Row23:
                    return "Col2Row23";

                case MicroPDF417SymbolType.Col2Row26:
                    return "Col2Row26";

                case MicroPDF417SymbolType.Col3Row6:
                    return "Col3Row6";

                case MicroPDF417SymbolType.Col3Row8:
                    return "Col3Row8";

                case MicroPDF417SymbolType.Col3Row10:
                    return "Col3Row10";

                case MicroPDF417SymbolType.Col3Row12:
                    return "Col3Row12";

                case MicroPDF417SymbolType.Col3Row15:
                    return "Col3Row15";

                case MicroPDF417SymbolType.Col3Row20:
                    return "Col3Row20";

                case MicroPDF417SymbolType.Col3Row26:
                    return "Col3Row26";

                case MicroPDF417SymbolType.Col3Row32:
                    return "Col3Row32";

                case MicroPDF417SymbolType.Col3Row38:
                    return "Col3Row38";

                case MicroPDF417SymbolType.Col3Row44:
                    return "Col3Row44";

                case MicroPDF417SymbolType.Col4Row4:
                    return "Col4Row4";

                case MicroPDF417SymbolType.Col4Row6:
                    return "Col4Row6";

                case MicroPDF417SymbolType.Col4Row8:
                    return "Col4Row8";

                case MicroPDF417SymbolType.Col4Row10:
                    return "Col4Row10";

                case MicroPDF417SymbolType.Col4Row12:
                    return "Col4Row12";

                case MicroPDF417SymbolType.Col4Row15:
                    return "Col4Row15";

                case MicroPDF417SymbolType.Col4Row20:
                    return "Col4Row20";

                case MicroPDF417SymbolType.Col4Row26:
                    return "Col4Row26";

                case MicroPDF417SymbolType.Col4Row32:
                    return "Col4Row32";

                case MicroPDF417SymbolType.Col4Row38:
                    return "Col4Row38";

                case MicroPDF417SymbolType.Col4Row44:
                    return "Col4Row44";

            }
            return null;
        }

        /// <summary>
        /// Returns a string representation of <see cref="PDF417ErrorCorrectionLevel"/> enum value.
        /// </summary>
        /// <param name="item">An enum value.</param>
        /// <returns>A string representation of enum value.</returns>
        internal static string PDF417ErrorCorrectionLevelToString(PDF417ErrorCorrectionLevel item)
        {
            switch (item)
            {
                case PDF417ErrorCorrectionLevel.Level0:
                    return "Level0";

                case PDF417ErrorCorrectionLevel.Level1:
                    return "Level1";

                case PDF417ErrorCorrectionLevel.Level2:
                    return "Level2";

                case PDF417ErrorCorrectionLevel.Level3:
                    return "Level3";

                case PDF417ErrorCorrectionLevel.Level4:
                    return "Level4";

                case PDF417ErrorCorrectionLevel.Level5:
                    return "Level5";

                case PDF417ErrorCorrectionLevel.Level6:
                    return "Level6";

                case PDF417ErrorCorrectionLevel.Level7:
                    return "Level7";

                case PDF417ErrorCorrectionLevel.Level8:
                    return "Level8";

                case PDF417ErrorCorrectionLevel.Undefined:
                    return "Undefined";

            }
            return null;
        }

        /// <summary>
        /// Returns a string representation of <see cref="QRErrorCorrectionLevel"/> enum value.
        /// </summary>
        /// <param name="item">An enum value.</param>
        /// <returns>A string representation of enum value.</returns>
        internal static string QRErrorCorrectionLevelToString(QRErrorCorrectionLevel item)
        {
            switch (item)
            {
                case QRErrorCorrectionLevel.L:
                    return "L";

                case QRErrorCorrectionLevel.M:
                    return "M";

                case QRErrorCorrectionLevel.Q:
                    return "Q";

                case QRErrorCorrectionLevel.H:
                    return "H";

            }
            return null;
        }

        /// <summary>
        /// Returns a string representation of <see cref="QRSymbolVersion"/> enum value.
        /// </summary>
        /// <param name="item">An enum value.</param>
        /// <returns>A string representation of enum value.</returns>
        internal static string QRSymbolVersionToString(QRSymbolVersion item)
        {
            switch (item)
            {
                case QRSymbolVersion.Version1:
                    return "Version1";

                case QRSymbolVersion.Version2:
                    return "Version2";

                case QRSymbolVersion.Version3:
                    return "Version3";

                case QRSymbolVersion.Version4:
                    return "Version4";

                case QRSymbolVersion.Version5:
                    return "Version5";

                case QRSymbolVersion.Version6:
                    return "Version6";

                case QRSymbolVersion.Version7:
                    return "Version7";

                case QRSymbolVersion.Version8:
                    return "Version8";

                case QRSymbolVersion.Version9:
                    return "Version9";

                case QRSymbolVersion.Version10:
                    return "Version10";

                case QRSymbolVersion.Version11:
                    return "Version11";

                case QRSymbolVersion.Version12:
                    return "Version12";

                case QRSymbolVersion.Version13:
                    return "Version13";

                case QRSymbolVersion.Version14:
                    return "Version14";

                case QRSymbolVersion.Version15:
                    return "Version15";

                case QRSymbolVersion.Version16:
                    return "Version16";

                case QRSymbolVersion.Version17:
                    return "Version17";

                case QRSymbolVersion.Version18:
                    return "Version18";

                case QRSymbolVersion.Version19:
                    return "Version19";

                case QRSymbolVersion.Version20:
                    return "Version20";

                case QRSymbolVersion.Version21:
                    return "Version21";

                case QRSymbolVersion.Version22:
                    return "Version22";

                case QRSymbolVersion.Version23:
                    return "Version23";

                case QRSymbolVersion.Version24:
                    return "Version24";

                case QRSymbolVersion.Version25:
                    return "Version25";

                case QRSymbolVersion.Version26:
                    return "Version26";

                case QRSymbolVersion.Version27:
                    return "Version27";

                case QRSymbolVersion.Version28:
                    return "Version28";

                case QRSymbolVersion.Version29:
                    return "Version29";

                case QRSymbolVersion.Version30:
                    return "Version30";

                case QRSymbolVersion.Version31:
                    return "Version31";

                case QRSymbolVersion.Version32:
                    return "Version32";

                case QRSymbolVersion.Version33:
                    return "Version33";

                case QRSymbolVersion.Version34:
                    return "Version34";

                case QRSymbolVersion.Version35:
                    return "Version35";

                case QRSymbolVersion.Version36:
                    return "Version36";

                case QRSymbolVersion.Version37:
                    return "Version37";

                case QRSymbolVersion.Version38:
                    return "Version38";

                case QRSymbolVersion.Version39:
                    return "Version39";

                case QRSymbolVersion.Version40:
                    return "Version40";

                case QRSymbolVersion.VersionM1:
                    return "VersionM1";

                case QRSymbolVersion.VersionM2:
                    return "VersionM2";

                case QRSymbolVersion.VersionM3:
                    return "VersionM3";

                case QRSymbolVersion.VersionM4:
                    return "VersionM4";

                case QRSymbolVersion.Model1Version1:
                    return "Model1Version1";

                case QRSymbolVersion.Model1Version2:
                    return "Model1Version2";

                case QRSymbolVersion.Model1Version3:
                    return "Model1Version3";

                case QRSymbolVersion.Model1Version4:
                    return "Model1Version4";

                case QRSymbolVersion.Model1Version5:
                    return "Model1Version5";

                case QRSymbolVersion.Model1Version6:
                    return "Model1Version6";

                case QRSymbolVersion.Model1Version7:
                    return "Model1Version7";

                case QRSymbolVersion.Model1Version8:
                    return "Model1Version8";

                case QRSymbolVersion.Model1Version9:
                    return "Model1Version9";

                case QRSymbolVersion.Model1Version10:
                    return "Model1Version10";

                case QRSymbolVersion.Model1Version11:
                    return "Model1Version11";

                case QRSymbolVersion.Model1Version12:
                    return "Model1Version12";

                case QRSymbolVersion.Model1Version13:
                    return "Model1Version13";

                case QRSymbolVersion.Model1Version14:
                    return "Model1Version14";

                case QRSymbolVersion.Undefined:
                    return "Undefined";

            }
            return null;
        }

        #endregion

        #endregion


        #region PRIVATE

        /// <summary>
        /// Returns string encoded usig specified text encoding.
        /// </summary>
        /// <param name="value">A value to encode.</param>
        /// <param name="textEncoding">Text encoding.</param>
        /// <returns>
        /// An encoded string.
        /// </returns>
        private static string GetEncodedString(string value, Encoding textEncoding)
        {
            bool singleByteEncoding = true;
            for (int i = 0; i < value.Length; i++)
                if ((int)value[i] > 255)
                {
                    singleByteEncoding = false;
                    break;
                }

            byte[] bytes;
            if (singleByteEncoding)
            {
                bytes = new byte[value.Length];
                for (int i = 0; i < value.Length; i++)
                    bytes[i] = (byte)value[i];
            }
            else
            {
                bytes = Encoding.Default.GetBytes(value);
            }

            return textEncoding.GetString(bytes);
        }

        #endregion

        #endregion

    }
}
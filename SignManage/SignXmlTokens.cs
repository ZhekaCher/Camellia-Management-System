﻿using System;

namespace Camellia_Management_System.SignManage
{

    /// @author Yevgeniy Cherdantsev
    /// @date 18.02.2020 10:35:27
    /// @version 1.0
    /// <summary>
    /// Class that signs xml tokens
    /// </summary>
    public class SignXmlTokens
    {
        /*!

@throw exception - unexpected exception
     
     */


        /// @author Yevgeniy Cherdantsev
        /// @date 18.02.2020 10:35:42
        /// @version 1.0
        /// <summary>
        /// Signing of the xml token
        /// </summary>
        /// <param name="inData">XML text</param>
        /// <param name="sign">Sign</param>
        /// <returns>String - signed token</returns>
        /// <exception cref="Exception">Unexpected exception</exception>


    

        public static string SignToken(string inData, Sign sign)
        {
            /*
             * Error status for electronic digital signature
             * 0 - ok
             * any numers greater then 0 is error
             */
            uint EDSError;

            /* 
             * Creating required parameters for using method
             * FilePath - Electronic digital signature .p12 key
             * Password - Password for key
             */


            // Here calling COM and initialazing it
            var kalkanComTest = new KalkanCryptCOMLib.KalkanCryptCOM();
            kalkanComTest.Init();

            // Loading hey with handling exception
            kalkanComTest.LoadKeyStore((int) KalkanCryptCOMLib.KALKANCRYPTCOM_STORETYPE.KCST_PKCS12, sign.Password,
                sign.FilePath,
                "");
            EDSError = kalkanComTest.GetLastError();

            if (EDSError > 0)
            {
                throw new Exception("Some error occured while loading the key storage");
            }

            var Alias = "";
            var SignNodeID = "";
            var ParentSignNode = "";
            var ParentNameSpace = "";
            // string InData = "";
            string OutSign = "", Error = "";

            // Signing XML
            kalkanComTest.SignXML(Alias, 0, SignNodeID, ParentSignNode, ParentNameSpace,
                // $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><login><timeTicket>123456789</timeTicket></login>",
                $"{inData}",
                out OutSign);
            // KalkanCOMTest.SignXML(Alias, 0, SignNodeID, ParentSignNode, ParentNameSpace, InData, out OutSign);
            kalkanComTest.GetLastErrorString(out Error, out EDSError);

            if (EDSError > 0)
            {
                throw new Exception("Some error occured while signing the token");
            }

            var outData = OutSign.Replace("\n", "\r\n");
            return outData;
        }
    }
}
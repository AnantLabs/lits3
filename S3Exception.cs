﻿using System;
using System.Net;
using System.Xml;

namespace LitS3
{
    public enum S3ErrorCode
    {
        Unknown,
        AccessDenied,
        NoSuchBucket,
        NoSuchKey,
        BucketNotEmpty
    }

    public class S3Exception : Exception
    {
        public S3ErrorCode ErrorCode { get; private set; }
        public string BucketName { get; private set; }

        public S3Exception(S3ErrorCode errorCode, string bucketName, string message, WebException innerException)
            : base(message, innerException)
        {
            this.ErrorCode = errorCode;
            this.BucketName = bucketName;
        }

        public static S3Exception FromWebException(WebException exception)
        {
            HttpWebResponse response = (HttpWebResponse)exception.Response;

            var reader = new XmlTextReader(response.GetResponseStream())
            {
                WhitespaceHandling = WhitespaceHandling.Significant,
                Namespaces = false
            };

            reader.MoveToContent();
            reader.ReadStartElement("Error");

            S3ErrorCode errorCode = S3ErrorCode.Unknown;
            string message = null;
            string bucketName = null;

            while (reader.Name != "Error")
            {
                switch (reader.Name)
                {
                    case "Code":
                        errorCode = ParseCode(reader.ReadElementContentAsString());
                        break;
                    case "Message":
                        message = reader.ReadElementContentAsString();
                        break;
                    case "BucketName":
                        bucketName = reader.ReadElementContentAsString();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            return new S3Exception(errorCode, bucketName, message, exception);
        }

        static S3ErrorCode ParseCode(string code)
        {
            if (Enum.IsDefined(typeof(S3ErrorCode), code))
                return (S3ErrorCode)Enum.Parse(typeof(S3ErrorCode), code);
            else
                return S3ErrorCode.Unknown;
        }
    }
}
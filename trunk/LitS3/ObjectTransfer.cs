using System;
using System.ComponentModel;

namespace LitS3
{
    /// <summary>
    /// Provides progress data for S3 object transfer operations.
    /// </summary>

    [Serializable]
    public class ObjectTransferProgressChangedEventArgs : ProgressChangedEventArgs
    {
        private readonly string bucketName;
        private readonly string key;

        public ObjectTransferProgressChangedEventArgs(
            string bucketName, string key,
            long bytesTransferred, long totalBytesToTransfer) :
                base((int) Math.Round(bytesTransferred * 100.0 / totalBytesToTransfer), null)
        {
            this.bucketName = bucketName;
            this.key = key;
            BytesTransferred = bytesTransferred;
            TotalBytesToTransfer = totalBytesToTransfer;
        }

        /// <summary>
        /// Gets the bucket of the object being transferred.
        /// </summary>
        public string BucketName { get { return bucketName ?? string.Empty; } }

        /// <summary>
        /// Gets the key of the object being transferred.
        /// </summary>
        public string Key { get { return key ?? string.Empty; } }

        /// <summary>
        /// Gets the number of bytes transferred. 
        /// </summary>
        public long BytesTransferred { get; private set; }

        /// <summary>
        /// Gets the total number of bytes in the transfer operation.
        /// </summary>
        public long TotalBytesToTransfer { get; private set; }
    }
}

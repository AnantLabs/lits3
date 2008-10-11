using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LitS3
{
    public enum BucketNameChecking
    {
        /// <summary>
        /// None specifies that no checking should be performed on a given bucket name.
        /// </summary>
        None,
        /// <summary>
        /// Loose specifies that checking should only be performed against Amazon S3 requirements.
        /// </summary>
        Loose,
        /// <summary>
        /// Strict (preferred) specifies that checking should be performed against both Amazon S3
        /// requirements and DNS requirements.
        /// </summary>
        Strict
    }

    // See http://docs.amazonwebservices.com/AmazonS3/2006-03-01/BucketRestrictions.html
    // for a rundown of this logic.

    /*public static class BucketNameChecker
    {
        public static bool IsLooseAmazonS3BucketName(this string s)
        {
            return true;
        }

        public static bool IsStrictAmazonS3BucketName(this string s)
        {
            // perform these checks first
            if (!IsLooseAmazonS3BucketName(s))
                return false;

            return true;
        }
    }*/
}

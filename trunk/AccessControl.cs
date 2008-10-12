
namespace LitS3
{
    public enum CannedAcl
    {
        /// <summary>
        /// Owner gets FULL_CONTROL. No one else has access rights (default).
        /// </summary>
        Private,
        /// <summary>
        /// Owner gets FULL_CONTROL and the anonymous principal is granted READ access. 
        /// If this policy is used on an object, it can be read from a browser with no authentication.
        /// </summary>
        PublicRead,
        /// <summary>
        /// Owner gets FULL_CONTROL, the anonymous principal is granted READ and WRITE access. This is 
        /// a useful policy to apply to a bucket, if you intend for any anonymous user to PUT 
        /// objects into the bucket.
        /// </summary>
        PublicReadWrite,
        /// <summary>
        /// Owner gets FULL_CONTROL, and any principal authenticated as a registered 
        /// Amazon S3 user is granted READ access.
        /// </summary>
        AuthenticatedRead
    }

    // TODO: support these "complex" access control policies

    /*public class AccessControlPolicy
    {
        /// <summary>
        /// Creates 
        /// </summary>
        public AccessControlPolicy()
        {

        }

        /// <summary>
        /// Creates an AccessControlPolicy matching the given "Canned ACL".
        /// </summary>
        public AccessControlPolicy(CannedAcl acl)
        {

        }
    }*/
}

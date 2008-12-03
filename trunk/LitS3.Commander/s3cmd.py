# LitS3.Commander
# Command-line interface to LitS3
#
# The MIT License
# 
# Copyright (c) 2008, Nick Farina
#
# Author(s):
#
#   Atif Aziz, http://www.raboo.com/
# 
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:
# 
# The above copyright notice and this permission notice shall be included in
# all copies or substantial portions of the Software.
# 
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
# THE SOFTWARE.

# $Id$

import sys, clr

from System import Int64, Byte, Array
from System.IO import Path, FileInfo, Directory, MemoryStream
from System.Text import Encoding
from System.Environment import GetEnvironmentVariable
from System.Net import ServicePointManager, ICertificatePolicy

# Add debug and release build paths
sys.path.extend([Path.Combine(Path.GetDirectoryName(sys.argv[0]), '..\\bin\\' + bld) 
                    for bld in ('Release', 'Debug')])

clr.AddReferenceToFileAndPath('LitS3.dll')
from LitS3 import *

MIME_MAP = {
    '.323'    : 'text/h323',
    '.asx'    : 'video/x-ms-asf',
    '.acx'    : 'application/internet-property-stream',
    '.ai'     : 'application/postscript',
    '.aif'    : 'audio/x-aiff',
    '.aiff'   : 'audio/aiff',
    '.axs'    : 'application/olescript',
    '.aifc'   : 'audio/aiff',
    '.asr'    : 'video/x-ms-asf',
    '.avi'    : 'video/x-msvideo',
    '.asf'    : 'video/x-ms-asf',
    '.au'     : 'audio/basic',
    '.bin'    : 'application/octet-stream',
    '.bas'    : 'text/plain',
    '.bcpio'  : 'application/x-bcpio',
    '.bmp'    : 'image/bmp',
    '.cdf'    : 'application/x-cdf',
    '.cat'    : 'application/vndms-pkiseccat',
    '.crt'    : 'application/x-x509-ca-cert',
    '.c'      : 'text/plain',
    '.css'    : 'text/css',
    '.cer'    : 'application/x-x509-ca-cert',
    '.crl'    : 'application/pkix-crl',
    '.cmx'    : 'image/x-cmx',
    '.csh'    : 'application/x-csh',
    '.cod'    : 'image/cis-cod',
    '.cpio'   : 'application/x-cpio',
    '.clp'    : 'application/x-msclip',
    '.crd'    : 'application/x-mscardfile',
    '.dll'    : 'application/x-msdownload',
    '.dot'    : 'application/msword',
    '.doc'    : 'application/msword',
    '.dvi'    : 'application/x-dvi',
    '.dir'    : 'application/x-director',
    '.dxr'    : 'application/x-director',
    '.der'    : 'application/x-x509-ca-cert',
    '.dib'    : 'image/bmp',
    '.dcr'    : 'application/x-director',
    '.disco'  : 'text/xml',
    '.exe'    : 'application/octet-stream',
    '.etx'    : 'text/x-setext',
    '.evy'    : 'application/envoy',
    '.eml'    : 'message/rfc822',
    '.eps'    : 'application/postscript',
    '.flr'    : 'x-world/x-vrml',
    '.fif'    : 'application/fractals',
    '.gtar'   : 'application/x-gtar',
    '.gif'    : 'image/gif',
    '.gz'     : 'application/x-gzip',
    '.hta'    : 'application/hta',
    '.htc'    : 'text/x-component',
    '.htt'    : 'text/webviewhtml',
    '.h'      : 'text/plain',
    '.hdf'    : 'application/x-hdf',
    '.hlp'    : 'application/winhlp',
    '.html'   : 'text/html',
    '.htm'    : 'text/html',
    '.hqx'    : 'application/mac-binhex40',
    '.isp'    : 'application/x-internet-signup',
    '.iii'    : 'application/x-iphone',
    '.ief'    : 'image/ief',
    '.ivf'    : 'video/x-ivf',
    '.ins'    : 'application/x-internet-signup',
    '.ico'    : 'image/x-icon',
    '.jpg'    : 'image/jpeg',
    '.jfif'   : 'image/pjpeg',
    '.jpe'    : 'image/jpeg',
    '.jpeg'   : 'image/jpeg',
    '.js'     : 'application/x-javascript',
    '.lsx'    : 'video/x-la-asf',
    '.latex'  : 'application/x-latex',
    '.lsf'    : 'video/x-la-asf',
    '.mhtml'  : 'message/rfc822',
    '.mny'    : 'application/x-msmoney',
    '.mht'    : 'message/rfc822',
    '.mid'    : 'audio/mid',
    '.mpv2'   : 'video/mpeg',
    '.man'    : 'application/x-troff-man',
    '.mvb'    : 'application/x-msmediaview',
    '.mpeg'   : 'video/mpeg',
    '.m3u'    : 'audio/x-mpegurl',
    '.mdb'    : 'application/x-msaccess',
    '.mpp'    : 'application/vnd.ms-project',
    '.m1v'    : 'video/mpeg',
    '.mpa'    : 'video/mpeg',
    '.me'     : 'application/x-troff-me',
    '.m13'    : 'application/x-msmediaview',
    '.movie'  : 'video/x-sgi-movie',
    '.m14'    : 'application/x-msmediaview',
    '.mpe'    : 'video/mpeg',
    '.mp2'    : 'video/mpeg',
    '.mov'    : 'video/quicktime',
    '.mp3'    : 'audio/mpeg',
    '.mpg'    : 'video/mpeg',
    '.ms'     : 'application/x-troff-ms',
    '.nc'     : 'application/x-netcdf',
    '.nws'    : 'message/rfc822',
    '.oda'    : 'application/oda',
    '.ods'    : 'application/oleobject',
    '.pmc'    : 'application/x-perfmon',
    '.p7r'    : 'application/x-pkcs7-certreqresp',
    '.p7b'    : 'application/x-pkcs7-certificates',
    '.p7s'    : 'application/pkcs7-signature',
    '.pmw'    : 'application/x-perfmon',
    '.ps'     : 'application/postscript',
    '.p7c'    : 'application/pkcs7-mime',
    '.pbm'    : 'image/x-portable-bitmap',
    '.ppm'    : 'image/x-portable-pixmap',
    '.pub'    : 'application/x-mspublisher',
    '.png'    : 'image/png',
    '.pnm'    : 'image/x-portable-anymap',
    '.pml'    : 'application/x-perfmon',
    '.p10'    : 'application/pkcs10',
    '.pfx'    : 'application/x-pkcs12',
    '.p12'    : 'application/x-pkcs12',
    '.pdf'    : 'application/pdf',
    '.pps'    : 'application/vnd.ms-powerpoint',
    '.p7m'    : 'application/pkcs7-mime',
    '.pko'    : 'application/vndms-pkipko',
    '.ppt'    : 'application/vnd.ms-powerpoint',
    '.pmr'    : 'application/x-perfmon',
    '.pma'    : 'application/x-perfmon',
    '.pot'    : 'application/vnd.ms-powerpoint',
    '.prf'    : 'application/pics-rules',
    '.pgm'    : 'image/x-portable-graymap',
    '.qt'     : 'video/quicktime',
    '.ra'     : 'audio/x-pn-realaudio',
    '.rgb'    : 'image/x-rgb',
    '.ram'    : 'audio/x-pn-realaudio',
    '.rmi'    : 'audio/mid',
    '.ras'    : 'image/x-cmu-raster',
    '.roff'   : 'application/x-troff',
    '.rtf'    : 'application/rtf',
    '.rtx'    : 'text/richtext',
    '.sv4crc' : 'application/x-sv4crc',
    '.spc'    : 'application/x-pkcs7-certificates',
    '.setreg' : 'application/set-registration-initiation',
    '.snd'    : 'audio/basic',
    '.stl'    : 'application/vndms-pkistl',
    '.setpay' : 'application/set-payment-initiation',
    '.stm'    : 'text/html',
    '.shar'   : 'application/x-shar',
    '.sh'     : 'application/x-sh',
    '.sit'    : 'application/x-stuffit',
    '.spl'    : 'application/futuresplash',
    '.sct'    : 'text/scriptlet',
    '.scd'    : 'application/x-msschedule',
    '.sst'    : 'application/vndms-pkicertstore',
    '.src'    : 'application/x-wais-source',
    '.sv4cpio': 'application/x-sv4cpio',
    '.tex'    : 'application/x-tex',
    '.tgz'    : 'application/x-compressed',
    '.t'      : 'application/x-troff',
    '.tar'    : 'application/x-tar',
    '.tr'     : 'application/x-troff',
    '.tif'    : 'image/tiff',
    '.txt'    : 'text/plain',
    '.texinfo': 'application/x-texinfo',
    '.trm'    : 'application/x-msterminal',
    '.tiff'   : 'image/tiff',
    '.tcl'    : 'application/x-tcl',
    '.texi'   : 'application/x-texinfo',
    '.tsv'    : 'text/tab-separated-values',
    '.ustar'  : 'application/x-ustar',
    '.uls'    : 'text/iuls',
    '.vcf'    : 'text/x-vcard',
    '.wps'    : 'application/vnd.ms-works',
    '.wav'    : 'audio/wav',
    '.wrz'    : 'x-world/x-vrml',
    '.wri'    : 'application/x-mswrite',
    '.wks'    : 'application/vnd.ms-works',
    '.wmf'    : 'application/x-msmetafile',
    '.wcm'    : 'application/vnd.ms-works',
    '.wrl'    : 'x-world/x-vrml',
    '.wdb'    : 'application/vnd.ms-works',
    '.wsdl'   : 'text/xml',
    '.xml'    : 'text/xml',
    '.xlm'    : 'application/vnd.ms-excel',
    '.xaf'    : 'x-world/x-vrml',
    '.xla'    : 'application/vnd.ms-excel',
    '.xls'    : 'application/vnd.ms-excel',
    '.xof'    : 'x-world/x-vrml',
    '.xlt'    : 'application/vnd.ms-excel',
    '.xlc'    : 'application/vnd.ms-excel',
    '.xsl'    : 'text/xml',
    '.xbm'    : 'image/x-xbitmap',
    '.xlw'    : 'application/vnd.ms-excel',
    '.xpm'    : 'image/x-xpixmap',
    '.xwd'    : 'image/x-xwindowdump',
    '.xsd'    : 'text/xml',
    '.z'      : 'application/x-compress',
    '.zip'    : 'application/x-zip-compressed',
    '.*'      : 'application/octet-stream',
    # Office 2007 MIME types
    # http://www.bram.us/2007/05/25/office-2007-mime-types-for-iis/
    'docm'    : 'application/vnd.ms-word.document.macroEnabled.12',
    'docx'    : 'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
    'dotm'    : 'application/vnd.ms-word.template.macroEnabled.12',
    'dotx'    : 'application/vnd.openxmlformats-officedocument.wordprocessingml.template',
    'potm'    : 'application/vnd.ms-powerpoint.template.macroEnabled.12',
    'potx'    : 'application/vnd.openxmlformats-officedocument.presentationml.template',
    'ppam'    : 'application/vnd.ms-powerpoint.addin.macroEnabled.12',
    'ppsm'    : 'application/vnd.ms-powerpoint.slideshow.macroEnabled.12',
    'ppsx'    : 'application/vnd.openxmlformats-officedocument.presentationml.slideshow',
    'pptm'    : 'application/vnd.ms-powerpoint.presentation.macroEnabled.12',
    'pptx'    : 'application/vnd.openxmlformats-officedocument.presentationml.presentation',
    'xlam'    : 'application/vnd.ms-excel.addin.macroEnabled.12',
    'xlsb'    : 'application/vnd.ms-excel.sheet.binary.macroEnabled.12',
    'xlsm'    : 'application/vnd.ms-excel.sheet.macroEnabled.12',
    'xlsx'    : 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
    'xltm'    : 'application/vnd.ms-excel.template.macroEnabled.12',
    'xltx'    : 'application/vnd.openxmlformats-officedocument.spreadsheetml.template',
}

def parse_s3obj_path(path):
    """Parses an S3 object path into its bucket and key constituents."""
    i = path.find('/')
    return i >= 0 and (path[0:i], path[i+1:]) or (path, '')

def copy_stream(source, dest, length):
    buffer = Array.CreateInstance(Byte, 8192)
    while length > 0:
        bytesRead = source.Read(buffer, 0, buffer.Length)
        if bytesRead > 0:
            dest.Write(buffer, 0, bytesRead)
        else:
            raise Exception("Unexpected end of stream while copying.")
        length -= bytesRead

class S3Commander(object):
    
    def __init__(self, s3):
        self.s3 = s3
    
    def buckets(self, args):
        """Lists all buckets."""
        print '\n'.join(
            ['%s  %s' % (b.CreationDate.ToString('r'), b.Name) for b in self.s3.GetAllBuckets()])

    def ls(self, args):
        self.list(args)
 
    def list(self, args):
        """Lists all objects in a bucket, optionally constrained by a prefix."""
        if not args:
            raise Exception('Missing S3 path.')
        bucket, prefix = parse_s3obj_path(args.pop(0))
        objs = self.s3.ListObjects(bucket, prefix)
        for obj in objs:
            if type(obj) == CommonPrefix:
                print ' ' * 53 + obj.Prefix
            else:
                print '%s  %20s  %s' % (
                    obj.LastModified.ToString('r'), 
                    obj.Size.ToString('N0'),
                    obj.Key[len(prefix):])

    def put(self, args):
        """Puts a local file as an object in a bucket."""
        if not args:
            raise Exception('Missing target object path.')
        bucket, key = parse_s3obj_path(args.pop(0))
        if not args:
            raise Exception('Missing local file path.')
        fpath = args.pop(0)
        if not key or key[-1] == '/':
            key = (key and key or '') + Path.GetFileName(fpath)
        content_type = MIME_MAP.get(Path.GetExtension(fpath), 'application/octet-stream')
        fname = Path.GetFileName(fpath)
        print 'Uploading %s (%s bytes) as %s...' % (fname, FileInfo(fpath).Length.ToString('N0'), content_type),
        self.s3.AddObject(fpath, bucket, key, content_type, CannedAcl.Private)
        print 'OK'

    def puts(self, args):
        """Puts text from standard input as an object in a bucket."""
        if not args:
            raise Exception('Missing target object for text.')
        bucket, key = parse_s3obj_path(args.pop(0))
        if not key:
            raise Exception('Missing key for text.')
        txt = sys.stdin.read()
        print 'Uploading %s characters of text...' % len(txt).ToString("N0"),
        self.s3.AddObjectString(txt, bucket, key, 'text/plain', CannedAcl.Private)
        print 'OK'

    def get(self, args):
        """Gets an object from a bucket as a local file."""
        if not args:
            raise Exception('Missing source object path.')
        bucket, key = parse_s3obj_path(args.pop(0))
        if not key:
            raise Exception('Missing key.')
        name = key.split('/')[-1]
        fpath = args and args.pop(0) or None
        isdir = Directory.Exists(fpath)
        if not fpath or isdir:
            fpath =  (isdir and fpath + '\\' or '') + name
        print 'Downloading %s to %s...' % (key, Path.GetFileName(fpath)),
        self.s3.GetObject(bucket, key, fpath)
        print 'OK'

    def gets(self, args):
        """Sends an object from a bucket to standard output."""
        print self.__gets(args)[-1]

    def pops(self, args):
        """Removes and sends an object from a bucket to standard output."""
        bucket, key, txt = self.__gets(args)
        print txt
        self.rm([bucket + '/' + key])

    def rm(self, args):
        """Removes an from a bucket."""
        if not args:
            raise Exception('Missing object path.')
        bucket, key = parse_s3obj_path(args.pop(0))
        if not key:
            raise Exception('Missing key.')
        self.s3.DeleteObject(bucket, key)

    def __gets(self, args):
        if not args:
            raise Exception('Missing source object path.')
        bucket, key = parse_s3obj_path(args.pop(0))
        if not key:
            raise Exception('Missing key.')
        content_type = clr.Reference[str]()
        content_length = clr.Reference[Int64]()
        input = self.s3.GetObjectStream(bucket, key, content_length, content_type)
        try:
            content_length, content_type = content_length.Value, content_type.Value
            if 'text/plain' != content_type:
                raise Exception('Object is %s, not plain/text.' % content_type)
            output = MemoryStream()
            copy_stream(input, output, content_length)
        finally:
            input.Close()
        txt = Encoding.UTF8.GetString(output.GetBuffer(), 0, content_length)
        return (bucket, key, txt)
        
class TrustAnyCertificatePolicy(ICertificatePolicy):
    def CheckValidationResult(self, sp, cert, request, problem):
        return True

def print_help():
    print """LitS3 Commander - $Revision$
Command-line interface to LitS3
http://lits3.googlecode.com/

Usage:

  %(this)s COMMAND AWS-KEY-ID AWS-SECRET-KEY ARGS
  %(this)s COMMAND - - ARGS
 
where:

  COMMAND is one of:
    buckets, list (ls), put, get, puts, gets, pops, rm (del)
  AWS-KEY-ACCESS-ID 
    Your AWS access key ID
  AWS-SECRET-ACCESS-KEY 
    Your AWS secret access key
  ARGS
    COMMAND-specific arguments

The access identifiers can be set into your environment. If you then 
use a dash where an identifier is expected then the corresponding value 
will be picked up from the environment. The environment variables 
sought are AWS_ACCESS_KEY_ID and AWS_SECRET_ACCESS_KEY. With those in 
place, you can simply resort to the second usage:
 
  %(this)s COMMAND - - ARGS
 
Each COMMAND has its own set of ARGS. Also as a general rule, each 
COMMAND that works with an object uses a a simple path scheme to 
identify the object. That scheme simply looks like this:
 
  BUCKET / KEY
 
Examples:
 
%(this)s buckets - -
  List all my buckets
 
%(this)s list - - foo
  List all objects in foo bucket
 
%(this)s list - - foo/images/
  List all objects in bucket foo with the common prefix of images/
 
%(this)s put - -  foo index.html
  Add local file named index.html as key index.html in bucket foo
 
%(this)s put - - foo/images/ ani.gif
  Add local file named ani.gif as key images/ani.gif in bucket foo
 
%(this)s put - - foo/images/animation.gif ani.gif
  Add local file named ani.gif as key images/animation.gif 
  in bucket foo
 
%(this)s get - - foo/index.html
  Get object with key index.html in bucket foo as local file named 
  index.html
 
%(this)s get - - foo/index.html bar.html
  Get object with key index.html in bucket foo as local file 
  named bar.html
 
%(this)s rm - - foo/index.html
  Remove the object with key index.html in the bucket foo
 
dir | %(this)s puts - - foo/dir.txt
  Puts the output from dir (on Windows; ls on Unix platforms) as a 
  plain text object named dir.txt in bucket foo
 
%(this)s gets - - foo/dir.txt
  Gets the plain text object named dir.txt in bucket foo and writes 
  its content to standard output
 
%(this)s pops - - foo/dir.txt
  Gets the plain text object named dir.txt in bucket foo, writes its 
  content to standard output and then removes the object.
""" % { 'this': Path.GetFileNameWithoutExtension(sys.argv[0]) }
        
def main(args):

    if not args:
        raise Exception('Missing command. Try help.')
    
    cmd_name = args.pop(0).replace('del', 'rm') # pop + alias
    if 'help' == cmd_name:
        print_help()
        return

    id = args and args.pop(0) or None
    if id == '-':
        id = GetEnvironmentVariable('AWS_ACCESS_KEY_ID')
    if not id:
        raise Exception('Missing AWS access key ID.')

    key = args and args.pop(0) or None
    if key == '-':
        key = GetEnvironmentVariable('AWS_SECRET_ACCESS_KEY')
    if not key:
        raise Exception('Missing AWS secret access key.')

    s3 = S3Service(AccessKeyID = id, SecretAccessKey = key)

    # TODO Review policy-hack to trust any certificate.
    # See: http://groups.google.com/group/lits3/t/7321c65188be7792
    ServicePointManager.CertificatePolicy = TrustAnyCertificatePolicy()    
    
    commander = S3Commander(s3)
    cmd = getattr(commander, cmd_name, None)
    
    if not cmd:
        raise Exception('Unknown command (%s).' % cmd_name)
    
    cmd(args)

if __name__ == '__main__':
    try:
        main(sys.argv[1:])
    except Exception, e:
        print >> sys.stderr, e
        sys.exit(1)

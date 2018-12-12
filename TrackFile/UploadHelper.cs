using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace TrackFile
{
    public static class UploadHelper
    {
        public static void TestUpload()
        {
            try
            {
                var filename = @"C:\Users\Administrator\Pictures\1.jpg";
                var url = "http://sgmw.umworks.com/Api/ParentChildApi/uploadPaper.aspx";
                var device = "abcd12345";
                var file = new FileInfo(filename);
                var iFileName = Path.GetFileNameWithoutExtension(filename);
                //todo
                var files = new string[1];
                files[0] = filename;
                var nvc = new NameValueCollection
                {
                    {"paperId", iFileName},
                    {"dealerId", device},
                };
                var str = WebHelper.UploadFilesToRemoteUrl(url, files, nvc);
                var serializer = new JavaScriptSerializer();
                var deserializedResult = serializer.Deserialize<UploadResultModel>(str);
                if (deserializedResult.code != 200)
                {
                    Console.WriteLine(deserializedResult.msg);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static bool UploadFiles(string url, string[] files, NameValueCollection data, out string msg)
        {
            msg = string.Empty;
            try
            {
                var str = WebHelper.UploadFilesToRemoteUrl(url, files, data);
                var serializer = new JavaScriptSerializer();
                var deserializedResult = serializer.Deserialize<UploadResultModel>(str);
                if (deserializedResult.code != 200)
                {
                    Console.WriteLine(deserializedResult.msg);
                    msg = deserializedResult.msg;
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return false;
        }
    }
}

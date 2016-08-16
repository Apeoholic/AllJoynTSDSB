using BridgeRT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdapterLib
{


    internal class AdapterIcon :IAdapterIcon
    {
        byte[] _image = null;
        public AdapterIcon(string url)
        {
            if (url.StartsWith("ms-appx:///"))
            {
                var s = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromUri(new Uri(url)).OpenReadAsync().AsTask();
                s.Wait();
                using (MemoryStream ms = new MemoryStream())
                {
                    s.Result.AsStreamForRead().CopyTo(ms);
                    _image = ms.ToArray();
                }
            }
            else
            {
                Url = url;
            }
        }

        public string MimeType { get; } = "image/png";
        public string Url { get; } = "";

        public byte[] GetImage()
        {
            return _image;
        }

    }
}

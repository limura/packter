using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace PackterViewer
{
    public class IPv4Tools
    {
        /// <summary>
        /// IPv4アドレス表記を 0 から1.0fまでの float へと変換します。
        /// 変換に失敗したら -1.0f を返します。 
        /// </summary>
        /// <param name="str">IPv4のアドレス表記(255.255.255.255のような奴)</param>
        /// <returns>IPv4アドレス表記を 0 から1.0fまでの float へと変換したもの</returns>
        public static double IPString2Double(string str)
        {
            try
            {
                if (str == null || str.Length <= 0)
                    return -1;
                double d = 0;
                // 0.1 のような表記の場合はそれを float だと思って変換する。
                if (str.IndexOf('.') >= 0 && str.IndexOf('.') == str.LastIndexOf('.') && double.TryParse(str, out d) == true && d < 1 && d >= 0)
                {
                    return d;
                }
                IPAddress addr = IPAddress.Parse(str);
                if (addr == null)
                {
                    return -1.0f;
                }
                byte[] ba = addr.GetAddressBytes();
                if (ba.Length < 4)
                    return -1.0f;

                d = 0;
                double dmax = 0;
                foreach (byte b in ba)
                {
                    d = d * 256 + b;
                    dmax = dmax * 256 + 255;
                }
                return d / dmax;
            }
            catch { return -1.0; }
        }
    }
}

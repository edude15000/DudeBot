using IgnitionHelper.CDLC;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace IgnitionHelper.Ignition
{
    public class IgnitionSearch
    {
        public string CFUserID;

        public int drawpass = 0;
        public CookieContainer ignitionRequestCookies = new CookieContainer();

        public IgnitionSearch(string userid, string laravel_session, string community_pass_hash, string ipsconnect)
        {
            CFUserID = userid;

            ignitionRequestCookies.Add(new Cookie("laravel_session", laravel_session, "/", "ignition.customsforge.com"));
            ignitionRequestCookies.Add(new Cookie("-community-member_id", CFUserID, "/", ".customsforge.com"));
            ignitionRequestCookies.Add(new Cookie("-community-pass_hash", community_pass_hash, "/", ".customsforge.com"));
            ignitionRequestCookies.Add(new Cookie(ipsconnect, "1", "/", ".customsforge.com"));

            Console.WriteLine("Search initialized");
        }

        /// <summary>
        /// Resolves a download url from cdlc id
        /// </summary>
        /// <param name="cdlcid"></param>
        /// <returns></returns>
        public Uri ResolveDownloadURL(long cdlcid)
        {
            Console.WriteLine("Resolving download url for cdlc id " + cdlcid);

            HttpWebRequest request = WebRequest.CreateHttp("http://customsforge.com/process.php?id=" + cdlcid);
            request.CookieContainer = ignitionRequestCookies;
            request.Method = "HEAD";
            request.AllowAutoRedirect = false;

            string location = null;
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                location = response.GetResponseHeader("Location");
            }

            return new Uri(location);
        }

        /// <summary>
        /// Fetches the @numrows recently updated cdlc
        /// </summary>
        public CDLCEntryList Search(int startrow, int numrows, string input)
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = ignitionRequestCookies;

            using (HttpClient cl = new HttpClient(handler, true))
            {
                Dictionary<string, string> postvars = new Dictionary<string, string>();

                //Number of draw pass
                postvars.Add("draw", (++drawpass).ToString());

                //ID
                postvars.Add("columns[0][data][_]", "19");
                postvars.Add("columns[0][data][display]", "undefined");
                postvars.Add("columns[0][name]", "");
                postvars.Add("columns[0][searchable]", "true");
                postvars.Add("columns[0][orderable]", "false");
                postvars.Add("columns[0][search][value]", "");
                postvars.Add("columns[0][search][regex]", "false");

                //Artist name
                postvars.Add("columns[1][data][_]", "1");
                postvars.Add("columns[1][data][display]", "undefined");
                postvars.Add("columns[1][name]", "");
                postvars.Add("columns[1][searchable]", "true");
                postvars.Add("columns[1][orderable]", "true");
                postvars.Add("columns[1][search][value]", "");
                postvars.Add("columns[1][search][regex]", "false");

                //Song name
                postvars.Add("columns[2][data][_]", "2");
                postvars.Add("columns[2][data][display]", "undefined");
                postvars.Add("columns[2][name]", "");
                postvars.Add("columns[2][searchable]", "true");
                postvars.Add("columns[2][orderable]", "true");
                postvars.Add("columns[2][search][value]", "");
                postvars.Add("columns[2][search][regex]", "false");

                //Album name
                postvars.Add("columns[3][data]", "3");
                postvars.Add("columns[3][name]", "");
                postvars.Add("columns[3][searchable]", "true");
                postvars.Add("columns[3][orderable]", "true");
                postvars.Add("columns[3][search][value]", "");
                postvars.Add("columns[3][search][regex]", "false");

                //Tuning
                postvars.Add("columns[4][data][_]", "4");
                postvars.Add("columns[4][data][display]", "undefined");
                postvars.Add("columns[4][name]", "");
                postvars.Add("columns[4][searchable]", "true");
                postvars.Add("columns[4][orderable]", "true");
                postvars.Add("columns[4][search][value]", "");
                postvars.Add("columns[4][search][regex]", "false");

                //Version
                postvars.Add("columns[5][data]", "5");
                postvars.Add("columns[5][name]", "");
                postvars.Add("columns[5][searchable]", "true");
                postvars.Add("columns[5][orderable]", "true");
                postvars.Add("columns[5][search][value]", "");
                postvars.Add("columns[5][search][regex]", "false");

                //Uploader name
                postvars.Add("columns[6][data]", "6");
                postvars.Add("columns[6][name]", "");
                postvars.Add("columns[6][searchable]", "true");
                postvars.Add("columns[6][orderable]", "true");
                postvars.Add("columns[6][search][value]", "");
                postvars.Add("columns[6][search][regex]", "false");

                //Uploaded Timestamp
                postvars.Add("columns[7][data][_]", "7");
                postvars.Add("columns[7][data][display]", "undefined");
                postvars.Add("columns[7][name]", "");
                postvars.Add("columns[7][searchable]", "true");
                postvars.Add("columns[7][orderable]", "true");
                postvars.Add("columns[7][search][value]", "");
                postvars.Add("columns[7][search][regex]", "false");

                //Updated Timestamp
                postvars.Add("columns[8][data][_]", "8");
                postvars.Add("columns[8][data][display]", "undefined");
                postvars.Add("columns[8][name]", "");
                postvars.Add("columns[8][searchable]", "true");
                postvars.Add("columns[8][orderable]", "true");
                postvars.Add("columns[8][search][value]", "");
                postvars.Add("columns[8][search][regex]", "false");

                //DL Count
                postvars.Add("columns[9][data]", "9");
                postvars.Add("columns[9][name]", "");
                postvars.Add("columns[9][searchable]", "true");
                postvars.Add("columns[9][orderable]", "true");
                postvars.Add("columns[9][search][value]", "");
                postvars.Add("columns[9][search][regex]", "false");

                //Parts
                postvars.Add("columns[10][data][_]", "10");
                postvars.Add("columns[10][data][display]", "undefined");
                postvars.Add("columns[10][name]", "");
                postvars.Add("columns[10][searchable]", "true");
                postvars.Add("columns[10][orderable]", "true");
                postvars.Add("columns[10][search][value]", "");
                postvars.Add("columns[10][search][regex]", "false");

                //DD
                postvars.Add("columns[11][data][_]", "11");
                postvars.Add("columns[11][data][filter]", "11");
                postvars.Add("columns[11][data][display]", "undefined");
                postvars.Add("columns[11][name]", "");
                postvars.Add("columns[11][searchable]", "true");
                postvars.Add("columns[11][orderable]", "true");
                postvars.Add("columns[11][search][value]", "");
                postvars.Add("columns[11][search][regex]", "false");

                //Platforms
                postvars.Add("columns[12][data][_]", "12");
                postvars.Add("columns[12][data][display]", "undefined");
                postvars.Add("columns[12][name]", "");
                postvars.Add("columns[12][searchable]", "true");
                postvars.Add("columns[12][orderable]", "true");
                postvars.Add("columns[12][search][value]", "");
                postvars.Add("columns[12][search][regex]", "false");

                //simplified song name?
                postvars.Add("columns[13][data]", "13");
                postvars.Add("columns[13][name]", "");
                postvars.Add("columns[13][searchable]", "true");
                postvars.Add("columns[13][orderable]", "true");
                postvars.Add("columns[13][search][value]", "");
                postvars.Add("columns[13][search][regex]", "false");

                //ID again?
                postvars.Add("columns[14][data]", "14");
                postvars.Add("columns[14][name]", "");
                postvars.Add("columns[14][searchable]", "true");
                postvars.Add("columns[14][orderable]", "true");
                postvars.Add("columns[14][search][value]", "");
                postvars.Add("columns[14][search][regex]", "false");

                //Official
                postvars.Add("columns[15][data]", "15");
                postvars.Add("columns[15][name]", "");
                postvars.Add("columns[15][searchable]", "true");
                postvars.Add("columns[15][orderable]", "true");
                postvars.Add("columns[15][search][value]", "");
                postvars.Add("columns[15][search][regex]", "false");

                //Info URL
                postvars.Add("columns[16][data]", "16");
                postvars.Add("columns[16][name]", "");
                postvars.Add("columns[16][searchable]", "true");
                postvars.Add("columns[16][orderable]", "true");
                postvars.Add("columns[16][search][value]", "");
                postvars.Add("columns[16][search][regex]", "false");

                //Video URL
                postvars.Add("columns[17][data]", "17");
                postvars.Add("columns[17][name]", "");
                postvars.Add("columns[17][searchable]", "true");
                postvars.Add("columns[17][orderable]", "true");
                postvars.Add("columns[17][search][value]", "");
                postvars.Add("columns[17][search][regex]", "false");

                //Tags
                postvars.Add("columns[18][data]", "18");
                postvars.Add("columns[18][name]", "");
                postvars.Add("columns[18][searchable]", "true");
                postvars.Add("columns[18][orderable]", "true");
                postvars.Add("columns[18][search][value]", "");
                postvars.Add("columns[18][search][regex]", "false");

                //unused?
                postvars.Add("columns[19][data]", "19");
                postvars.Add("columns[19][name]", "");
                postvars.Add("columns[19][searchable]", "true");
                postvars.Add("columns[19][orderable]", "true");
                postvars.Add("columns[19][search][value]", "");
                postvars.Add("columns[19][search][regex]", "false");

                //unused?
                postvars.Add("columns[20][data]", "20");
                postvars.Add("columns[20][name]", "");
                postvars.Add("columns[20][searchable]", "true");
                postvars.Add("columns[20][orderable]", "true");
                postvars.Add("columns[20][search][value]", "");
                postvars.Add("columns[20][search][regex]", "false");

                //Order
                postvars.Add("order[0][column]", "8");
                postvars.Add("order[0][dir]", "desc");

                //Number of results
                postvars.Add("start", startrow.ToString());
                postvars.Add("length", numrows.ToString());

                //Generic search text
                postvars.Add("search[value]", input);
                postvars.Add("search[regex]", "false");

                FormUrlEncodedContent post = new FormUrlEncodedContent(postvars);

                HttpResponseMessage response = cl.PostAsync("http://ignition.customsforge.com/cfss?u=" + CFUserID, post).Result;

                string responsetext = response.Content.ReadAsStringAsync().Result;

                string json_text = JObject.Parse(responsetext).ToString();

                return JsonConvert.DeserializeObject<CDLCEntryList>(json_text);
            }
        }
    }
}

using IgnitionHelper.CDLC;
using IgnitionHelper.Ignition;
using System;

namespace IgnitionHelper
{
    class Program
    {
        static void Main(string[] args)
        {
            //The users customsforge id
            string userid = "";

            //The laravel_session cookie
            string laravel_session = "";

            //The community_pass_hash cookie
            string community_pass_hash = "";

            //ipsconnect_xxxxx - the name of the cookie
            string ipsconnect = "";

            var search = new IgnitionSearch(userid, laravel_session, community_pass_hash, ipsconnect);

            CDLCEntryList results = search.Search(0, 100, "Iron Maiden", "the trooper");

            foreach (CDLCEntry result in results)
            {
                Console.WriteLine("{0} - {1}", result.artist, result.title);
            }

            Console.ReadKey();
        }
    }
}

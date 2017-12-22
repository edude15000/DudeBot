using System;
using System.Collections;
using System.Linq;

namespace IgnitionHelper.CDLC
{
    [Serializable]
    public class CDLCEntryList
    {
        public CDLCEntry[] data;

        /// <summary>
        /// Shortcut for data.Length
        /// </summary>
        public int Count {
            get {
                return data.Length;
            }
        }

        /// <summary>
        /// Shortcut for data enumerator, allows enumerating of the data array
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return data.GetEnumerator();
        }

        /// <summary>
        /// Returns the most recently updated CDLC from this entry list
        /// </summary>
        /// <returns></returns>
        public CDLCEntry GetNewest()
        {
            return (from cdlc in data orderby cdlc.updated descending select cdlc).First();
        }

        /// <summary>
        /// Prints all entries in the list in the format 'artist - song'
        /// </summary>
        public void Print()
        {
            foreach (CDLCEntry entry in this)
            {
                Console.WriteLine("\t{0} - {1}", entry.artist, entry.title);
            }

            Console.WriteLine("Newest:\t{0} - {1}", GetNewest().artist, GetNewest().title);
        }
    }
}

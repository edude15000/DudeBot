using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace IgnitionHelper.CDLC
{
    [Serializable]
    [JsonArray]
    class CDLCEntry
    {
        public long cdlcid;

        public string title;
        public string artist;
        public string album;

        public Tuning tuning;

        public string version;
        public string creator;

        public DateTime uploaded;
        public DateTime updated;

        public Part parts;

        public bool dd;

        public Platform platforms;

        public string simplename;

        public bool official;

        public string infourl;
        public string videourl;

        public Tag tags;

        public CDLCEntry()
        {

        }

        [JsonConstructor]
        public CDLCEntry(List<object> arr)
        {
            cdlcid = (long)arr[0];

            artist = WebUtility.HtmlDecode((string)arr[1]);
            title = WebUtility.HtmlDecode((string)arr[2]);
            album = WebUtility.HtmlDecode((string)arr[3]);

            switch ((string)arr[4])
            {
                case "fsharpstandard":
                    tuning = Tuning.HIGH_Fs_STANDARD;
                    break;
                case "fstandardhigh":
                    tuning = Tuning.HIGH_F_STANDARD;
                    break;

                case "estandard":
                    tuning = Tuning.E_STANDARD;
                    break;
                case "edropd":
                    tuning = Tuning.DROP_D;
                    break;

                case "eflatstandard":
                    tuning = Tuning.Eb_STANDARD;
                    break;
                case "eflatdropdflat":
                    tuning = Tuning.Eb_DROP_Db;
                    break;

                case "dstandard":
                    tuning = Tuning.D_STANDARD;
                    break;
                case "ddropc":
                    tuning = Tuning.D_DROP_C;
                    break;

                case "csharpstandard":
                    tuning = Tuning.Cs_STANDARD;
                    break;
                case "csharpdropb":
                    tuning = Tuning.Cs_DROP_B;
                    break;

                case "cstandard":
                    tuning = Tuning.C_STANDARD;
                    break;
                case "cdropbflat":
                    tuning = Tuning.C_DROP_Bb;
                    break;

                case "bstandard":
                    tuning = Tuning.B_STANDARD;
                    break;
                case "bdropa":
                    tuning = Tuning.B_DROP_A;
                    break;

                case "bflatstandard":
                    tuning = Tuning.Bb_STANDARD;
                    break;
                case "bflatdropaflat":
                    tuning = Tuning.Bb_DROP_Ab;
                    break;

                case "astandard":
                    tuning = Tuning.A_STANDARD;
                    break;
                case "aflatstandard":
                    tuning = Tuning.Ab_STANDARD;
                    break;

                case "gstandard":
                    tuning = Tuning.G_STANDARD;
                    break;
                case "gflatstandard":
                    tuning = Tuning.LOW_Gb_STANDARD;
                    break;

                case "fstandard":
                    tuning = Tuning.LOW_F_STANDARD;
                    break;

                case "octavestandard":
                    tuning = Tuning.OCTAVE;
                    break;

                case "opena":
                    tuning = Tuning.OPEN_A;
                    break;
                case "openb":
                    tuning = Tuning.OPEN_B;
                    break;
                case "openc":
                    tuning = Tuning.OPEN_C;
                    break;
                case "opend":
                    tuning = Tuning.OPEN_D;
                    break;
                case "opene":
                    tuning = Tuning.OPEN_E;

                    break;
                case "openf":
                    tuning = Tuning.OPEN_F;

                    break;
                case "openg":
                    tuning = Tuning.OPEN_G;

                    break;

                case "celtic":
                    tuning = Tuning.CELTIC;

                    break;
                case "other":
                    tuning = Tuning.OTHER;

                    break;
                default:
                    throw new Exception("Unknown tuning: " + arr[4]);
            }

            version = (string)arr[5];
            creator = WebUtility.HtmlDecode((string)arr[6]);

            uploaded = DateTimeOffset.FromUnixTimeSeconds((long)arr[7]).DateTime;
            updated = DateTimeOffset.FromUnixTimeSeconds((long)arr[8]).DateTime;

            foreach (string part in ((string)arr[10]).Split(','))
            {
                switch (part)
                {
                    case "lead":
                        parts = parts | Part.LEAD;
                        break;
                    case "rhythm":
                        parts = parts | Part.RHYTHM;
                        break;
                    case "bass":
                        parts = parts | Part.BASS;
                        break;
                    case "vocals":
                        parts = parts | Part.VOCALS;
                        break;
                    case "":
                        break;
                    default:
                        throw new Exception("Unknown part: " + part);
                }
            }

            dd = (bool)arr[11];

            foreach (string platform in ((string)arr[12]).Split(','))
            {
                switch (platform)
                {
                    case "pc":
                        platforms = platforms | Platform.PC;
                        break;
                    case "ps3":
                        platforms = platforms | Platform.PS3;
                        break;
                    case "xbox360":
                        platforms = platforms | Platform.XBOX360;
                        break;
                    case "mac":
                        platforms = platforms | Platform.MAC;
                        break;
                    case "":
                        break;
                    default:
                        throw new Exception("Unknown platform: " + platform);
                }
            }

            simplename = (string)arr[13];

            official = (bool)arr[15];

            infourl = (string)arr[16];
            videourl = (string)arr[17];

            if (arr[18] == null)
            {
                tags = Tag.NONE;
            }
            else
            {
                foreach (string tag in ((string)arr[18]).Split(','))
                {
                    switch (tag)
                    {
                        case "ii_capolead":
                            tags = tags | Tag.CAPO_LEAD;
                            break;
                        case "ii_caporhythm":
                            tags = tags | Tag.CAPO_RHYTHM;
                            break;
                        case "ii_slidelead":
                            tags = tags | Tag.SLIDE_LEAD;
                            break;
                        case "ii_sliderhythm":
                            tags = tags | Tag.SLIDE_RHYTHM;
                            break;
                        case "ii_5stringbass":
                            tags = tags | Tag.FIVE_STRING_BASS;
                            break;
                        case "ii_6stringbass":
                            tags = tags | Tag.SIX_STRING_BASS;
                            break;
                        case "ii_7stringguitar":
                            tags = tags | Tag.SEVEN_STRING_GUITAR;
                            break;
                        case "ii_12stringguitar":
                            tags = tags | Tag.TWELVE_STRING_GUITAR;
                            break;
                        case "ii_heavystrings":
                            tags = tags | Tag.HEAVY_STRINGS;
                            break;
                        case "ii_tremolo":
                            tags = tags | Tag.TREMOLO;
                            break;
                        case "":
                            break;
                        default:
                            throw new Exception("Unknown tag: " + tag);
                    }
                }
            }

            /*
             * 0:long - CDLC ID
             * 1:string - Arist name
             * 2:string - Song title
             * 3:string - Album title
             * 4:string - Tuning
             * 5:string - Version
             * 6:string - Creator name
             * 7:ulong - Uploaded timestamp
             * 8:ulong - Updated timestamp
             * 9:int - Download count
             * 10:comma separated string - Parts
             * 11:bool - Dynamic Difficulty
             * 12:comma separated string - Platforms
             * 13:string - Simplified name, probably for forum topic link
             * 14:long - CDLC ID again
             * 15:bool - Official dlc
             * 16:string - info url
             * 17:string - video url
             * 18:comma separated string - tags
             * 19: unused/unknown
             * 20: unused/unknown
            */

            //Console.WriteLine( "Parsed: " + title + " - " + artist );
        }
    }
}

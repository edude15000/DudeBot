using System;

namespace IgnitionHelper.CDLC
{
    public enum Tuning
    {
        HIGH_Fs_STANDARD,
        HIGH_F_STANDARD,

        E_STANDARD,
        DROP_D,

        Eb_STANDARD,
        Eb_DROP_Db,

        D_STANDARD,
        D_DROP_C,

        Cs_STANDARD,
        Cs_DROP_B,

        C_STANDARD,
        C_DROP_Bb,

        B_STANDARD,
        B_DROP_A,

        Bb_STANDARD,
        Bb_DROP_Ab,

        A_STANDARD,
        Ab_STANDARD,

        G_STANDARD,
        LOW_Gb_STANDARD,

        LOW_F_STANDARD,

        OCTAVE,

        OPEN_A,
        OPEN_B,
        OPEN_C,
        OPEN_D,
        OPEN_E,
        OPEN_F,
        OPEN_G,

        CELTIC,
        OTHER
    }

    [Flags]
    public enum Tag
    {
        NONE = 0,

        CAPO_LEAD = 1 << 0,
        CAPO_RHYTHM = 1 << 1,

        SLIDE_LEAD = 1 << 2,
        SLIDE_RHYTHM = 1 << 3,

        FIVE_STRING_BASS = 1 << 4,
        SIX_STRING_BASS = 1 << 5,

        SEVEN_STRING_GUITAR = 1 << 6,
        TWELVE_STRING_GUITAR = 1 << 7,

        HEAVY_STRINGS = 1 << 8,
        TREMOLO = 1 << 9
    }

    [Flags]
    public enum Part
    {
        LEAD = 1 << 0,
        RHYTHM = 1 << 1,
        BASS = 1 << 2,
        VOCALS = 1 << 3,
        ALL = LEAD | RHYTHM | BASS | VOCALS
    }

    [Flags]
    public enum Platform
    {
        PC = 1 << 0,
        PS3 = 1 << 1,
        XBOX360 = 1 << 2,
        MAC = 1 << 3,
        ALL = PC | PS3 | XBOX360 | MAC
    }
}

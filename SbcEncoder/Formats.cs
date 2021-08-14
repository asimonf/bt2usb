using System;

namespace SbcEncoder
{
    public class Formats
    {
        public static ushort bswap_16(ushort v)
        {
            return (ushort) (
                ((v << 8) & 0xFF00) |
                ((v >> 8) & 0x00FF)
            );
        }
        
        public static uint bswap_32(uint v)
        {
            return ((v << 24) & 0xFF000000) |
                   ((v << 08) & 0x00FF0000) |
                   ((v >> 08) & 0x0000FF00) |
                   ((v >> 24) & 0x000000FF);
        }
        
        public static int COMPOSE_ID(int a, int b, int c, int d)
        {
            return a | (b << 8) | (c << 16) | (d << 24);
        }

        public static int LE_SHORT(int v) => v;
        public static int LE_INT(int v) => v;
        public static ushort BE_SHORT(ushort v) => bswap_16(v);
        public static uint BE_INT(uint v) => bswap_32(v);

        public static readonly uint AU_MAGIC = (uint) COMPOSE_ID('.', 's', 'n', 'd');

        public const int AU_FMT_ULAW = 1;
        public const int AU_FMT_LIN8 = 2;
        public const int AU_FMT_LIN16 = 3;

        public struct au_header
        {
            public uint magic; /* '.snd' */
            public uint hdr_size; /* size of header (min 24) */
            public uint data_size; /* size of data */
            public uint encoding; /* see to AU_FMT_XXXX */
            public uint sample_rate; /* sample rate */
            public uint channels; /* number of channels (voices) */
        };
    }
}
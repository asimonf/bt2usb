using System;
using System.IO;
using static SbcEncoder.Formats;
using static SharpSBC.Native;

namespace SbcEncoder
{
    static class Program
    {
        const int BUF_SIZE = 32768;

        static bool verbose = false;
        
        static int Main(string[] args)
        {
            int subBands = 8, bitpool = 25, blocks = 16;

            var joint = false;
            var dualChannel = true;
            var snr = false;
            
            verbose = true;

            if (args.Length > 0)
            {
                foreach (var file in args)
                    encode(file, subBands, bitpool, joint, dualChannel, snr, blocks);                
            }
            else
            {
                encode("test_32k.au", subBands, bitpool, joint, dualChannel, snr, blocks);
            }
            
            return 0;
        }

        static unsafe void encode(string filename, int subbands, int bitpool, bool joint,
            bool dualchannel, bool snr, int blocks)
        {
            au_header au_hdr;
            sbc_t sbc;
            int size, srate, codesize, nframes;
            long encoded;
            long len;

            const int inputSize = BUF_SIZE;
            const int outputSize = BUF_SIZE + BUF_SIZE / 4;

            var input = stackalloc byte[inputSize];
            var output = stackalloc byte[outputSize];


            FileStream stream;
            FileStream outputStream;

            if (sizeof(au_header) != 24)
            {
                /* Sanity check just in case */
                Console.WriteLine("FIXME: sizeof(au_hdr) != 24");
                return;
            }

            if (string.CompareOrdinal(filename, "-") != 0)
            {
                stream = File.OpenRead(filename);
                outputStream =
                    File.OpenWrite(Path.Combine(Path.GetDirectoryName(filename) ?? string.Empty, "output.sbc"));
            }
            else
                throw new NotImplementedException();

            try
            {
                len = stream.Read(new Span<byte>(&au_hdr, sizeof(au_header)));
                if (len < sizeof(au_header))
                {
                    // if (fd > fileno(stderr))
                    // 	fprintf(stderr, "Can't read header from file %s: %s\n",
                    // 				filename, strerror(errno));
                    // else
                    // 	perror("Can't read audio header");
                }

                if (au_hdr.magic != AU_MAGIC ||
                    BE_INT(au_hdr.hdr_size) > 128 ||
                    BE_INT(au_hdr.hdr_size) < sizeof(au_header) ||
                    BE_INT(au_hdr.encoding) != AU_FMT_LIN16)
                {
                    // fprintf(stderr, "Not in Sun/NeXT audio S16_BE format\n");
                    return;
                }

                sbc_init(&sbc, 0);

                sbc.frequency = BE_INT(au_hdr.sample_rate) switch
                {
                    16000 => SBC_FREQ_16000,
                    32000 => SBC_FREQ_32000,
                    44100 => SBC_FREQ_44100,
                    48000 => SBC_FREQ_48000,
                    _ => sbc.frequency
                };

                sbc.subbands = (byte) (subbands == 4 ? SBC_SB_4 : SBC_SB_8);

                if (BE_INT(au_hdr.channels) == 1)
                {
                    sbc.mode = SBC_MODE_MONO;
                    if (joint || dualchannel)
                    {
                        // fprintf(stderr, "Audio is mono but joint or "
                        // 	"dualchannel mode has been specified\n");
                        return;
                    }
                }
                else if (joint && !dualchannel)
                    sbc.mode = SBC_MODE_JOINT_STEREO;
                else if (!joint && dualchannel)
                    sbc.mode = SBC_MODE_DUAL_CHANNEL;
                else if (!joint && !dualchannel)
                    sbc.mode = SBC_MODE_STEREO;
                else
                {
                    // fprintf(stderr, "Both joint and dualchannel mode have been "
                    // 						"specified\n");
                    return;
                }

                sbc.endian = SBC_BE;
                /* Skip extra bytes of the header if any */

                if (stream.Read(new Span<byte>(input, (int) (BE_INT(au_hdr.hdr_size) - len))) < 0)
                    return;

                sbc.bitpool = (byte) bitpool;
                sbc.allocation = (byte) (snr ? SBC_AM_SNR : SBC_AM_LOUDNESS);

                sbc.blocks = blocks switch
                {
                    4 => SBC_BLK_4,
                    8 => SBC_BLK_8,
                    12 => SBC_BLK_12,
                    _ => SBC_BLK_16
                };

                if (verbose)
                {
                    // fprintf(stderr, "encoding %s with rate %d, %d blocks, "
                    // 	"%d subbands, %d bits, allocation method %s, "
                    // 					"and mode %s\n",
                    // 	filename, srate, blocks, subbands, bitpool,
                    // 	sbc.allocation == SBC_AM_SNR ? "SNR" : "LOUDNESS",
                    // 	sbc.mode == SBC_MODE_MONO ? "MONO" :
                    // 			sbc.mode == SBC_MODE_STEREO ?
                    // 				"STEREO" : "JOINTSTEREO");
                }

                codesize = (int) sbc_get_codesize(&sbc);
                nframes = 1;
                var count = 1;
                
                Console.WriteLine(codesize);
                Console.WriteLine(nframes);

                while (count-- > 0)
                {
                    /* read data for up to 'nframes' frames of input data */
                    size = stream.Read(new Span<byte>(input, codesize * nframes));
                    if (size < 0)
                    {
                        /* Something really bad happened */
                        // perror("Can't read audio data");
                        break;
                    }

                    if (size < codesize)
                    {
                        /* Not enough data for encoding even a single frame */
                        break;
                    }

                    /* encode all the data from the input buffer in a loop */
                    var inp = input;
                    var outp = output;

                    while (size >= codesize)
                    {
                        len = sbc_encode(&sbc, inp, (ulong)codesize,
                            outp, (ulong) (outputSize - (outp - output)),
                            &encoded);
                        if (len != codesize || encoded <= 0)
                        {
                            // fprintf(stderr,
                            // 	"sbc_encode fail, len=%zd, encoded=%lu\n",
                            // 	len, (unsigned long) encoded);
                            break;
                        }

                        size = (int) (size - len);
                        inp += len;
                        outp += encoded;
                    }

                    outputStream.Write(new ReadOnlySpan<byte>(output, (int) (outp - output)));
                    if (size != 0)
                    {
                        /*
                         * sbc_encode failure has been detected earlier or end
                         * of file reached (have trailing partial data which is
                         * insufficient to encode SBC frame)
                         */
                        break;
                    }
                }

                sbc_finish(&sbc);
            }
            finally
            {
                stream.Dispose();
                outputStream.Dispose();
            }
        }
    }
}
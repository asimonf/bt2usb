using System;
using System.Runtime.InteropServices;
using static SharpSBC.Native;

namespace TestServer
{
    public unsafe class SbcEncoder: IDisposable
    {
        private sbc_t* _sbc;

        public ulong Codesize { get; }

        public SbcEncoder(int sampleRate, int subbands, int bitpool, bool joint,
            bool dualchannel, bool snr, int blocks)
        {
            _sbc = (sbc_t*) Marshal.AllocHGlobal(new IntPtr(sizeof(sbc_t)));

            sbc_init(_sbc, 0);

            _sbc->frequency = sampleRate switch
            {
                16000 => SBC_FREQ_16000,
                32000 => SBC_FREQ_32000,
                44100 => SBC_FREQ_44100,
                48000 => SBC_FREQ_48000,
                _ => _sbc->frequency
            };
            
            _sbc->subbands = (byte) (subbands == 4 ? SBC_SB_4 : SBC_SB_8);
            
            if (joint && !dualchannel)
                _sbc->mode = SBC_MODE_JOINT_STEREO;
            else if (!joint && dualchannel)
                _sbc->mode = SBC_MODE_DUAL_CHANNEL;
            else if (!joint)
                _sbc->mode = SBC_MODE_STEREO;

            _sbc->endian = SBC_LE;
            
            _sbc->bitpool = (byte) bitpool;
            _sbc->allocation = (byte) (snr ? SBC_AM_SNR : SBC_AM_LOUDNESS);

            _sbc->blocks = blocks switch
            {
                4 => SBC_BLK_4,
                8 => SBC_BLK_8,
                12 => SBC_BLK_12,
                _ => SBC_BLK_16
            };

            Codesize = sbc_get_codesize(_sbc);
        }
        
        public unsafe long Encode(byte* src, byte* dst, ulong dstSize, out long encoded)
        {
            long tmp;
            var len = sbc_encode(_sbc, src, Codesize, dst, dstSize, &tmp);
            encoded = tmp;

            return len;
        }

        public unsafe void Dispose()
        {
            sbc_finish(_sbc);
            Marshal.FreeHGlobal(new IntPtr(_sbc));
        }
    }
}
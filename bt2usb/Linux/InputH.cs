// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo
// ReSharper disable FieldCanBeMadeReadOnly.Global

using System.Runtime.InteropServices;
using Tmds.Linux;
using static bt2usb.Linux.IoctlH;

namespace bt2usb.Linux
{
    public static class InputH
    {
        /*
         * Protocol version.
         */
        public const int EV_VERSION = 0x010001;

        public const uint INPUT_KEYMAP_BY_INDEX = 1;

        /*
         * IDs.
         */

        public const uint ID_BUS = 0;
        public const uint ID_VENDOR = 1;
        public const uint ID_PRODUCT = 2;
        public const uint ID_VERSION = 3;

        public const uint BUS_PCI = 0x01;
        public const uint BUS_ISAPNP = 0x02;
        public const uint BUS_USB = 0x03;
        public const uint BUS_HIL = 0x04;
        public const uint BUS_BLUETOOTH = 0x05;
        public const uint BUS_VIRTUAL = 0x06;

        public const uint BUS_ISA = 0x10;
        public const uint BUS_I8042 = 0x11;
        public const uint BUS_XTKBD = 0x12;
        public const uint BUS_RS232 = 0x13;
        public const uint BUS_GAMEPORT = 0x14;
        public const uint BUS_PARPORT = 0x15;
        public const uint BUS_AMIGA = 0x16;
        public const uint BUS_ADB = 0x17;
        public const uint BUS_I2C = 0x18;
        public const uint BUS_HOST = 0x19;
        public const uint BUS_GSC = 0x1A;
        public const uint BUS_ATARI = 0x1B;
        public const uint BUS_SPI = 0x1C;
        public const uint BUS_RMI = 0x1D;
        public const uint BUS_CEC = 0x1E;
        public const uint BUS_INTEL_ISHTP = 0x1F;

        /*
         * MT_TOOL types
         */
        public const uint MT_TOOL_FINGER = 0x00;
        public const uint MT_TOOL_PEN = 0x01;
        public const uint MT_TOOL_PALM = 0x02;
        public const uint MT_TOOL_DIAL = 0x0a;
        public const uint MT_TOOL_MAX = 0x0f;

        /*
         * Values describing the status of a force-feedback effect
         */
        public const uint FF_STATUS_STOPPED = 0x00;
        public const uint FF_STATUS_PLAYING = 0x01;
        public const uint FF_STATUS_MAX = 0x01;

        /*
         * Force feedback effect types
         */

        public const uint FF_RUMBLE = 0x50;
        public const uint FF_PERIODIC = 0x51;
        public const uint FF_CONSTANT = 0x52;
        public const uint FF_SPRING = 0x53;
        public const uint FF_FRICTION = 0x54;
        public const uint FF_DAMPER = 0x55;
        public const uint FF_INERTIA = 0x56;
        public const uint FF_RAMP = 0x57;

        public const uint FF_EFFECT_MIN = FF_RUMBLE;
        public const uint FF_EFFECT_MAX = FF_RAMP;

        /*
         * Force feedback periodic effect types
         */

        public const uint FF_SQUARE = 0x58;
        public const uint FF_TRIANGLE = 0x59;
        public const uint FF_SINE = 0x5a;
        public const uint FF_SAW_UP = 0x5b;
        public const uint FF_SAW_DOWN = 0x5c;
        public const uint FF_CUSTOM = 0x5d;

        public const uint FF_WAVEFORM_MIN = FF_SQUARE;
        public const uint FF_WAVEFORM_MAX = FF_CUSTOM;

        /*
         * Set ff device properties
         */

        public const uint FF_GAIN = 0x60;
        public const uint FF_AUTOCENTER = 0x61;

        /*
         * ff->playback(effect_id = FF_GAIN) is the first effect_id to
         * cause a collision with another ff method, in this case ff->set_gain().
         * Therefore the greatest safe value for effect_id is FF_GAIN - 1,
         * and thus the total number of effects should never exceed FF_GAIN.
         */
        public const uint FF_MAX_EFFECTS = FF_GAIN;

        public const uint FF_MAX = 0x7f;
        public const uint FF_CNT = FF_MAX + 1;

        public static uint EVIOCGVERSION => _IOR<int>('E', 0x01); /* get driver version */
        public static uint EVIOCGID => _IOR<input_id>('E', 0x02); /* get device ID */
        public static uint EVIOCGREP => _IOR<helper_repeat_settings>('E', 0x03); /* get repeat settings */
        public static uint EVIOCSREP => _IOW<helper_repeat_settings>('E', 0x03); /* set repeat settings */

        public static uint EVIOCGKEYCODE => _IOR<helper_keycode>('E', 0x04); /* get keycode */
        public static uint EVIOCGKEYCODE_V2 => _IOR<input_keymap_entry>('E', 0x04);
        public static uint EVIOCSKEYCODE => _IOW<helper_keycode>('E', 0x04); /* set keycode */
        public static uint EVIOCSKEYCODE_V2 => _IOW<input_keymap_entry>('E', 0x04);

        public static uint EVIOCGNAME(uint len)
        {
            return _IOC(_IOC_READ, 'E', 0x06, len) /* get device name */;
        }

        public static uint EVIOCGPHYS(uint len)
        {
            return _IOC(_IOC_READ, 'E', 0x07, len) /* get physical location */;
        }

        public static uint EVIOCGUNIQ(uint len)
        {
            return _IOC(_IOC_READ, 'E', 0x08, len) /* get unique identifier */;
        }

        public static uint EVIOCGPROP(uint len)
        {
            return _IOC(_IOC_READ, 'E', 0x09, len) /* get device properties */;
        }

        /**
         * EVIOCGMTSLOTS(len) - get MT slot values
         * @len: size of the data buffer in bytes
         * 
         * The ioctl buffer argument should be binary equivalent to
         * 
         * struct input_mt_request_layout {
         * uint code;
         * __s32 values[num_slots];
         * };
         * 
         * where num_slots is the (arbitrary) number of MT slots to extract.
         * 
         * The ioctl size argument (len) is the size of the buffer, which
         * should satisfy len = (num_slots + 1) * sizeof(__s32).  If len is
         * too small to fit all available slots, the first num_slots are
         * returned.
         * 
         * Before the call, code is set to the wanted ABS_MT event type. On
         * return, values[] is filled with the slot values for the specified
         * ABS_MT code.
         * 
         * If the request code is not an ABS_MT value, -EINVAL is returned.
         */
        public static uint EVIOCGMTSLOTS(uint len)
        {
            return _IOC(_IOC_READ, 'E', 0x0a, len);
        }

        public static uint EVIOCGKEY(uint len)
        {
            return _IOC(_IOC_READ, 'E', 0x18, len) /* get global key state */;
        }

        public static uint EVIOCGLED(uint len)
        {
            return _IOC(_IOC_READ, 'E', 0x19, len) /* get all LEDs */;
        }

        public static uint EVIOCGSND(uint len)
        {
            return _IOC(_IOC_READ, 'E', 0x1a, len) /* get all sounds status */;
        }

        public static uint EVIOCGSW(uint len)
        {
            return _IOC(_IOC_READ, 'E', 0x1b, len) /* get all switch states */;
        }

        public static uint EVIOCGBIT(uint ev, uint len)
        {
            return _IOC(_IOC_READ, 'E', 0x20 + ev, len) /* get event bits */;
        }

        public static uint EVIOCGABS(uint abs)
        {
            return _IOR<input_absinfo>('E', 0x40 + abs) /* get abs value/limits */;
        }

        public static uint EVIOCSABS(uint abs)
        {
            return _IOW<input_absinfo>('E', 0xc0 + abs) /* set abs value/limits */;
        }

        public static uint EVIOCSFF()
        {
            return _IOW<ff_effect>('E', 0x80) /* send a force effect to a force feedback device */;
        }

        public static uint EVIOCRMFF()
        {
            return _IOW<int>('E', 0x81) /* Erase a force effect */;
        }

        public static uint EVIOCGEFFECTS()
        {
            return _IOR<int>('E', 0x84) /* Report number of effects playable at the same time */;
        }

        public static uint EVIOCGRAB()
        {
            return _IOW<int>('E', 0x90) /* Grab/Release device */;
        }

        public static uint EVIOCREVOKE()
        {
            return _IOW<int>('E', 0x91) /* Revoke device access */;
        }

        /**
         * EVIOCGMASK - Retrieve current event mask
         * 
         * This ioctl allows user to retrieve the current event mask for specific
         * event type. The argument must be of type "struct input_mask" and
         * specifies the event type to query, the address of the receive buffer and
         * the size of the receive buffer.
         * 
         * The event mask is a per-client mask that specifies which events are
         * forwarded to the client. Each event code is represented by a single bit
         * in the event mask. If the bit is set, the event is passed to the client
         * normally. Otherwise, the event is filtered and will never be queued on
         * the client's receive buffer.
         * 
         * Event masks do not affect global state of the input device. They only
         * affect the file descriptor they are applied to.
         * 
         * The default event mask for a client has all bits set, i.e. all events
         * are forwarded to the client. If the kernel is queried for an unknown
         * event type or if the receive buffer is larger than the number of
         * event codes known to the kernel, the kernel returns all zeroes for those
         * codes.
         * 
         * At maximum, codes_size bytes are copied.
         * 
         * This ioctl may fail with ENODEV in case the file is revoked, EFAULT
         * if the receive-buffer points to invalid memory, or EINVAL if the kernel
         * does not implement the ioctl.
         */
        public static uint EVIOCGMASK()
        {
            return _IOR<input_mask>('E', 0x92);
        }

        /**
         * EVIOCSMASK - Set event mask
         * 
         * This ioctl is the counterpart to EVIOCGMASK. Instead of receiving the
         * current event mask, this changes the client's event mask for a specific
         * type.  See EVIOCGMASK for a description of event-masks and the
         * argument-type.
         * 
         * This ioctl provides full forward compatibility. If the passed event type
         * is unknown to the kernel, or if the number of event codes specified in
         * the mask is bigger than what is known to the kernel, the ioctl is still
         * accepted and applied. However, any unknown codes are left untouched and
         * stay cleared. That means, the kernel always filters unknown codes
         * regardless of what the client requests.  If the new mask doesn't cover
         * all known event-codes, all remaining codes are automatically cleared and
         * thus filtered.
         * 
         * This ioctl may fail with ENODEV in case the file is revoked. EFAULT is
         * returned if the receive-buffer points to invalid memory. EINVAL is returned
         * if the kernel does not implement the ioctl.
         */
        public static uint EVIOCSMASK()
        {
            return _IOW<input_mask>('E', 0x93) /* Set event-masks */;
        }

        public static uint EVIOCSCLOCKID()
        {
            return _IOW<int>('E', 0xa0) /* Set clockid to be used for timestamps */;
        }

        /*
         * The event structure itself
         */
        public struct input_event
        {
            public time_t input_event_sec;
            public time_t input_event_usec;
            public ushort type;
            public ushort code;
            public int value;
        }

        /*
         * IOCTLs (0x00 - 0x7f)
         */
        public struct input_id
        {
            public ushort bustype;
            public ushort vendor;
            public ushort product;
            public ushort version;
        }

        /**
         * struct input_absinfo - used by EVIOCGABS/EVIOCSABS ioctls
         * @value: latest reported value for the axis.
         * @minimum: specifies minimum value for the axis.
         * @maximum: specifies maximum value for the axis.
         * @fuzz: specifies fuzz value that is used to filter noise from
         * the event stream.
         * @flat: values that are within this value will be discarded by
         * joydev interface and reported as 0 instead.
         * @resolution: specifies resolution for the values reported for
         * the axis.
         * 
         * Note that input core does not clamp reported values to the
         * [minimum, maximum] limits, such task is left to userspace.
         * 
         * The default resolution for main axes (ABS_X, ABS_Y, ABS_Z)
         * is reported in units per millimeter (units/mm), resolution
         * for rotational axes (ABS_RX, ABS_RY, ABS_RZ) is reported
         * in units per radian.
         * When INPUT_PROP_ACCELEROMETER is set the resolution changes.
         * The main axes (ABS_X, ABS_Y, ABS_Z) are then reported in
         * units per g (units/g) and in units per degree per second
         * (units/deg/s) for rotational axes (ABS_RX, ABS_RY, ABS_RZ).
         */
        public struct input_absinfo
        {
            public int value;
            public int minimum;
            public int maximum;
            public int fuzz;
            public int flat;
            public int resolution;
        }

        /**
         * struct input_keymap_entry - used by EVIOCGKEYCODE/EVIOCSKEYCODE ioctls
         * @scancode: scancode represented in machine-endian form.
         * @len: length of the scancode that resides in @scancode buffer.
         * @index: index in the keymap, may be used instead of scancode
         * @flags: allows to specify how kernel should handle the request. For
         * example, setting INPUT_KEYMAP_BY_INDEX flag indicates that kernel
         * should perform lookup in keymap by @index instead of @scancode
         * @keycode: key code assigned to this scancode
         * 
         * The structure is used to retrieve and modify keymap data. Users have
         * option of performing lookup either by @scancode itself or by @index
         * in keymap entry. EVIOCGKEYCODE will also return scancode or index
         * (depending on which element was used to perform lookup).
         */
        public struct input_keymap_entry
        {
            public byte flags;
            public byte len;
            public ushort index;
            public uint keycode;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] scancode;
        }

        public struct input_mask
        {
            public uint type;
            public uint codes_size;
            public ulong codes_ptr;
        }

        public struct helper_repeat_settings
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public uint data;
        }

        public struct helper_keycode
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public uint data;
        }

        /*
 * Structures used in ioctls to upload effects to a device
 * They are pieces of a bigger structure (called ff_effect)
 */

/*
 * All duration values are expressed in ms. Values above 32767 ms (0x7fff)
 * should not be used and have unspecified results.
 */

/**
         * struct ff_replay - defines scheduling of the force-feedback effect
         * @length: duration of the effect
         * @delay: delay before effect should start playing
         */
public struct ff_replay
        {
            public ushort length;
            public ushort delay;
        }

/**
         * struct ff_trigger - defines what triggers the force-feedback effect
         * @button: number of the button triggering the effect
         * @interval: controls how soon the effect can be re-triggered
         */
public struct ff_trigger
        {
            public ushort button;
            public ushort interval;
        }

/**
         * struct ff_envelope - generic force-feedback effect envelope
         * @attack_length: duration of the attack (ms)
         * @attack_level: level at the beginning of the attack
         * @fade_length: duration of fade (ms)
         * @fade_level: level at the end of fade
         * 
         * The @attack_level and @fade_level are absolute values; when applying
         * envelope force-feedback core will convert to positive/negative
         * value based on polarity of the default level of the effect.
         * Valid range for the attack and fade levels is 0x0000 - 0x7fff
         */
public struct ff_envelope
        {
            public ushort attack_length;
            public ushort attack_level;
            public ushort fade_length;
            public ushort fade_level;
        }

/**
         * struct ff_constant_effect - defines parameters of a constant force-feedback effect
         * @level: strength of the effect; may be negative
         * @envelope: envelope data
         */
public struct ff_constant_effect
        {
            public short level;
            public ff_envelope envelope;
        }

/**
         * struct ff_ramp_effect - defines parameters of a ramp force-feedback effect
         * @start_level: beginning strength of the effect; may be negative
         * @end_level: final strength of the effect; may be negative
         * @envelope: envelope data
         */
public struct ff_ramp_effect
        {
            public short start_level;
            public short end_level;
            public ff_envelope envelope;
        }

/**
         * struct ff_condition_effect - defines a spring or friction force-feedback effect
         * @right_saturation: maximum level when joystick moved all way to the right
         * @left_saturation: same for the left side
         * @right_coeff: controls how fast the force grows when the joystick moves
         * to the right
         * @left_coeff: same for the left side
         * @deadband: size of the dead zone, where no force is produced
         * @center: position of the dead zone
         */
public struct ff_condition_effect
        {
            public ushort right_saturation;
            public ushort left_saturation;

            public short right_coeff;
            public short left_coeff;

            public ushort deadband;
            public short center;
        }

/**
         * struct ff_periodic_effect - defines parameters of a periodic force-feedback effect
         * @waveform: kind of the effect (wave)
         * @period: period of the wave (ms)
         * @magnitude: peak value
         * @offset: mean value of the wave (roughly)
         * @phase: 'horizontal' shift
         * @envelope: envelope data
         * @custom_len: number of samples (FF_CUSTOM only)
         * @custom_data: buffer of samples (FF_CUSTOM only)
         * 
         * Known waveforms - FF_SQUARE, FF_TRIANGLE, FF_SINE, FF_SAW_UP,
         * FF_SAW_DOWN, FF_CUSTOM. The exact syntax FF_CUSTOM is undefined
         * for the time being as no driver supports it yet.
         * 
         * Note: the data pointed by custom_data is copied by the driver.
         * You can therefore dispose of the memory after the upload/update.
         */
public struct ff_periodic_effect
        {
            public ushort waveform;
            public ushort period;
            public short magnitude;
            public short offset;
            public ushort phase;

            public ff_envelope envelope;

            public uint custom_len;

            [MarshalAs(UnmanagedType.ByValArray)] public short[] custom_data;
        }

/**
         * struct ff_rumble_effect - defines parameters of a periodic force-feedback effect
         * @strong_magnitude: magnitude of the heavy motor
         * @weak_magnitude: magnitude of the light one
         * 
         * Some rumble pads have two motors of different weight. Strong_magnitude
         * represents the magnitude of the vibration generated by the heavy one.
         */
public struct ff_rumble_effect
        {
            public ushort strong_magnitude;
            public ushort weak_magnitude;
        }

/**
         * struct ff_effect - defines force feedback effect
         * @type: type of the effect (FF_CONSTANT, FF_PERIODIC, FF_RAMP, FF_SPRING,
         * FF_FRICTION, FF_DAMPER, FF_RUMBLE, FF_INERTIA, or FF_CUSTOM)
         * @id: an unique id assigned to an effect
         * @direction: direction of the effect
         * @trigger: trigger conditions (struct ff_trigger)
         * @replay: scheduling of the effect (struct ff_replay)
         * @u: effect-specific structure (one of ff_constant_effect, ff_ramp_effect,
         * ff_periodic_effect, ff_condition_effect, ff_rumble_effect) further
         * defining effect parameters
         * 
         * This structure is sent through ioctl from the application to the driver.
         * To create a new effect application should set its @id to -1; the kernel
         * will return assigned @id which can later be used to update or delete
         * this effect.
         * 
         * Direction of the effect is encoded as follows:
         * 0 deg -> 0x0000 (down)
         * 90 deg -> 0x4000 (left)
         * 180 deg -> 0x8000 (up)
         * 270 deg -> 0xC000 (right)
         */
public struct ff_effect
        {
            public ushort type;
            public short id;
            public ushort direction;
            public ff_trigger trigger;
            public ff_replay replay;

            [StructLayout(LayoutKind.Explicit)]
            public struct _anon
            {
                [FieldOffset(0)] public ff_constant_effect constant;

                [FieldOffset(0)] public ff_ramp_effect ramp;

                [FieldOffset(0)] public ff_periodic_effect periodic;

                [FieldOffset(0)] [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
                public ff_condition_effect[] condition; /* One for each axis */

                [FieldOffset(0)] public ff_rumble_effect rumble;
            }

            public _anon a;
        }
    }
}
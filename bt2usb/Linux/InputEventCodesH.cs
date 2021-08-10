// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace bt2usb.Linux
{
    public static class InputEventCodesH
    {
        /*
         * Device properties and quirks
         */

        public const uint INPUT_PROP_POINTER = 0x00; /* needs a pointer */
        public const uint INPUT_PROP_DIRECT = 0x01; /* direct input devices */
        public const uint INPUT_PROP_BUTTONPAD = 0x02; /* has button(s) under pad */
        public const uint INPUT_PROP_SEMI_MT = 0x03; /* touch rectangle only */
        public const uint INPUT_PROP_TOPBUTTONPAD = 0x04; /* softbuttons at top of pad */
        public const uint INPUT_PROP_POINTING_STICK = 0x05; /* is a pointing stick */
        public const uint INPUT_PROP_ACCELEROMETER = 0x06; /* has accelerometer */

        public const uint INPUT_PROP_MAX = 0x1f;
        public const uint INPUT_PROP_CNT = INPUT_PROP_MAX + 1;

        /*
         * Event types
         */

        public const uint EV_SYN = 0x00;
        public const uint EV_KEY = 0x01;
        public const uint EV_REL = 0x02;
        public const uint EV_ABS = 0x03;
        public const uint EV_MSC = 0x04;
        public const uint EV_SW = 0x05;
        public const uint EV_LED = 0x11;
        public const uint EV_SND = 0x12;
        public const uint EV_REP = 0x14;
        public const uint EV_FF = 0x15;
        public const uint EV_PWR = 0x16;
        public const uint EV_FF_STATUS = 0x17;
        public const uint EV_MAX = 0x1f;
        public const uint EV_CNT = EV_MAX + 1;

        /*
         * Synchronization events.
         */

        public const uint SYN_REPORT = 0;
        public const uint SYN_CONFIG = 1;
        public const uint SYN_MT_REPORT = 2;
        public const uint SYN_DROPPED = 3;
        public const uint SYN_MAX = 0xf;
        public const uint SYN_CNT = SYN_MAX + 1;

        /*
         * Keys and buttons
         *
         * Most of the keys/buttons are modeled after USB HUT 1.12
         * (see http://www.usb.org/developers/hidpage).
         * Abbreviations in the comments:
         * AC - Application Control
         * AL - Application Launch Button
         * SC - System Control
         */

        public const uint KEY_RESERVED = 0;
        public const uint KEY_ESC = 1;
        public const uint KEY_1 = 2;
        public const uint KEY_2 = 3;
        public const uint KEY_3 = 4;
        public const uint KEY_4 = 5;
        public const uint KEY_5 = 6;
        public const uint KEY_6 = 7;
        public const uint KEY_7 = 8;
        public const uint KEY_8 = 9;
        public const uint KEY_9 = 10;
        public const uint KEY_0 = 11;
        public const uint KEY_MINUS = 12;
        public const uint KEY_EQUAL = 13;
        public const uint KEY_BACKSPACE = 14;
        public const uint KEY_TAB = 15;
        public const uint KEY_Q = 16;
        public const uint KEY_W = 17;
        public const uint KEY_E = 18;
        public const uint KEY_R = 19;
        public const uint KEY_T = 20;
        public const uint KEY_Y = 21;
        public const uint KEY_U = 22;
        public const uint KEY_I = 23;
        public const uint KEY_O = 24;
        public const uint KEY_P = 25;
        public const uint KEY_LEFTBRACE = 26;
        public const uint KEY_RIGHTBRACE = 27;
        public const uint KEY_ENTER = 28;
        public const uint KEY_LEFTCTRL = 29;
        public const uint KEY_A = 30;
        public const uint KEY_S = 31;
        public const uint KEY_D = 32;
        public const uint KEY_F = 33;
        public const uint KEY_G = 34;
        public const uint KEY_H = 35;
        public const uint KEY_J = 36;
        public const uint KEY_K = 37;
        public const uint KEY_L = 38;
        public const uint KEY_SEMICOLON = 39;
        public const uint KEY_APOSTROPHE = 40;
        public const uint KEY_GRAVE = 41;
        public const uint KEY_LEFTSHIFT = 42;
        public const uint KEY_BACKSLASH = 43;
        public const uint KEY_Z = 44;
        public const uint KEY_X = 45;
        public const uint KEY_C = 46;
        public const uint KEY_V = 47;
        public const uint KEY_B = 48;
        public const uint KEY_N = 49;
        public const uint KEY_M = 50;
        public const uint KEY_COMMA = 51;
        public const uint KEY_DOT = 52;
        public const uint KEY_SLASH = 53;
        public const uint KEY_RIGHTSHIFT = 54;
        public const uint KEY_KPASTERISK = 55;
        public const uint KEY_LEFTALT = 56;
        public const uint KEY_SPACE = 57;
        public const uint KEY_CAPSLOCK = 58;
        public const uint KEY_F1 = 59;
        public const uint KEY_F2 = 60;
        public const uint KEY_F3 = 61;
        public const uint KEY_F4 = 62;
        public const uint KEY_F5 = 63;
        public const uint KEY_F6 = 64;
        public const uint KEY_F7 = 65;
        public const uint KEY_F8 = 66;
        public const uint KEY_F9 = 67;
        public const uint KEY_F10 = 68;
        public const uint KEY_NUMLOCK = 69;
        public const uint KEY_SCROLLLOCK = 70;
        public const uint KEY_KP7 = 71;
        public const uint KEY_KP8 = 72;
        public const uint KEY_KP9 = 73;
        public const uint KEY_KPMINUS = 74;
        public const uint KEY_KP4 = 75;
        public const uint KEY_KP5 = 76;
        public const uint KEY_KP6 = 77;
        public const uint KEY_KPPLUS = 78;
        public const uint KEY_KP1 = 79;
        public const uint KEY_KP2 = 80;
        public const uint KEY_KP3 = 81;
        public const uint KEY_KP0 = 82;
        public const uint KEY_KPDOT = 83;

        public const uint KEY_ZENKAKUHANKAKU = 85;
        public const uint KEY_102ND = 86;
        public const uint KEY_F11 = 87;
        public const uint KEY_F12 = 88;
        public const uint KEY_RO = 89;
        public const uint KEY_KATAKANA = 90;
        public const uint KEY_HIRAGANA = 91;
        public const uint KEY_HENKAN = 92;
        public const uint KEY_KATAKANAHIRAGANA = 93;
        public const uint KEY_MUHENKAN = 94;
        public const uint KEY_KPJPCOMMA = 95;
        public const uint KEY_KPENTER = 96;
        public const uint KEY_RIGHTCTRL = 97;
        public const uint KEY_KPSLASH = 98;
        public const uint KEY_SYSRQ = 99;
        public const uint KEY_RIGHTALT = 100;
        public const uint KEY_LINEFEED = 101;
        public const uint KEY_HOME = 102;
        public const uint KEY_UP = 103;
        public const uint KEY_PAGEUP = 104;
        public const uint KEY_LEFT = 105;
        public const uint KEY_RIGHT = 106;
        public const uint KEY_END = 107;
        public const uint KEY_DOWN = 108;
        public const uint KEY_PAGEDOWN = 109;
        public const uint KEY_INSERT = 110;
        public const uint KEY_DELETE = 111;
        public const uint KEY_MACRO = 112;
        public const uint KEY_MUTE = 113;
        public const uint KEY_VOLUMEDOWN = 114;
        public const uint KEY_VOLUMEUP = 115;
        public const uint KEY_POWER = 116; /* SC System Power Down */
        public const uint KEY_KPEQUAL = 117;
        public const uint KEY_KPPLUSMINUS = 118;
        public const uint KEY_PAUSE = 119;
        public const uint KEY_SCALE = 120; /* AL Compiz Scale (Expose) */

        public const uint KEY_KPCOMMA = 121;
        public const uint KEY_HANGEUL = 122;
        public const uint KEY_HANGUEL = KEY_HANGEUL;
        public const uint KEY_HANJA = 123;
        public const uint KEY_YEN = 124;
        public const uint KEY_LEFTMETA = 125;
        public const uint KEY_RIGHTMETA = 126;
        public const uint KEY_COMPOSE = 127;

        public const uint KEY_STOP = 128; /* AC Stop */
        public const uint KEY_AGAIN = 129;
        public const uint KEY_PROPS = 130; /* AC Properties */
        public const uint KEY_UNDO = 131; /* AC Undo */
        public const uint KEY_FRONT = 132;
        public const uint KEY_COPY = 133; /* AC Copy */
        public const uint KEY_OPEN = 134; /* AC Open */
        public const uint KEY_PASTE = 135; /* AC Paste */
        public const uint KEY_FIND = 136; /* AC Search */
        public const uint KEY_CUT = 137; /* AC Cut */
        public const uint KEY_HELP = 138; /* AL Integrated Help Center */
        public const uint KEY_MENU = 139; /* Menu (show menu) */
        public const uint KEY_CALC = 140; /* AL Calculator */
        public const uint KEY_SETUP = 141;
        public const uint KEY_SLEEP = 142; /* SC System Sleep */
        public const uint KEY_WAKEUP = 143; /* System Wake Up */
        public const uint KEY_FILE = 144; /* AL Local Machine Browser */
        public const uint KEY_SENDFILE = 145;
        public const uint KEY_DELETEFILE = 146;
        public const uint KEY_XFER = 147;
        public const uint KEY_PROG1 = 148;
        public const uint KEY_PROG2 = 149;
        public const uint KEY_WWW = 150; /* AL Internet Browser */
        public const uint KEY_MSDOS = 151;
        public const uint KEY_COFFEE = 152; /* AL Terminal Lock/Screensaver */
        public const uint KEY_SCREENLOCK = KEY_COFFEE;
        public const uint KEY_ROTATE_DISPLAY = 153; /* Display orientation for e.g. tablets */
        public const uint KEY_DIRECTION = KEY_ROTATE_DISPLAY;
        public const uint KEY_CYCLEWINDOWS = 154;
        public const uint KEY_MAIL = 155;
        public const uint KEY_BOOKMARKS = 156; /* AC Bookmarks */
        public const uint KEY_COMPUTER = 157;
        public const uint KEY_BACK = 158; /* AC Back */
        public const uint KEY_FORWARD = 159; /* AC Forward */
        public const uint KEY_CLOSECD = 160;
        public const uint KEY_EJECTCD = 161;
        public const uint KEY_EJECTCLOSECD = 162;
        public const uint KEY_NEXTSONG = 163;
        public const uint KEY_PLAYPAUSE = 164;
        public const uint KEY_PREVIOUSSONG = 165;
        public const uint KEY_STOPCD = 166;
        public const uint KEY_RECORD = 167;
        public const uint KEY_REWIND = 168;
        public const uint KEY_PHONE = 169; /* Media Select Telephone */
        public const uint KEY_ISO = 170;
        public const uint KEY_CONFIG = 171; /* AL Consumer Control Configuration */
        public const uint KEY_HOMEPAGE = 172; /* AC Home */
        public const uint KEY_REFRESH = 173; /* AC Refresh */
        public const uint KEY_EXIT = 174; /* AC Exit */
        public const uint KEY_MOVE = 175;
        public const uint KEY_EDIT = 176;
        public const uint KEY_SCROLLUP = 177;
        public const uint KEY_SCROLLDOWN = 178;
        public const uint KEY_KPLEFTPAREN = 179;
        public const uint KEY_KPRIGHTPAREN = 180;
        public const uint KEY_NEW = 181; /* AC New */
        public const uint KEY_REDO = 182; /* AC Redo/Repeat */

        public const uint KEY_F13 = 183;
        public const uint KEY_F14 = 184;
        public const uint KEY_F15 = 185;
        public const uint KEY_F16 = 186;
        public const uint KEY_F17 = 187;
        public const uint KEY_F18 = 188;
        public const uint KEY_F19 = 189;
        public const uint KEY_F20 = 190;
        public const uint KEY_F21 = 191;
        public const uint KEY_F22 = 192;
        public const uint KEY_F23 = 193;
        public const uint KEY_F24 = 194;

        public const uint KEY_PLAYCD = 200;
        public const uint KEY_PAUSECD = 201;
        public const uint KEY_PROG3 = 202;
        public const uint KEY_PROG4 = 203;
        public const uint KEY_DASHBOARD = 204; /* AL Dashboard */
        public const uint KEY_SUSPEND = 205;
        public const uint KEY_CLOSE = 206; /* AC Close */
        public const uint KEY_PLAY = 207;
        public const uint KEY_FASTFORWARD = 208;
        public const uint KEY_BASSBOOST = 209;
        public const uint KEY_PRINT = 210; /* AC Print */
        public const uint KEY_HP = 211;
        public const uint KEY_CAMERA = 212;
        public const uint KEY_SOUND = 213;
        public const uint KEY_QUESTION = 214;
        public const uint KEY_EMAIL = 215;
        public const uint KEY_CHAT = 216;
        public const uint KEY_SEARCH = 217;
        public const uint KEY_CONNECT = 218;
        public const uint KEY_FINANCE = 219; /* AL Checkbook/Finance */
        public const uint KEY_SPORT = 220;
        public const uint KEY_SHOP = 221;
        public const uint KEY_ALTERASE = 222;
        public const uint KEY_CANCEL = 223; /* AC Cancel */
        public const uint KEY_BRIGHTNESSDOWN = 224;
        public const uint KEY_BRIGHTNESSUP = 225;
        public const uint KEY_MEDIA = 226;

        public const uint
            KEY_SWITCHVIDEOMODE = 227; /* Cycle between available video outputs (Monitor/LCD/TV-out/etc) */

        public const uint KEY_KBDILLUMTOGGLE = 228;
        public const uint KEY_KBDILLUMDOWN = 229;
        public const uint KEY_KBDILLUMUP = 230;

        public const uint KEY_SEND = 231; /* AC Send */
        public const uint KEY_REPLY = 232; /* AC Reply */
        public const uint KEY_FORWARDMAIL = 233; /* AC Forward Msg */
        public const uint KEY_SAVE = 234; /* AC Save */
        public const uint KEY_DOCUMENTS = 235;

        public const uint KEY_BATTERY = 236;

        public const uint KEY_BLUETOOTH = 237;
        public const uint KEY_WLAN = 238;
        public const uint KEY_UWB = 239;

        public const uint KEY_UNKNOWN = 240;

        public const uint KEY_VIDEO_NEXT = 241; /* drive next video source */
        public const uint KEY_VIDEO_PREV = 242; /* drive previous video source */
        public const uint KEY_BRIGHTNESS_CYCLE = 243; /* brightness up, after max is min */

        public const uint KEY_BRIGHTNESS_AUTO = 244;
        /* Set Auto Brightness: manual
           brightness control is off,
           rely on ambient */

        public const uint KEY_BRIGHTNESS_ZERO = KEY_BRIGHTNESS_AUTO;
        public const uint KEY_DISPLAY_OFF = 245; /* display device to off state */

        public const uint KEY_WWAN = 246; /* Wireless WAN (LTE, UMTS, GSM, etc.) */
        public const uint KEY_WIMAX = KEY_WWAN;
        public const uint KEY_RFKILL = 247; /* Key that controls all radios */

        public const uint KEY_MICMUTE = 248; /* Mute / unmute the microphone */

        /* Code 255 is reserved for special needs of AT keyboard driver */

        public const uint BTN_MISC = 0x100;
        public const uint BTN_0 = 0x100;
        public const uint BTN_1 = 0x101;
        public const uint BTN_2 = 0x102;
        public const uint BTN_3 = 0x103;
        public const uint BTN_4 = 0x104;
        public const uint BTN_5 = 0x105;
        public const uint BTN_6 = 0x106;
        public const uint BTN_7 = 0x107;
        public const uint BTN_8 = 0x108;
        public const uint BTN_9 = 0x109;

        public const uint BTN_MOUSE = 0x110;
        public const uint BTN_LEFT = 0x110;
        public const uint BTN_RIGHT = 0x111;
        public const uint BTN_MIDDLE = 0x112;
        public const uint BTN_SIDE = 0x113;
        public const uint BTN_EXTRA = 0x114;
        public const uint BTN_FORWARD = 0x115;
        public const uint BTN_BACK = 0x116;
        public const uint BTN_TASK = 0x117;

        public const uint BTN_JOYSTICK = 0x120;
        public const uint BTN_TRIGGER = 0x120;
        public const uint BTN_THUMB = 0x121;
        public const uint BTN_THUMB2 = 0x122;
        public const uint BTN_TOP = 0x123;
        public const uint BTN_TOP2 = 0x124;
        public const uint BTN_PINKIE = 0x125;
        public const uint BTN_BASE = 0x126;
        public const uint BTN_BASE2 = 0x127;
        public const uint BTN_BASE3 = 0x128;
        public const uint BTN_BASE4 = 0x129;
        public const uint BTN_BASE5 = 0x12a;
        public const uint BTN_BASE6 = 0x12b;
        public const uint BTN_DEAD = 0x12f;

        public const uint BTN_GAMEPAD = 0x130;
        public const uint BTN_SOUTH = 0x130;
        public const uint BTN_A = BTN_SOUTH;
        public const uint BTN_EAST = 0x131;
        public const uint BTN_B = BTN_EAST;
        public const uint BTN_C = 0x132;
        public const uint BTN_NORTH = 0x133;
        public const uint BTN_X = BTN_NORTH;
        public const uint BTN_WEST = 0x134;
        public const uint BTN_Y = BTN_WEST;
        public const uint BTN_Z = 0x135;
        public const uint BTN_TL = 0x136;
        public const uint BTN_TR = 0x137;
        public const uint BTN_TL2 = 0x138;
        public const uint BTN_TR2 = 0x139;
        public const uint BTN_SELECT = 0x13a;
        public const uint BTN_START = 0x13b;
        public const uint BTN_MODE = 0x13c;
        public const uint BTN_THUMBL = 0x13d;
        public const uint BTN_THUMBR = 0x13e;

        public const uint BTN_DIGI = 0x140;
        public const uint BTN_TOOL_PEN = 0x140;
        public const uint BTN_TOOL_RUBBER = 0x141;
        public const uint BTN_TOOL_BRUSH = 0x142;
        public const uint BTN_TOOL_PENCIL = 0x143;
        public const uint BTN_TOOL_AIRBRUSH = 0x144;
        public const uint BTN_TOOL_FINGER = 0x145;
        public const uint BTN_TOOL_MOUSE = 0x146;
        public const uint BTN_TOOL_LENS = 0x147;
        public const uint BTN_TOOL_QUINTTAP = 0x148; /* Five fingers on trackpad */
        public const uint BTN_STYLUS3 = 0x149;
        public const uint BTN_TOUCH = 0x14a;
        public const uint BTN_STYLUS = 0x14b;
        public const uint BTN_STYLUS2 = 0x14c;
        public const uint BTN_TOOL_DOUBLETAP = 0x14d;
        public const uint BTN_TOOL_TRIPLETAP = 0x14e;
        public const uint BTN_TOOL_QUADTAP = 0x14f; /* Four fingers on trackpad */

        public const uint BTN_WHEEL = 0x150;
        public const uint BTN_GEAR_DOWN = 0x150;
        public const uint BTN_GEAR_UP = 0x151;

        public const uint KEY_OK = 0x160;
        public const uint KEY_SELECT = 0x161;
        public const uint KEY_GOTO = 0x162;
        public const uint KEY_CLEAR = 0x163;
        public const uint KEY_POWER2 = 0x164;
        public const uint KEY_OPTION = 0x165;
        public const uint KEY_INFO = 0x166; /* AL OEM Features/Tips/Tutorial */
        public const uint KEY_TIME = 0x167;
        public const uint KEY_VENDOR = 0x168;
        public const uint KEY_ARCHIVE = 0x169;
        public const uint KEY_PROGRAM = 0x16a; /* Media Select Program Guide */
        public const uint KEY_CHANNEL = 0x16b;
        public const uint KEY_FAVORITES = 0x16c;
        public const uint KEY_EPG = 0x16d;
        public const uint KEY_PVR = 0x16e; /* Media Select Home */
        public const uint KEY_MHP = 0x16f;
        public const uint KEY_LANGUAGE = 0x170;
        public const uint KEY_TITLE = 0x171;
        public const uint KEY_SUBTITLE = 0x172;
        public const uint KEY_ANGLE = 0x173;
        public const uint KEY_FULL_SCREEN = 0x174; /* AC View Toggle */
        public const uint KEY_ZOOM = KEY_FULL_SCREEN;
        public const uint KEY_MODE = 0x175;
        public const uint KEY_KEYBOARD = 0x176;
        public const uint KEY_ASPECT_RATIO = 0x177; /* HUTRR37: Aspect */
        public const uint KEY_SCREEN = KEY_ASPECT_RATIO;
        public const uint KEY_PC = 0x178; /* Media Select Computer */
        public const uint KEY_TV = 0x179; /* Media Select TV */
        public const uint KEY_TV2 = 0x17a; /* Media Select Cable */
        public const uint KEY_VCR = 0x17b; /* Media Select VCR */
        public const uint KEY_VCR2 = 0x17c; /* VCR Plus */
        public const uint KEY_SAT = 0x17d; /* Media Select Satellite */
        public const uint KEY_SAT2 = 0x17e;
        public const uint KEY_CD = 0x17f; /* Media Select CD */
        public const uint KEY_TAPE = 0x180; /* Media Select Tape */
        public const uint KEY_RADIO = 0x181;
        public const uint KEY_TUNER = 0x182; /* Media Select Tuner */
        public const uint KEY_PLAYER = 0x183;
        public const uint KEY_TEXT = 0x184;
        public const uint KEY_DVD = 0x185; /* Media Select DVD */
        public const uint KEY_AUX = 0x186;
        public const uint KEY_MP3 = 0x187;
        public const uint KEY_AUDIO = 0x188; /* AL Audio Browser */
        public const uint KEY_VIDEO = 0x189; /* AL Movie Browser */
        public const uint KEY_DIRECTORY = 0x18a;
        public const uint KEY_LIST = 0x18b;
        public const uint KEY_MEMO = 0x18c; /* Media Select Messages */
        public const uint KEY_CALENDAR = 0x18d;
        public const uint KEY_RED = 0x18e;
        public const uint KEY_GREEN = 0x18f;
        public const uint KEY_YELLOW = 0x190;
        public const uint KEY_BLUE = 0x191;
        public const uint KEY_CHANNELUP = 0x192; /* Channel Increment */
        public const uint KEY_CHANNELDOWN = 0x193; /* Channel Decrement */
        public const uint KEY_FIRST = 0x194;
        public const uint KEY_LAST = 0x195; /* Recall Last */
        public const uint KEY_AB = 0x196;
        public const uint KEY_NEXT = 0x197;
        public const uint KEY_RESTART = 0x198;
        public const uint KEY_SLOW = 0x199;
        public const uint KEY_SHUFFLE = 0x19a;
        public const uint KEY_BREAK = 0x19b;
        public const uint KEY_PREVIOUS = 0x19c;
        public const uint KEY_DIGITS = 0x19d;
        public const uint KEY_TEEN = 0x19e;
        public const uint KEY_TWEN = 0x19f;
        public const uint KEY_VIDEOPHONE = 0x1a0; /* Media Select Video Phone */
        public const uint KEY_GAMES = 0x1a1; /* Media Select Games */
        public const uint KEY_ZOOMIN = 0x1a2; /* AC Zoom In */
        public const uint KEY_ZOOMOUT = 0x1a3; /* AC Zoom Out */
        public const uint KEY_ZOOMRESET = 0x1a4; /* AC Zoom */
        public const uint KEY_WORDPROCESSOR = 0x1a5; /* AL Word Processor */
        public const uint KEY_EDITOR = 0x1a6; /* AL Text Editor */
        public const uint KEY_SPREADSHEET = 0x1a7; /* AL Spreadsheet */
        public const uint KEY_GRAPHICSEDITOR = 0x1a8; /* AL Graphics Editor */
        public const uint KEY_PRESENTATION = 0x1a9; /* AL Presentation App */
        public const uint KEY_DATABASE = 0x1aa; /* AL Database App */
        public const uint KEY_NEWS = 0x1ab; /* AL Newsreader */
        public const uint KEY_VOICEMAIL = 0x1ac; /* AL Voicemail */
        public const uint KEY_ADDRESSBOOK = 0x1ad; /* AL Contacts/Address Book */
        public const uint KEY_MESSENGER = 0x1ae; /* AL Instant Messaging */
        public const uint KEY_DISPLAYTOGGLE = 0x1af; /* Turn display (LCD) on and off */
        public const uint KEY_BRIGHTNESS_TOGGLE = KEY_DISPLAYTOGGLE;
        public const uint KEY_SPELLCHECK = 0x1b0; /* AL Spell Check */
        public const uint KEY_LOGOFF = 0x1b1; /* AL Logoff */

        public const uint KEY_DOLLAR = 0x1b2;
        public const uint KEY_EURO = 0x1b3;

        public const uint KEY_FRAMEBACK = 0x1b4; /* Consumer - transport controls */
        public const uint KEY_FRAMEFORWARD = 0x1b5;
        public const uint KEY_CONTEXT_MENU = 0x1b6; /* GenDesc - system context menu */
        public const uint KEY_MEDIA_REPEAT = 0x1b7; /* Consumer - transport control */
        public const uint KEY_10CHANNELSUP = 0x1b8; /* 10 channels up (10+) */
        public const uint KEY_10CHANNELSDOWN = 0x1b9; /* 10 channels down (10-) */
        public const uint KEY_IMAGES = 0x1ba; /* AL Image Browser */
        public const uint KEY_NOTIFICATION_CENTER = 0x1bc; /* Show/hide the notification center */
        public const uint KEY_PICKUP_PHONE = 0x1bd; /* Answer incoming call */
        public const uint KEY_HANGUP_PHONE = 0x1be; /* Decline incoming call */

        public const uint KEY_DEL_EOL = 0x1c0;
        public const uint KEY_DEL_EOS = 0x1c1;
        public const uint KEY_INS_LINE = 0x1c2;
        public const uint KEY_DEL_LINE = 0x1c3;

        public const uint KEY_FN = 0x1d0;
        public const uint KEY_FN_ESC = 0x1d1;
        public const uint KEY_FN_F1 = 0x1d2;
        public const uint KEY_FN_F2 = 0x1d3;
        public const uint KEY_FN_F3 = 0x1d4;
        public const uint KEY_FN_F4 = 0x1d5;
        public const uint KEY_FN_F5 = 0x1d6;
        public const uint KEY_FN_F6 = 0x1d7;
        public const uint KEY_FN_F7 = 0x1d8;
        public const uint KEY_FN_F8 = 0x1d9;
        public const uint KEY_FN_F9 = 0x1da;
        public const uint KEY_FN_F10 = 0x1db;
        public const uint KEY_FN_F11 = 0x1dc;
        public const uint KEY_FN_F12 = 0x1dd;
        public const uint KEY_FN_1 = 0x1de;
        public const uint KEY_FN_2 = 0x1df;
        public const uint KEY_FN_D = 0x1e0;
        public const uint KEY_FN_E = 0x1e1;
        public const uint KEY_FN_F = 0x1e2;
        public const uint KEY_FN_S = 0x1e3;
        public const uint KEY_FN_B = 0x1e4;
        public const uint KEY_FN_RIGHT_SHIFT = 0x1e5;

        public const uint KEY_BRL_DOT1 = 0x1f1;
        public const uint KEY_BRL_DOT2 = 0x1f2;
        public const uint KEY_BRL_DOT3 = 0x1f3;
        public const uint KEY_BRL_DOT4 = 0x1f4;
        public const uint KEY_BRL_DOT5 = 0x1f5;
        public const uint KEY_BRL_DOT6 = 0x1f6;
        public const uint KEY_BRL_DOT7 = 0x1f7;
        public const uint KEY_BRL_DOT8 = 0x1f8;
        public const uint KEY_BRL_DOT9 = 0x1f9;
        public const uint KEY_BRL_DOT10 = 0x1fa;

        public const uint KEY_NUMERIC_0 = 0x200; /* used by phones, remote controls, */
        public const uint KEY_NUMERIC_1 = 0x201; /* and other keypads */
        public const uint KEY_NUMERIC_2 = 0x202;
        public const uint KEY_NUMERIC_3 = 0x203;
        public const uint KEY_NUMERIC_4 = 0x204;
        public const uint KEY_NUMERIC_5 = 0x205;
        public const uint KEY_NUMERIC_6 = 0x206;
        public const uint KEY_NUMERIC_7 = 0x207;
        public const uint KEY_NUMERIC_8 = 0x208;
        public const uint KEY_NUMERIC_9 = 0x209;
        public const uint KEY_NUMERIC_STAR = 0x20a;
        public const uint KEY_NUMERIC_POUND = 0x20b;
        public const uint KEY_NUMERIC_A = 0x20c; /* Phone key A - HUT Telephony 0xb9 */
        public const uint KEY_NUMERIC_B = 0x20d;
        public const uint KEY_NUMERIC_C = 0x20e;
        public const uint KEY_NUMERIC_D = 0x20f;

        public const uint KEY_CAMERA_FOCUS = 0x210;
        public const uint KEY_WPS_BUTTON = 0x211; /* WiFi Protected Setup key */

        public const uint KEY_TOUCHPAD_TOGGLE = 0x212; /* Request switch touchpad on or off */
        public const uint KEY_TOUCHPAD_ON = 0x213;
        public const uint KEY_TOUCHPAD_OFF = 0x214;

        public const uint KEY_CAMERA_ZOOMIN = 0x215;
        public const uint KEY_CAMERA_ZOOMOUT = 0x216;
        public const uint KEY_CAMERA_UP = 0x217;
        public const uint KEY_CAMERA_DOWN = 0x218;
        public const uint KEY_CAMERA_LEFT = 0x219;
        public const uint KEY_CAMERA_RIGHT = 0x21a;

        public const uint KEY_ATTENDANT_ON = 0x21b;
        public const uint KEY_ATTENDANT_OFF = 0x21c;
        public const uint KEY_ATTENDANT_TOGGLE = 0x21d; /* Attendant call on or off */
        public const uint KEY_LIGHTS_TOGGLE = 0x21e; /* Reading light on or off */

        public const uint BTN_DPAD_UP = 0x220;
        public const uint BTN_DPAD_DOWN = 0x221;
        public const uint BTN_DPAD_LEFT = 0x222;
        public const uint BTN_DPAD_RIGHT = 0x223;

        public const uint KEY_ALS_TOGGLE = 0x230; /* Ambient light sensor */
        public const uint KEY_ROTATE_LOCK_TOGGLE = 0x231; /* Display rotation lock */

        public const uint KEY_BUTTONCONFIG = 0x240; /* AL Button Configuration */
        public const uint KEY_TASKMANAGER = 0x241; /* AL Task/Project Manager */
        public const uint KEY_JOURNAL = 0x242; /* AL Log/Journal/Timecard */
        public const uint KEY_CONTROLPANEL = 0x243; /* AL Control Panel */
        public const uint KEY_APPSELECT = 0x244; /* AL Select Task/Application */
        public const uint KEY_SCREENSAVER = 0x245; /* AL Screen Saver */
        public const uint KEY_VOICECOMMAND = 0x246; /* Listening Voice Command */
        public const uint KEY_ASSISTANT = 0x247; /* AL Context-aware desktop assistant */
        public const uint KEY_KBD_LAYOUT_NEXT = 0x248; /* AC Next Keyboard Layout Select */
        public const uint KEY_EMOJI_PICKER = 0x249; /* Show/hide emoji picker (HUTRR101) */

        public const uint KEY_BRIGHTNESS_MIN = 0x250; /* Set Brightness to Minimum */
        public const uint KEY_BRIGHTNESS_MAX = 0x251; /* Set Brightness to Maximum */

        public const uint KEY_KBDINPUTASSIST_PREV = 0x260;
        public const uint KEY_KBDINPUTASSIST_NEXT = 0x261;
        public const uint KEY_KBDINPUTASSIST_PREVGROUP = 0x262;
        public const uint KEY_KBDINPUTASSIST_NEXTGROUP = 0x263;
        public const uint KEY_KBDINPUTASSIST_ACCEPT = 0x264;
        public const uint KEY_KBDINPUTASSIST_CANCEL = 0x265;

        /* Diagonal movement keys */
        public const uint KEY_RIGHT_UP = 0x266;
        public const uint KEY_RIGHT_DOWN = 0x267;
        public const uint KEY_LEFT_UP = 0x268;
        public const uint KEY_LEFT_DOWN = 0x269;

        public const uint KEY_ROOT_MENU = 0x26a; /* Show Device's Root Menu */

        /* Show Top Menu of the Media (e.g. DVD) */
        public const uint KEY_MEDIA_TOP_MENU = 0x26b;
        public const uint KEY_NUMERIC_11 = 0x26c;

        public const uint KEY_NUMERIC_12 = 0x26d;

        /*
         * Toggle Audio Description: refers to an audio service that helps blind and
         * visually impaired consumers understand the action in a program. Note: in
         * some countries this is referred to as "Video Description".
         */
        public const uint KEY_AUDIO_DESC = 0x26e;
        public const uint KEY_3D_MODE = 0x26f;
        public const uint KEY_NEXT_FAVORITE = 0x270;
        public const uint KEY_STOP_RECORD = 0x271;
        public const uint KEY_PAUSE_RECORD = 0x272;
        public const uint KEY_VOD = 0x273; /* Video on Demand */
        public const uint KEY_UNMUTE = 0x274;
        public const uint KEY_FASTREVERSE = 0x275;

        public const uint KEY_SLOWREVERSE = 0x276;

        /*
         * Control a data application associated with the currently viewed channel,
         * e.g. teletext or data broadcast application (MHEG, MHP, HbbTV, etc.)
         */
        public const uint KEY_DATA = 0x277;

        public const uint KEY_ONSCREEN_KEYBOARD = 0x278;

        /* Electronic privacy screen control */
        public const uint KEY_PRIVACY_SCREEN_TOGGLE = 0x279;

        /* Select an area of screen to be copied */
        public const uint KEY_SELECTIVE_SCREENSHOT = 0x27a;

        /*
         * Some keyboards have keys which do not have a defined meaning, these keys
         * are intended to be programmed / bound to macros by the user. For most
         * keyboards with these macro-keys the key-sequence to inject, or action to
         * take, is all handled by software on the host side. So from the kernel's
         * point of view these are just normal keys.
         *
         * The KEY_MACRO# codes below are intended for such keys, which may be labeled
         * e.g. G1-G18, or S1 - S30. The KEY_MACRO# codes MUST NOT be used for keys
         * where the marking on the key does indicate a defined meaning / purpose.
         *
         * The KEY_MACRO# codes MUST also NOT be used as fallback for when no existing
         * KEY_FOO define matches the marking / purpose. In this case a new KEY_FOO
         * define MUST be added.
         */
        public const uint KEY_MACRO1 = 0x290;
        public const uint KEY_MACRO2 = 0x291;
        public const uint KEY_MACRO3 = 0x292;
        public const uint KEY_MACRO4 = 0x293;
        public const uint KEY_MACRO5 = 0x294;
        public const uint KEY_MACRO6 = 0x295;
        public const uint KEY_MACRO7 = 0x296;
        public const uint KEY_MACRO8 = 0x297;
        public const uint KEY_MACRO9 = 0x298;
        public const uint KEY_MACRO10 = 0x299;
        public const uint KEY_MACRO11 = 0x29a;
        public const uint KEY_MACRO12 = 0x29b;
        public const uint KEY_MACRO13 = 0x29c;
        public const uint KEY_MACRO14 = 0x29d;
        public const uint KEY_MACRO15 = 0x29e;
        public const uint KEY_MACRO16 = 0x29f;
        public const uint KEY_MACRO17 = 0x2a0;
        public const uint KEY_MACRO18 = 0x2a1;
        public const uint KEY_MACRO19 = 0x2a2;
        public const uint KEY_MACRO20 = 0x2a3;
        public const uint KEY_MACRO21 = 0x2a4;
        public const uint KEY_MACRO22 = 0x2a5;
        public const uint KEY_MACRO23 = 0x2a6;
        public const uint KEY_MACRO24 = 0x2a7;
        public const uint KEY_MACRO25 = 0x2a8;
        public const uint KEY_MACRO26 = 0x2a9;
        public const uint KEY_MACRO27 = 0x2aa;
        public const uint KEY_MACRO28 = 0x2ab;
        public const uint KEY_MACRO29 = 0x2ac;
        public const uint KEY_MACRO30 = 0x2ad;

        /*
         * Some keyboards with the macro-keys described above have some extra keys
         * for controlling the host-side software responsible for the macro handling:
         * -A macro recording start/stop key. Note that not all keyboards which emit
         *  KEY_MACRO_RECORD_START will also emit KEY_MACRO_RECORD_STOP if
         *  KEY_MACRO_RECORD_STOP is not advertised, then KEY_MACRO_RECORD_START
         *  should be interpreted as a recording start/stop toggle;
         * -Keys for switching between different macro (pre)sets, either a key for
         *  cycling through the configured presets or keys to directly select a preset.
         */
        public const uint KEY_MACRO_RECORD_START = 0x2b0;
        public const uint KEY_MACRO_RECORD_STOP = 0x2b1;
        public const uint KEY_MACRO_PRESET_CYCLE = 0x2b2;
        public const uint KEY_MACRO_PRESET1 = 0x2b3;
        public const uint KEY_MACRO_PRESET2 = 0x2b4;
        public const uint KEY_MACRO_PRESET3 = 0x2b5;

        /*
         * Some keyboards have a buildin LCD panel where the contents are controlled
         * by the host. Often these have a number of keys directly below the LCD
         * intended for controlling a menu shown on the LCD. These keys often don't
         * have any labeling so we just name them KEY_KBD_LCD_MENU#
         */
        public const uint KEY_KBD_LCD_MENU1 = 0x2b8;
        public const uint KEY_KBD_LCD_MENU2 = 0x2b9;
        public const uint KEY_KBD_LCD_MENU3 = 0x2ba;
        public const uint KEY_KBD_LCD_MENU4 = 0x2bb;
        public const uint KEY_KBD_LCD_MENU5 = 0x2bc;

        public const uint BTN_TRIGGER_HAPPY = 0x2c0;
        public const uint BTN_TRIGGER_HAPPY1 = 0x2c0;
        public const uint BTN_TRIGGER_HAPPY2 = 0x2c1;
        public const uint BTN_TRIGGER_HAPPY3 = 0x2c2;
        public const uint BTN_TRIGGER_HAPPY4 = 0x2c3;
        public const uint BTN_TRIGGER_HAPPY5 = 0x2c4;
        public const uint BTN_TRIGGER_HAPPY6 = 0x2c5;
        public const uint BTN_TRIGGER_HAPPY7 = 0x2c6;
        public const uint BTN_TRIGGER_HAPPY8 = 0x2c7;
        public const uint BTN_TRIGGER_HAPPY9 = 0x2c8;
        public const uint BTN_TRIGGER_HAPPY10 = 0x2c9;
        public const uint BTN_TRIGGER_HAPPY11 = 0x2ca;
        public const uint BTN_TRIGGER_HAPPY12 = 0x2cb;
        public const uint BTN_TRIGGER_HAPPY13 = 0x2cc;
        public const uint BTN_TRIGGER_HAPPY14 = 0x2cd;
        public const uint BTN_TRIGGER_HAPPY15 = 0x2ce;
        public const uint BTN_TRIGGER_HAPPY16 = 0x2cf;
        public const uint BTN_TRIGGER_HAPPY17 = 0x2d0;
        public const uint BTN_TRIGGER_HAPPY18 = 0x2d1;
        public const uint BTN_TRIGGER_HAPPY19 = 0x2d2;
        public const uint BTN_TRIGGER_HAPPY20 = 0x2d3;
        public const uint BTN_TRIGGER_HAPPY21 = 0x2d4;
        public const uint BTN_TRIGGER_HAPPY22 = 0x2d5;
        public const uint BTN_TRIGGER_HAPPY23 = 0x2d6;
        public const uint BTN_TRIGGER_HAPPY24 = 0x2d7;
        public const uint BTN_TRIGGER_HAPPY25 = 0x2d8;
        public const uint BTN_TRIGGER_HAPPY26 = 0x2d9;
        public const uint BTN_TRIGGER_HAPPY27 = 0x2da;
        public const uint BTN_TRIGGER_HAPPY28 = 0x2db;
        public const uint BTN_TRIGGER_HAPPY29 = 0x2dc;
        public const uint BTN_TRIGGER_HAPPY30 = 0x2dd;
        public const uint BTN_TRIGGER_HAPPY31 = 0x2de;
        public const uint BTN_TRIGGER_HAPPY32 = 0x2df;
        public const uint BTN_TRIGGER_HAPPY33 = 0x2e0;
        public const uint BTN_TRIGGER_HAPPY34 = 0x2e1;
        public const uint BTN_TRIGGER_HAPPY35 = 0x2e2;
        public const uint BTN_TRIGGER_HAPPY36 = 0x2e3;
        public const uint BTN_TRIGGER_HAPPY37 = 0x2e4;
        public const uint BTN_TRIGGER_HAPPY38 = 0x2e5;
        public const uint BTN_TRIGGER_HAPPY39 = 0x2e6;
        public const uint BTN_TRIGGER_HAPPY40 = 0x2e7;

        /* We avoid low common keys in module aliases so they don't get huge. */
        public const uint KEY_MIN_INTERESTING = KEY_MUTE;
        public const uint KEY_MAX = 0x2ff;
        public const uint KEY_CNT = KEY_MAX + 1;

        /*
         * Relative axes
         */

        public const uint REL_X = 0x00;
        public const uint REL_Y = 0x01;
        public const uint REL_Z = 0x02;
        public const uint REL_RX = 0x03;
        public const uint REL_RY = 0x04;
        public const uint REL_RZ = 0x05;
        public const uint REL_HWHEEL = 0x06;
        public const uint REL_DIAL = 0x07;
        public const uint REL_WHEEL = 0x08;

        public const uint REL_MISC = 0x09;

        /*
         * 0x0a is reserved and should not be used in input drivers.
         * It was used by HID as REL_MISC+1 and userspace needs to detect if
         * the next REL_* event is correct or is just REL_MISC + n.
         * We define here REL_RESERVED so userspace can rely on it and detect
         * the situation described above.
         */
        public const uint REL_RESERVED = 0x0a;
        public const uint REL_WHEEL_HI_RES = 0x0b;
        public const uint REL_HWHEEL_HI_RES = 0x0c;
        public const uint REL_MAX = 0x0f;
        public const uint REL_CNT = REL_MAX + 1;

        /*
         * Absolute axes
         */

        public const uint ABS_X = 0x00;
        public const uint ABS_Y = 0x01;
        public const uint ABS_Z = 0x02;
        public const uint ABS_RX = 0x03;
        public const uint ABS_RY = 0x04;
        public const uint ABS_RZ = 0x05;
        public const uint ABS_THROTTLE = 0x06;
        public const uint ABS_RUDDER = 0x07;
        public const uint ABS_WHEEL = 0x08;
        public const uint ABS_GAS = 0x09;
        public const uint ABS_BRAKE = 0x0a;
        public const uint ABS_HAT0X = 0x10;
        public const uint ABS_HAT0Y = 0x11;
        public const uint ABS_HAT1X = 0x12;
        public const uint ABS_HAT1Y = 0x13;
        public const uint ABS_HAT2X = 0x14;
        public const uint ABS_HAT2Y = 0x15;
        public const uint ABS_HAT3X = 0x16;
        public const uint ABS_HAT3Y = 0x17;
        public const uint ABS_PRESSURE = 0x18;
        public const uint ABS_DISTANCE = 0x19;
        public const uint ABS_TILT_X = 0x1a;
        public const uint ABS_TILT_Y = 0x1b;
        public const uint ABS_TOOL_WIDTH = 0x1c;

        public const uint ABS_VOLUME = 0x20;

        public const uint ABS_MISC = 0x28;

        /*
         * 0x2e is reserved and should not be used in input drivers.
         * It was used by HID as ABS_MISC+6 and userspace needs to detect if
         * the next ABS_* event is correct or is just ABS_MISC + n.
         * We define here ABS_RESERVED so userspace can rely on it and detect
         * the situation described above.
         */
        public const uint ABS_RESERVED = 0x2e;

        public const uint ABS_MT_SLOT = 0x2f; /* MT slot being modified */
        public const uint ABS_MT_TOUCH_MAJOR = 0x30; /* Major axis of touching ellipse */
        public const uint ABS_MT_TOUCH_MINOR = 0x31; /* Minor axis (omit if circular) */
        public const uint ABS_MT_WIDTH_MAJOR = 0x32; /* Major axis of approaching ellipse */
        public const uint ABS_MT_WIDTH_MINOR = 0x33; /* Minor axis (omit if circular) */
        public const uint ABS_MT_ORIENTATION = 0x34; /* Ellipse orientation */
        public const uint ABS_MT_POSITION_X = 0x35; /* Center X touch position */
        public const uint ABS_MT_POSITION_Y = 0x36; /* Center Y touch position */
        public const uint ABS_MT_TOOL_TYPE = 0x37; /* Type of touching device */
        public const uint ABS_MT_BLOB_ID = 0x38; /* Group a set of packets as a blob */
        public const uint ABS_MT_TRACKING_ID = 0x39; /* Unique ID of initiated contact */
        public const uint ABS_MT_PRESSURE = 0x3a; /* Pressure on contact area */
        public const uint ABS_MT_DISTANCE = 0x3b; /* Contact hover distance */
        public const uint ABS_MT_TOOL_X = 0x3c; /* Center X tool position */
        public const uint ABS_MT_TOOL_Y = 0x3d; /* Center Y tool position */

        public const uint ABS_MAX = 0x3f;
        public const uint ABS_CNT = ABS_MAX + 1;

        /*
         * Switch events
         */

        public const uint SW_LID = 0x00; /* set = lid shut */
        public const uint SW_TABLET_MODE = 0x01; /* set = tablet mode */
        public const uint SW_HEADPHONE_INSERT = 0x02; /* set = inserted */
        public const uint SW_RFKILL_ALL = 0x03; /* rfkill master switch, type "any" set = radio enabled */
        public const uint SW_RADIO = SW_RFKILL_ALL; /* deprecated */
        public const uint SW_MICROPHONE_INSERT = 0x04; /* set = inserted */
        public const uint SW_DOCK = 0x05; /* set = plugged into dock */
        public const uint SW_LINEOUT_INSERT = 0x06; /* set = inserted */
        public const uint SW_JACK_PHYSICAL_INSERT = 0x07; /* set = mechanical switch set */
        public const uint SW_VIDEOOUT_INSERT = 0x08; /* set = inserted */
        public const uint SW_CAMERA_LENS_COVER = 0x09; /* set = lens covered */
        public const uint SW_KEYPAD_SLIDE = 0x0a; /* set = keypad slide out */
        public const uint SW_FRONT_PROXIMITY = 0x0b; /* set = front proximity sensor active */
        public const uint SW_ROTATE_LOCK = 0x0c; /* set = rotate locked/disabled */
        public const uint SW_LINEIN_INSERT = 0x0d; /* set = inserted */
        public const uint SW_MUTE_DEVICE = 0x0e; /* set = device disabled */
        public const uint SW_PEN_INSERTED = 0x0f; /* set = pen inserted */
        public const uint SW_MACHINE_COVER = 0x10; /* set = cover closed */
        public const uint SW_MAX = 0x10;
        public const uint SW_CNT = SW_MAX + 1;

        /*
         * Misc events
         */

        public const uint MSC_SERIAL = 0x00;
        public const uint MSC_PULSELED = 0x01;
        public const uint MSC_GESTURE = 0x02;
        public const uint MSC_RAW = 0x03;
        public const uint MSC_SCAN = 0x04;
        public const uint MSC_TIMESTAMP = 0x05;
        public const uint MSC_MAX = 0x07;
        public const uint MSC_CNT = MSC_MAX + 1;

        /*
         * LEDs
         */

        public const uint LED_NUML = 0x00;
        public const uint LED_CAPSL = 0x01;
        public const uint LED_SCROLLL = 0x02;
        public const uint LED_COMPOSE = 0x03;
        public const uint LED_KANA = 0x04;
        public const uint LED_SLEEP = 0x05;
        public const uint LED_SUSPEND = 0x06;
        public const uint LED_MUTE = 0x07;
        public const uint LED_MISC = 0x08;
        public const uint LED_MAIL = 0x09;
        public const uint LED_CHARGING = 0x0a;
        public const uint LED_MAX = 0x0f;
        public const uint LED_CNT = LED_MAX + 1;

        /*
         * Autorepeat values
         */

        public const uint REP_DELAY = 0x00;
        public const uint REP_PERIOD = 0x01;
        public const uint REP_MAX = 0x01;
        public const uint REP_CNT = REP_MAX + 1;

        /*
         * Sounds
         */

        public const uint SND_CLICK = 0x00;
        public const uint SND_BELL = 0x01;
        public const uint SND_TONE = 0x02;
        public const uint SND_MAX = 0x07;
        public const uint SND_CNT = SND_MAX + 1;
    }
}
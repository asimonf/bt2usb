# BT2USB - A BT Redirect tool

This project came about as a way to circumvent a limitation of the Windows operating system.
Windows doesn't allow more than one BT radio adapter connected to it. 

I set up myself to build a multi-seat solution for my house. I wanted to enjoy couch
playing with my PC, but I didn't want to purchase a PC for each TV on my house. Especially 
given the current state of GPU pricing. 

I wanted to be wireless when connected to the TV for gaming, whether I used a KB/Mouse 
combo or a Gamepad such as the DS4, but here is where the problem arises. Either I duplicate
peripherals for each TV on the house (Living room setup or bed setup), or I find a way 
to use BT to be able to carry my gaming peripherals around, especially for multiplayer sessions.
Good quality peripherals are expensive, as are mouses and keyboards. So, I set out to find a way to use BT peripherals wherever I was in my house. I had a bunch
of Beaglebone boards laying around from other projects so I figured I might give it a shot.

BT and USB share HID descriptors for many common peripherals. This includes keyboards, mouses 
and the DS4 gamepad. Therefore, I figured I might try using the `libcomposite` kernel module
to build a custom USB device using the Beaglebone as a controller board. Keyboard and mouse was
pretty easy. I just needed to find a compatible mouse and keyboard HID descriptor and use that
to replicate a HID device on the Beaglebone, then simply reroute data from `/dev/hidraw#` to 
`/dev/hidg#` and viceversa. Seems to work just fine.

DS4 proved more difficult. I wanted to maintain as much functionality as I could from the DS4
device, but unfortunately, simply redirecting USB reports from one side to the other didn't seem
to work. For some reason Windows simply didn't like the composite device. The sony driver probably
verifies vendor data and/or does initialization which can't be simply rerouted. So I turned to ViGEM.

I made a server-side app for the PC that communicates with the ViGEM driver and simply send
the raw HID reports. Seems to work just fine. Recognizes everything including the touchpad and
the gyro data as well. No apparent lag either. For sending data back, I build the HID report on the
PC and submit that through the Socket back to the Beaglebone and then straight to the HIDRAW device.
Led color changing and FF works fine too (though for some reason the led color keeps flickering).

# TL;DR

This redirects BT peripherals through a USB gadget on the beaglebone and makes the computer think
the peripherals are connected through USB. Neither the mouse nor the keyboard require configuration
or drivers on the PC side, just pairing on the Beaglebone. The DS4 gamepad requires an additional
piece of software to act as a client connecting through an RNDIS network interface also provided by the
Beaglebone board through the same `libcomposite` gadget.

# What's missing

This is a proof of concept. Reliability is still a concern, for example when connecting and disconnecting
devices, or when closing the client app and attempting to reconnect. Ideally, pairing should be possible
through some sort of webpage to make configuration much easier.

After that, possibly adding other gamepad support. I figure regular XInput gamepads should be much easier 
to support by just providing a compatible HID Descriptor and simply redirecting reports like for mouse and
keyboards. 

# Final notes

There doesn't appear to be a limitation regarding BT connectivity as far as I can tell. I bought a cheap
BLE USB adapter that was compatible with Linux and used that as my BT adapter on the Beaglebone.

I didn't write any code specific to the Beaglebone, but I did write a configuration script that assumes
a Beaglebone for configuring the `libcomposite` module. The rest should work as is on any other SBC computer
that has a device USB connector like the Raspberry Zero W (I decided against this board mostly out of concern
for the BT range).

This was made using C# and .Net Core 3.1. I uploaded this for reference. This contains code
that doesn't belong to me that I could not manage to make work as a NuGET package and I decided
against fighting the package manager, so please bear in mind that when navigating through it.

I built my first prototype in C++ but, honestly, as a rookie C/C++ programmer I just didn't feel 
like making my life harder than necessary. I didn't think there would be enough of a performance 
difference to warrant one over the other. The other side of the coin, however, was writing, and fighting,
the p/invoke definitions of obscure Linux specific APIs to make this work.

# Credit

[This](https://github.com/samartzidis/RaspiKey) served as a big inspiration and as a starting point. 
The idea to use a server for configuration came from that project as well.

The Udev stuff comes from [here](https://github.com/DandyCode/Dandy.Linux). Which I copy/pasted straight
to my project because the NuGET package didn't support the `linux-arm` target and, copying it worked just fine.
Besides, that project has been inactive for a few years now, so I figured this was faster. I hope the author
doesn't mind. If they do, I'll remove and write my own mappings over `libudev`. 

# Licence

I don't know what the license the udev code is, couldn't find anything on their repository. 
The rest of the code: GPL v3. 

I promise I will clean it up at some point and try to use actual NuGET packages for the things I copy/pasted
from other repositories. This is mostly related to system-level interaction libraries related to libudev and libc though.

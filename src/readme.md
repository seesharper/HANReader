lsusb

```
Bus 001 Device 004: ID 067b:2303 Prolific Technology, Inc. PL2303 Serial Port
Bus 001 Device 005: ID 0bda:2838 Realtek Semiconductor Corp. RTL2838 DVB-T
Bus 001 Device 006: ID 0424:7800 Standard Microsystems Corp.
Bus 001 Device 003: ID 0424:2514 Standard Microsystems Corp. USB 2.0 Hub
Bus 001 Device 002: ID 0424:2514 Standard Microsystems Corp. USB 2.0 Hub
Bus 001 Device 001: ID 1d6b:0002 Linux Foundation 2.0 root hub
```

dmesg | grep usb

```
[    3.809651] usb 1-1.3: New USB device found, idVendor=067b, idProduct=2303, bcdDevice= 3.00
[    3.815125] usb 1-1.3: New USB device strings: Mfr=1, Product=2, SerialNumber=0
[    3.815138] usb 1-1.3: Product: USB-Serial Controller
[    3.815152] usb 1-1.3: Manufacturer: Prolific Technology Inc.
[    3.906561] usb 1-1.1.3: new high-speed USB device number 5 using dwc_otg
[    4.048624] usb 1-1.1.3: New USB device found, idVendor=0bda, idProduct=2838, bcdDevice= 1.00
[    4.054100] usb 1-1.1.3: New USB device strings: Mfr=1, Product=2, SerialNumber=3
[    4.059877] usb 1-1.1.3: Product: RTL2838UHIDIR
[    4.062819] usb 1-1.1.3: Manufacturer: Realtek
[    4.065678] usb 1-1.1.3: SerialNumber: 00000001
[    4.386481] usb 1-1.1.1: new high-speed USB device number 6 using dwc_otg
[    4.517073] usb 1-1.1.1: New USB device found, idVendor=0424, idProduct=7800, bcdDevice= 3.00
[    4.522399] usb 1-1.1.1: New USB device strings: Mfr=0, Product=0, SerialNumber=0
[    7.388535] usb 1-1.1.3: dvb_usb_v2: found a 'Realtek RTL2832U reference design' in warm state
[    7.408856] usbcore: registered new interface driver usbserial_generic
[    7.408932] usbserial: USB Serial support registered for generic
[    7.414783] usbcore: registered new interface driver pl2303
[    7.414884] usbserial: USB Serial support registered for pl2303
[    7.422639] usb 1-1.3: pl2303 converter now attached to ttyUSB0

```

# EnetUnetTransport

This is project which make ENet ([with this wrapper](https://github.com/nxrighthere/ENet-CSharp)) as transport layer for UNET HLAPI, it is require Unity 2018.3 or above

## How to change transport layer for UNET HLAPI

Put following codes to anywhere before NetworkManager initialized
```
NetworkManager.activeTransport = new EnetUnetTransport();
```

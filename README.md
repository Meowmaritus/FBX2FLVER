# FBX2FLVER
Experimental FBX model importer for Dark Souls, Dark Souls: Remastered, Dark Souls II: Scholar of the First Sin, and Dark Souls III. Support for other games is as simple as a small patch once I get around to it.

## Some notes:
* For FLVER/TPF paths ***you can select a .CHRBND.DCX, .TEXBND.DCX, .BND.DCX etc and then select the file inside! WAY EASIER PLEASE USE THIS METHOD I'M BEGGING YOU DON'T EXTRACT THEN IMPORT TO LOOSE FILES AND REPACK I'M BEGGING YOU PLEASE SAVE YOURSELF FROM THIS NONSENSE AND SELECT A BND! A BND FILE!!!!!!***
* Material naming convention: `CustomMaterialName | IngameMTDName` e.g. `Blade | P_Metal[DSB]`
* Certain textures are actually imported (supports FBX with textures embedded inside too) and will output a .TPF for you even:
  * Diffuse Map (DS1/DS2/DS1R) / Albedo Map (BB/DS3/SDT) uses the `Diffuse Color` channel of the FBX.
  * Specular Map (DS1/DS2/DS1R) / Reflectance Map (BB/DS3/SDT) uses the `Specular Color` channel of the FBX.
  * Normal Map uses the `Bump` channel of the FBX.
* All textures must be a proper DDS format the game supports otherwise the game will immediately crash upon trying to load the textures.

## Special Thanks
* **TKGP** for his incredible [SoulsFormats library](https://github.com/JKAnderson/SoulsFormats).
* **SiriusTexra** for helping with testing.
* **Dropoff** for helping with testing.

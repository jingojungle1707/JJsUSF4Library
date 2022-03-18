# JJsUSF4Library

net5.0 library for reading and writing DIMPS engine game files (SF4, SFxT, Saint Seiya Soldiers' Soul and Dragonball Xenoverse). Classes are written primarily with SF4 in mind, aside from a couple of SFxT specific variants, so YMMV working with files from later DIMPS engine games.

File classes inherit from JJsUSF4Library.FileClasses.USF4File; loading files is done through USF4Utils.OpenFileStreamCheckCompression(), which will attempt to determine the file type based on its DWORD and return a populated instance of the appropriate class. If the file is a zipped .emz file, it will extract it to an .emb in memory before populating the class instance. Alternatively you can create your own instance and populate it using USF4File.ReadFromStream(), but this will not automatically handle unzipping. 

See the TestApp for an example of loading files.

USF4File.GenerateBytes() returns byte[] representing the instance, in case you need to modify the binary data in memory for some reason before writing to file. Mostly it's just been left exposed for compatability with some of my old code.

USF4File.SaveFile() will save the current state of the class instance to the specified path, and will do its best to generate a functional binary file.

This is a work in progress, and I have no idea what I'm doing, so any part of the implementation is liable to change at any time...

Thanks to piecemontee and revelation and their work on SF4explorer, going through the SF4explorer sourcecode and trawling their old discussion threads both inspired this project and provided a large chunk of the groundwork, particularly in regards to .emo and .ema files.

Huge thanks to BEAR who helped me write the very first implementations of these file classes.

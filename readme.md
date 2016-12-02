SnazzyMap
==============
This map system allows formatting to be used, and downloaded from a web service. Use SnazzyMap to create the style files:
https://snazzymaps.com/

Dependencies
==============
This project has the following dependencies, which should be installed using NuGet.
- OpenTK:
--------------
	A library for .NET that implements OpenGL. This is mainly just for testing out core functionality in Visual Studio.

	Install by typing the following in the Package Manager Console:
		`Install-Package OpenTK`

	More information can be found here:
		- https://github.com/opentk/opentk

- Google Maps API for .NET:
--------------
	A library for .NET that allows an OOP approach to accessing Google Maps data. It allows the developer to more easily
	create and work with web data sent back and forth with the Google servers.

	Install by typing the following in the Package Manager Console:
		`Install-Package gmaps-api-net`
	
	More information can be found here:
		- https://github.com/ericnewton76/gmaps-api-net

- Json.NET:
--------------
	A library for both encoding and decoding JSON strings.

	Install by typing the following in the Package Manager Console:
		`Install-Package Newtonsoft.Json -Version 9.0.1`

	More information can be found here:
		- http://www.newtonsoft.com/json
		- https://github.com/JamesNK/Newtonsoft.Json
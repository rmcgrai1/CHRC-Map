Formatting
==============
If it is desired, the map can be styled by passing an external style sheet formatted in JSON. Websites for generating such a style sheet can be found here:
- https://snazzymaps.com/
- https://mapstyle.withgoogle.com/
- http://www.mapstylr.com/map-style-editor/

Dependencies
==============
This project has the following dependencies, which should be installed using NuGet.
OpenTK:
--------------
A library for .NET that implements OpenGL. This is mainly just for testing out core functionality in Visual Studio.

Install by typing the following in the Package Manager Console:
`Install-Package OpenTK`

More information can be found here:
- https://github.com/opentk/opentk

Google Maps API for .NET:
--------------
A library for .NET that allows an OOP approach to accessing Google Maps data. It allows the developer to more easily
create and work with web data sent back and forth with the Google servers.

Install by typing the following in the Package Manager Console:
`Install-Package gmaps-api-net`
	
More information can be found here:
- https://github.com/ericnewton76/gmaps-api-net

Json.NET:
--------------
A library for both encoding and decoding JSON strings.

Install by typing the following in the Package Manager Console:
`Install-Package Newtonsoft.Json -Version 9.0.1`

More information can be found here:
- http://www.newtonsoft.com/json
- https://github.com/JamesNK/Newtonsoft.Json

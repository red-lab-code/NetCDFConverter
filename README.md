NetCDFConverter
===============

a simple C# library to read and parse NetCDF files into a more managable non-binary form.

this is far from a complete library for reading NetCDF files (the spec is pretty extensive), but it covers most use cases that I've come across.
it's simple enough to create a wrapper class to take the data read from the nc file by the Header class and move it into a properly formatted, useful form.

Includes an example class for reading some basic GeoSpatial data from an NC file.

##Example

```
// you can load up a geospatial NC file simply by calling
GeoSpatialData gsd = new GeoSpatialData(@"path_to_nc");

// if you have non-geospatial data then you can first load the data from a file into a Header object
Header head = new Header(new FileStream(@"path_to_nc", FileMode.Open, FileAccess.Read));
// and then deal with the loaded data however you need

```
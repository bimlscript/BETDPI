# -BETDPI
Biml Enabled Tabular Data Package Importer



[Tabular Data Package](http://dataprotocols.org/tabular-data-package/) is a technology-agnostic format for publishing/transporting data. It uses text files (in the form of CSV files and a JSON file) to house data and define the nature (i.e. data-types) of that data.
The aim here is to create a data publishing format that can be used to move data between different systems via text files.

BETDPI is an attempt to use [BIML](http://bimlscript.com/) to generate SSIS packages that can:
* export data from a relational database to a Tabular Data Package and
* import data from a Tabular Data Package into a relational database


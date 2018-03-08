DESIGN

A conventional heat map consists of a 2D grid of colored squares where each square represents an observation of a random variable and the color of the square is proportional to the value of that observation. 2D heat maps are used pervasively in the Biological sciences and both the grid and the dimension mapped to the grid can represent a variety of concepts. Genes, experimental conditions, subjects, genomic elements, etc... are distributed on the grid where a color palette is used to encode transcript abundance, protein concentration, conservation, activation, etc...

It is often desirable to map several dimensions to the same grid. This situation is usually resolved by plotting a separate 2D heat map for each dimension. The analysis of relationships between multiple dimensions is usually hindered by this design due to the loss of context and orientation when transitioning between dimensions in large data sets. It is our goal to explore alternative representations that superimpose and interleave several dimensions onto the same grid. Through this approach we aim to find a solution that decreases the disorienting effect of transitioning between dense and separately graphed volumes of data and to increase the interpretability of multidimensional data without overwhelming the user's senses.

The current version includes the following features:

- Updating graphical parameters in real time.
The parameters that govern the graphing of data can be changed in real time. This allows for the seamless transition between dimensions without losing the current perspective and arrangement of the 3D heat map.

- Superimposing dimensions.
In order to map several dimensions onto the same grid we have chosen simple yet multifaceted geometries. The graphical unit can hold one dimension as its height, a second dimension as the color on its horizontal surface and a third dimension as the color of its vertical surfaces.

- Interleaving dimensions.
An alternative to superimposition that allows for an arbitrary number of dimensions to be mapped to the same grid is interleaving. This is achieved by consolidating the same row in the grid across all dimensions and plotting the consolidated rows adjacently. Spacer of different widths are used to convey the hierarchical structure of rows.

All features can be explored in combination. It is possible to superimpose, interleave and switch between dimensions without interrupting the path of flight through the data or losing the point of view. It is important in this exercise that the user is able to start from a conventional 2D heat map and incrementally add dimensions as they elaborate and refine their analysis and interpretation. It is also up to the user to decide which variables are better represented by height or color.


DATA IMPORT

3D Heatmap reads data from an SQLite database.  This does not require any database server; it is completely contained in a single file, "testdata.sqlite" which should be in the same folder as the executable file.  The program will list in the "Data Selection" menu any SQLite data tables whose names begin with "heat_".  Each such table must contain, at a minimum, integer columns named "col", "row", and "bin", and a float column named "height".  If the table contains additional integer columns they will appear in the "Top Color" and "Side Color" menus when the dataset is selected.

Two optional tables allow more control of 3D Heatmaps.  The first allows custom labels for rows.  If a dataset is named "heat_foo", then the corresponding label table should be named "heatrows_foo".  It will have two columns: and integer column "row" holding the row number, and a text column "name" holding the label for that row.

The second optional table allows custom colors to be assigned for values in the optional columns of the heat_ table.  For example, if you wanted each data point to be associated with one of three possible outcomes, "good", "indeterminate", "poor", you might create an integer column "outcome" in heat_foo, holding values 1, 2, or 3.  You could create a table called heatfield_foo_outcome, which would contain three integer columns, "value", "r", "g", "b" and one text column "name", like so:
value r     g      b      name
1     0     255    0      good
2     255   255    0      indeterminate
3     255   0      0      poor

If you assigned "Top Color" to "outcome", then the tops of good outcomes would be green, indeterminate would be yellow, and poor would be red.  Note that in order to see these custom colors you must not select any other range of colors: red->green, grayscale, etc.

One easy way to get data into the SQLite table is to use the Firefox browser plugin "SQLite Manager".  If, for example, you have a spreadsheet table with columns named "row", "col", "height", "bin", and "outcome", then save it as a tab-delimited CSV file.  Use the import function of SQLite Manager to import it, using CSV format, and checking the box labelled "First row contains column names".  If all your data is in a single bin, just use 1 for all the bin values.

If you prefer to work from a command line, we have provided some perl scripts to help.  Here is how they work:

All data sets being processed into a 3D heat map design should have the same dimensions, that is, they should correspond the same underlying grid. All scripts require a data ID as a first argument. This ID will then appear in the list of available datasets within the 3D heat map program.

Data sets can be superimposed using the merge.pl script. An arbitrary number of superimposed data sets can be binned together with the bin.pl script. The following example processes the input data and imports it to the database.

    rownames.pl PATTERN patred.txt
    merge.pl PATTERN patred.txt patgreen.txt
    merge.pl PATTERN patgreen.txt patblue.txt
    merge.pl PATTERN patblue.txt patred.txt
    bin.pl PATTERN
    import.pl PATTERN

If you are running the scripts under Windows type "perl " preceding the previous commands, if you're running a Linux/UNIX distribution precede them with "./".




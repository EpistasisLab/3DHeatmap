# 3D Heatmap

3D Heatmap is a project of

    Idea Factory
    Institute for Biomedical Informatics (IBI)
    Perelman School of Medicine
    University of Pennsylvania

    Director: Dr. Jason Moore
    Lead Developer: Michael Stauffer (stauffer@upenn.edu)

# Overview

3D Heatmap is a program for simultaneoulsy visualizing up to three dimensions of independent variables (Feature Values) against two independent variables (Sample and Feature sets).

The goal is to provide a better way to find relationships between multiple dependent variables, in particular for large data sets where viewing three conventional 2D heatmaps side by side can become very awkward.

### Example of Appropriate data

An example of an appropriate data set is one in which gene expression (the Feature Value, or independent variable) is measured for a number of different genes (the Feature Set, or first independent variable) across different cell lines (the Sample Set, or second independent variable), under three different environmental conditions - thus generating a three-dimensional feature value set. Typically these would be displayed using three separate 2D heatmaps, one for each environmental condition. Using the 3D Heatmap tool, these three conditions can be visualized in one map.

You can load a demo data set from the user interface to play around with things.

### System Requirements

The project is built in Unity, and can be compiled for Windows, MacOS and (probably) Linux, Android and iOS (hasn't been tried yet). The main system limitation will be memory requirements for very large data sets - you'll just have to try it and see if it works. 

### VR support

Basic VR support is provided for SteamVR-capable devices. VR can make for easier viewing of large data sets. See below for more details.

### Data Format

The program requires data to be in either of the the common csv and tab-delimited formats. See below for more details.

### Call for Data

As we develop this project, we are looking for data sets from researchers and clinicians that might benefit from this new tool. We'd like to try the tool with your data to learn how it may help your visualization needs.

### License
MIT License. See LICENSE doc in repo.

# Motivation for Developing 3D Heatmap

## Conventional 2D Heatmaps

A conventional heat map consists of a 2D grid of colored squares where each square represents an observation of a single (1D) dependent variable (Feature Value) for a given pair of members of two independent variables (Sample and Feature sets)The color of the square is proportional to the Feature Value. 2D heat maps are used pervasively in the Biological sciences and the Samples, Features, and Feature Values can represent a variety of concepts. The rows and columns of the grid consist of Sample and Feature sets, which may represent genes, experimental conditions, subjects, genomic elements, etc. The observed Feature Value in each grid is shown using a color palette, and may represent transcript abundance, protein concentration, conservation, activation, etc.

![Conventional 2D Heatmap](readme_images/Heatmap.png "2D Heatmap example, by Miguel Andrade at English Wikipedia")

## Visualizing Multiple Dimenions

However, it is often desirable to map several dimensions of Feature Values. This situation is usually resolved by plotting a separate 2D heat map for each dimension. The analysis of relationships between multiple dimensions is usually hindered by this design due to the loss of context and orientation when transitioning between dimensions in large data sets. It is our goal to explore alternative representations that superimpose and interleave several dimensions onto the same grid. Through this approach we aim to find a solution that decreases the disorienting effect of transitioning between dense and separately graphed volumes of data and to increase the interpretability of multidimensional data without overwhelming the user's senses.

## 3D Heatmap example

Here is an example of the tool in its current form.

Each block represents three Feature Values, one each shown by block height, side color and top color.

![Conventional 2D Heatmap](readme_images/3dheatmap.png "3D Heatmap")

# <a name="Usage"></a>Usage Instructions

### File Format

### Loading and Viewing Data

1. Click _Demo Data_ button in the bottom left of the user interface to view some demo data. Otherwise...
1. Under _Data Variables_ in the GUI on the left, select and load your data files
1. Under -Visual Mappings_, assign the variables to each of Height, Top Color, and Side Color.
1. Choose color tables for Top and Side colors.
1. Click the Redraw button, or press F2.

|a|b|c|
|1|2|3|


### View Controls

|x|Mouse|Touch|Keyboard|
|Move|left-click & drag|two-finger drag|arrow keys|
|Rotate|right-click & drag|three-finger drag| ctrl + arrows|
|Zoom|scroll wheel|two-finger pinch|shift + arrows, or +/- keys|



# <a name="VRsupport"></a>VR Support

"<b>VR</b>\n\n" +
                "SteamVR/OpenVR devices are supported for viewing in VR.\nHold the controller's grip button to move the data, and the trigger to inspect it.\n\n"+

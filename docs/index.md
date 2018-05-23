# 3D Heatmap

## MOTIVATION

### 2D Heatmaps

A conventional heat map consists of a 2D grid of colored squares where each square represents an observation of a single (1D) dependent variable (Feature Value) for a given pair of members of Sample and Feature sets. The color of the square is proportional to the Feature Value. 2D heat maps are used pervasively in the Biological sciences and the Samples, Features, and Feature Values can represent a variety of concepts. The rows and columns of the grid consist of Sample and Feature sets, which may represent genes, experimental conditions, subjects, genomic elements, etc. The observed Feature Value in each grid is shown using a color palette, and may represent transcript abundance, protein concentration, conservation, activation, etc.

![Conventional 2D Heatmap](images/Heatmap.png)

### Visualizing Multiple Dimenions

However, it is often desirable to map several dimensions of Feature Values. This situation is usually resolved by plotting a separate 2D heat map for each dimension. The analysis of relationships between multiple dimensions is usually hindered by this design due to the loss of context and orientation when transitioning between dimensions in large data sets. It is our goal to explore alternative representations that superimpose and interleave several dimensions onto the same grid. Through this approach we aim to find a solution that decreases the disorienting effect of transitioning between dense and separately graphed volumes of data and to increase the interpretability of multidimensional data without overwhelming the user's senses.

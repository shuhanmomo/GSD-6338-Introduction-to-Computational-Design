# GSD-6338-Introduction-to-Computational-Design
My class projects in the GSD 6338 about using C# , python and grasshopper in Geometric Computation and Data Visualization 

## Kirigametry
### Collaboration
Lingbo Li, Henry Chung
Video Link: https://youtu.be/a-1zU8A4ovI

### Project Statement
The construction of physical models is a critical aspect of architectural education and design in Gund Hall. However, this process often results in the waste of significant amounts of materials, including single-use final models, well-crafted scaled furniture, and partially used materials. Given this, the project aims to explore and address the issue of material waste in the construction of architectural models. The focus of the project will be to encourage the reuse of model materials, promoting a sustainable approach to architecture by recognizing that one person's waste can be another's resource.
  
We implement Kirigamy algorithm from paper to generate a bounding hexagon to fit the random cuts by detecting the location of each edge points and minimalize the wasted area. The hexagon will fit into the background triangular grid. Then the algorithm will generate the crease pattern indicating mountain and valley folds, which also enables further Kangeroo simulation process. The physical demos we produced show how the curvature created by closing the cuts is resolved by applying origami folds and how the geometric complexity is produced through these operations. We also implement the surface prototype we tested through physical demos into a bag design.
  
### Project Concept
![kiri concept](https://github.com/shuhanmomo/GSD-6338-Introduction-to-Computational-Design/blob/93844bebb3600069c0a92ae05628ed4ef3ef472d/img/kirigamy-concept.png)
### Grasshopper Script
The grasshopper and C# script inside is stored under the folder'Kirigametry'
![kiri script](https://github.com/shuhanmomo/GSD-6338-Introduction-to-Computational-Design/blob/93844bebb3600069c0a92ae05628ed4ef3ef472d/img/4d-script.png)
what the script trying to do is to implement the algorithm from paper to build the bounding hexagons for given random holes.  
![kiri hexagon](https://github.com/shuhanmomo/GSD-6338-Introduction-to-Computational-Design/blob/93844bebb3600069c0a92ae05628ed4ef3ef472d/img/kirigamy-crease.jpg)
### Physical Results
![kiri physical](https://github.com/shuhanmomo/GSD-6338-Introduction-to-Computational-Design/blob/851ff2019981477247571f9e05d76e55907d511f/img/kirigamy-result.png)
  
  
  
## 4D Visualization
### Collaboration
Lingbo Li, Sheng Qian  
Video Link: https://youtu.be/11GrcHLRRIw

### Project Statement
In the general understanding, the 4th Dimension is time. But in this Assignment, we are not focusing on the 3D slices along various points in time. We are interested in   considering the 4th Dimension as a Spatial Dimension. Imagine there is a 4th axis w perpendicular to the world coordinate system (x,y,z). And each point in a 4D space    have 4 components. The coordinate of a 4D point is (x,y,z,w).Now, the key here is that what the 4D being sees are 3D images, and we understand 3D very well, so it is   perfectly possible for us to “see” the images in the 4D being's eyes. All we need to do is to learn how to infer the 4th dimension from these 3D images.  
  
In the Grasshopper script, the first step is to scale and locate the imported file in the world center , then remesh existing 3d scan objects in to a fixed amount of  dots.Inside the C# components, the alogrithm extrudes all points along the 4th axis (w = [-10,10]) and applies a 4d rotation matrix to the points. Finally, it filters  the transformed points whose w is equal to 0. For display, it colors all visible points by their original relative w distance to w = 0. So in the sequencial images  above, we can clear see which points are from other hyperplane.
  
### Dimensional Analogy
![analogy](https://github.com/shuhanmomo/GSD-6338-Introduction-to-Computational-Design/blob/f8fefb0260214ccbb1d26c64a9bdf10edfbd723d/img/4d-mechanism.png)
### Grasshopper Script
The grasshopper and C# script inside is stored under the folder'4D Visualization'
![4dscript](https://github.com/shuhanmomo/GSD-6338-Introduction-to-Computational-Design/blob/f8fefb0260214ccbb1d26c64a9bdf10edfbd723d/img/4d-script.png)
### Visualization Result
![4dvisualization](https://github.com/shuhanmomo/GSD-6338-Introduction-to-Computational-Design/blob/f8fefb0260214ccbb1d26c64a9bdf10edfbd723d/img/4d-transformation.png)






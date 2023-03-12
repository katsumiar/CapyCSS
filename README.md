[![MIT License](http://img.shields.io/badge/license-MIT-blue.svg?style=flat)](LICENSE)

# CapyCSS
![CapyCSS_netproject](https://user-images.githubusercontent.com/63950487/189475693-e937a85b-d228-450b-9288-fc376a7200a4.gif)

## Features
CapyCSS is a visual scripting tool for C# that enables node-based programming. By importing DLLs, creating scripts can be easily achieved.

## Target Environment
* .NET Desktop 6
* c#

## Solution for "The data version are incompatible."
Please delete the old "CapyCSS" folder in your Documents directory.

## Execution options
When a script file is specified as a command-line argument, it is loaded at startup. If the -as option is also specified, nodes with the "Entry Point" checkbox selected will be executed after the script is loaded. If the -ae option is specified, the program will automatically exit after the script is executed. If the -ase option is specified, the program will execute -as and -ae sequentially.
```
CapyCSS.exe [cbs fils]
CapyCSS.exe [project file]&[cbs fils]
CapyCSS.exe -as [cbs fils]
CapyCSS.exe -as -ae [cbs fils]
CapyCSS.exe -ase [cbs fils]
CapyCSS.exe -as [project file]&[cbs fils]
CapyCSS.exe -ase [project file]&[cbs fils]
Note: The current directory is set to "CapyCSS executable file directory\Sample".
```
Note: You can set an exit code using the "SetExitCode" node, which will force the program to terminate when the node is executed.

## Console output
If launched from the console, the outputs from the "Call File" and "OutConsole" nodes will also be displayed in the console.

## Instructions
* You can display the command window by pressing the space bar or wheel button.
* Once the command window is displayed, you can place nodes by clicking on an item under the "Program" section of the command menu.
* To connect nodes, left-click and drag the circle of one node to the square of another node.
* You can disconnect connected nodes by holding down the Ctrl key and left-clicking on the square (double-clicking also works).
* Hold down the Shift key and drag your mouse to select multiple nodes.
* To delete selected nodes, press the Delete key.
* You can move a node by left-clicking on its name and dragging it (Ctrl key moves the node along the guide).
* You can move the entire screen by left-clicking and dragging (if nodes are selected, they will move with the screen).
* You can zoom in or out by rotating the wheel button.
* Use Ctrl + A to select all nodes, Ctrl + Space to deselect, Ctrl + C to copy selected nodes, and Ctrl + V to paste copied nodes.
* Use Ctrl + S to save the script, Ctrl + O to load a script, Ctrl + N to create a new script, and Ctrl + Shift + N to clear the script.
* Press 'j' key to adjust the script display to the center of the screen, and Ctrl + j to adjust it to the upper-left corner of the screen.
* Press F5 to execute the node with the "Entry Point" checkbox checked.

Note: That you can edit node and argument names by double-clicking on them, but variable names can only be edited in the variable list on the left side of the screen.

## Convenient features of the script
* Hover over an argument to view its contents.
* Simply drag and drop numbers and characters to create constant nodes.
* The node connections allow for flexible casting, though it does not guarantee execution.

## Hint display
This software supports both English (machine translated) and Japanese.<br>
To display the interface in Japanese, you need to modify the content of the app.xml file created after execution.<br>
Change the following line from:<br>
&lt;Language>en-US&lt;/Language&gt;<br>
to: <br>
&lt;Language>ja-JP&lt;/Language&gt;<br>
After restarting the application, the new settings will take effect.<br>
Note: Currently under adjustment.

## Features supported by method import
* Class constructor (includes the ability to create new instances).
* Method (accepts the 'this' keyword as an argument).
* Static method.
* Getter (retrieves the value of a private field).
* Setter (sets the value of a private field).

The class to which the method belongs must have a public access modifier.<br>
Abstract classes are excluded.<br>
The method must have a public access modifier.<br>

## The argument type (and modifiers) of the method supported by the script
* int, string, double, byte, sbyte, long, short, ushort, uint, ulong, char, float, decimal, bool, object
* Arrays (Supported for scripting from ver0.3.5.0 onwards) Note: up to one dimension only.
* Types that have IEnumerable (can be manipulated on UI)
* Class
* Struct
* Interface
* Delegate
* Enum
* Generics
* Initializer value
* ref (reference)
* out parameter modifier
* in parameter modifier
* params parameter array
* Nullable types (Supported for scripting from ver0.3.4.0 onwards)

## The return type of the method supported by the script
* int, string, double, byte, sbyte, long, short, ushort, uint, ulong, char, float, decimal, bool, object
* Arrays
* Class
* Struct
* Interface
* Delegate
* Enum
* Generics
* Void
* Nullable type

## Custom types
* text type (a string type that has a multi-line editing area on the UI)
* password type (a string type that masks its display on the UI)

## Method calls from within a method
Delegate-type arguments can be connected to nodes of matching return type. However, for Action-types, connection is unconditional.<br>
When referencing a delegate variable, the execution result of the connected node is returned.

## License
CapyCSS is open source software released under the [MIT License](https://github.com/katsumiar/CapyCSS/blob/main/LICENSE). [![MIT License](http://img.shields.io/badge/license-MIT-blue.svg?style=flat)](LICENSE)

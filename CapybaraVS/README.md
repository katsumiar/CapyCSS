# CbVS
![CbVS](https://user-images.githubusercontent.com/63950487/85939295-168b3c00-b94f-11ea-8fcf-07b27c3dfc26.png)

## Summary
You can easily write a program using the mouse as you write notes.<br>
There is a function to import so that it can be used as a node by modifying the source and adding a method(Requires build).<br>

## Target
* .Net Core 3.1
* c#
* wpf
* Math.NET Numerics
* MaterialDesignInXamlToolkit

## Options
If you specify the saved script file in the command argument, it will be automatically loaded at startup. If -as is specified for the option, the node specified as "public" will be executed after the script is loaded. If -ae is specified for the option, it will be automatically terminated after the script is executed.
```
CapybaraVS.exe script.cbs
CapybaraVS.exe -as script.cbs
CapybaraVS.exe -as -ae script.cbs
```

## Characteristic
This is a sample that uses the attribute to nodeize the written method.
```
[ScriptMethod]
public static System.IO.StreamReader GetReadStream(string fileName, string encoding = "utf-8")
{
    var encodingCode = Encoding.GetEncoding(encoding);
    return new System.IO.StreamReader(fileName, encodingCode);
}
```

## Method of operation
* Click an item from the menu on the left side of the screen to place a node.
* Left click and drag to connect the nodes with the circle and the square.
* Ctrl + left click on the square to disconnect the node.
* Range selection by dragging the mouse while pressing the Shift key.
* Delete the selected node with the Delete key.
* Move node by left-click dragging (If you hold down the Ctrl key, move it along the guide).
* Move the entire screen by dragging with a left click on the screen (if selected, the selected one moves).
* Scale the display with the wheel button.
* Copy selected nodes with Ctrl + C.
* Paste with Ctrl + V (Selected after pasting).
* Save with Ctrl + S.
* Edit node name, argument name and variable name by double-clicking(Can be changed only in the variable name list on the left of the screen).

## Convenient function of script
* You can check the contents processed by the node just by moving the cursor.
* Just drag and drop numbers and letters to create a constant node.
* Supports type casting when connecting nodes(Does not support casting from objects).

## Tip display
Supports English and Japanese. However, in the initial state, it is English only machine translated.
To display in Japanese, modify the contents of the app.xml file created after execution.
```
<Language>en-US</Language>
```
Modify the above contents as follows.
```
<Language>ja-JP</Language>
```
The settings will be applied when you restart.

## Methods supported by method import
* Static method
* Class method(The self argument that receives this is added to the first argument)

## Corresponding method argument
* Type: int, string, double, sbyte, long, short, ushort, uint, ulong, char, float, decimal, bool, object
* Class
* Enum
* List&lt;&gt;
* Func&lt;&gt;
* Action&lt;&gt;
* Generic
* Default arguments
* Reference(Used as a variable reference)
* Overload(Different node types)

## Return type of the corresponding method
* Type: int, string, double, sbyte, long, short, ushort, uint, ulong, char, float, decimal, bool, object
* Class
* Enum
* List&lt;&gt;
* Func&lt;&gt;
* Action&lt;&gt;
* Generic
* void

## Method call from method
"Func <>" and "Action <>" type arguments can call the node as an external process. In case of "Func<>", connection can be made when the node type and return value type match.

## Script execution flow
Find the method to expose by reflection by the following method and register it in the menu.

```ScriptImplement.ImplemantScriptMethods(TreeMenuNode node)```

Public methods are registered and managed in the AutoImplementFunction class by the methods below.

```bool AutoImplementFunction.ImplAsset(MultiRootConnector col, bool noThreadMode = false, DummyArgumentsControl dummyArgumentsControl = null)```

(The method that has the function of calling an event is managed by the AutoImplementEventFunction class.)

By clicking the public method registered in the menu, the command will be registered in BaseWorkCanvas by the following method.

```TreeMenuNodeCommand CommandCanvas.CreateEventCanvasCommand(Func<object> action, TreeMenuNode vm = null)```

Command The registered command will be executed by the following method when the canvas (BaseWorkCanvas) is clicked.

```void BaseWorkCanvas.ProcessCommand(Point setPos)```

The node placed on the canvas is created by the MultiRootConnector created when the command is executed.
The contents of MultiRootConnector are formed by the following methods.

```void MultiRootConnector.MakeFunctionType()```

AutoImplementFunction.ImplAsset called in this registers the public method call process in the Function of MultiRootConnector by the following method.

```
void MultiRootConnector.MakeFunction(
            string name,
            string hint,
            Func<ICbVSValue> retType,
            List<ICbVSValue> argumentList,
            Func<List<ICbVSValue>, CbPushList, ICbVSValue> func
            )
```

The return value of the node on the UI is managed by RootConnector.
This RootConnector also has the ability to call public methods.
Arguments on UI are managed by LinkConnector, and arguments with List function are managed by LinkConnectorList of LinkConnector.

When the script is executed, RootConnector pulls out the information (return value) of RootConnector from the node connected to itself.
Call the public method from the obtained argument and calculate the return value of itself.
In this way we grab the LinkConnector from the RootConnector and calculate the final result.
In addition, basically, once the result is obtained, the processing is sped up by referencing only the result from RootConnector.
However, the node where Forced is checked and the event call (Func<>) will be calculated every time.
For nodes that need to be calculated each time (nodes that have state), you need to check Forced as necessary.

## License
MIT License

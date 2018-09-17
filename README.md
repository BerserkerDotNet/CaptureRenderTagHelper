# ScriptCaptureTagHelper
[![Build status](https://ci.appveyor.com/api/projects/status/vwivx49nk3ofn0p7/branch/master?svg=true)](https://ci.appveyor.com/project/BerserkerDotNet/scriptcapturetaghelper/branch/master)
[![Nuget](https://buildstats.info/nuget/ScriptCaptureTagHelper?v=0.3.1)](https://www.nuget.org/packages/ScriptCaptureTagHelper)

A set of Tag Helpers that can capture a script block and render it later in another place.

## Installing
1. Add a reference to the [package](https://www.nuget.org/packages/ScriptCaptureTagHelper):
    ```powershell
    PM> Install-Package ScriptCaptureTagHelper
    ```
    or
    ```cmd
    MyGreatProject> dotnet add package ScriptCaptureTagHelper
    ```
1. Restore packages:
    ```cmd
    MyGreatProject> dotnet restore
    ```
1. Register the Tag Helpers in your application's `_ViewImports.cshtml` file:
    ```
    @addTagHelper *, ScriptCaptureTagHelper
    ```
    
## Usage & Features:
Razor [sections](https://docs.microsoft.com/en-us/aspnet/core/mvc/views/layout#sections) does not work in partial views or in display templates. This TagHelper will capture a `script` block in the partial view or in the display template and render it later on the page.

### Simple capture
To capture a script block add `capture` attribute to a `<script>` tag
```html
<script capture='<UniqueID>'>
  ... JS code to capture
</script>
```
and to render this block, add `render` attribute to an empty `<script>` block in another file, specifying the same `UniqueID`:
```html
<script render='<UniqueID>'>
</script>
```
### Capturing multiple blocks

Multiple blocks can be captured by passing the same ID.
```html
<script capture='Foo'>
 console.log('Foo 1');
</script>
<script capture='Foo'>
 console.log('Foo 2');
</script>
```
and later rendered with a single `script` tag:
```html
<script render='Foo'></script>
```
which by default will expand into two `script` tags that were captured previously.
```html
<script>
    console.log('Foo 1');
</script>
<script>
    console.log('Foo 2');
</script>
```

### Changing the order
By default the script blocks will be rendered in the same order as they were captured.
This can be changed by setting `priority` attribute upon capture.
```html
<script capture="SimplePriority" priority="3">
    console.log('SimplePriority 1');
</script>
<script capture="SimplePriority" priority="2">
    console.log('SimplePriority 2');
</script>
<script capture="SimplePriority" priority="1" src="SimplePriority.js"></script>
```
The result of rendering this:
```html
<script src="SimplePriority.js"></script>
<script>
    console.log('SimplePriority 2');
</script>
<script>
    console.log('SimplePriority 1');
</script>
``` 

### Merging script blocks
There are couple of ways to tell `ScriptRenderTagHelper` to merge captured blocks
1. By using `auto-merge` attribute on the render tag helper.

    If set to true, render tag helper will merge script blocks that have content and not marked with `allow-merge='false'` upon capture.
    ```html
    <script capture="AutoMerge">
        console.log('AutoMerge 1');
    </script>
    <script capture="AutoMerge">
        console.log('AutoMerge 2');
    </script>
    <script capture="AutoMerge" src="AutoMerge.js"></script>
    // somewhere in another file
    <script render="AutoMerge" auto-merge="true">
    </script>
    ```
    the output of the render would be
    ```html
    <script>
        console.log('AutoMerge 1');

        console.log('AutoMerge 2');
    </script>
    <script src="AutoMerge.js"></script>
    ```
2. By setting `allow-merge='true'` upon capture.

    In this case only blocks that are explicitly marked for merge will be merged on render.
    ```html
    <script capture="ExplicitMerge" allow-merge="true">
        console.log('ExplicitMerge 1');
    </script>
    <script capture="ExplicitMerge" allow-merge="true">
        console.log('ExplicitMerge 2');
    </script>
    <script capture="ExplicitMerge" allow-merge="true" src="ExplicitMerge.js"></script>
    // somewhere in another file
    <script render="ExplicitMerge" auto-merge="false">
    </script>
    ```
    the output is
    ```html
    <script>
        console.log('ExplicitMerge 1');

        console.log('ExplicitMerge 2');
    </script>
    <script src="ExplicitMerge.js"></script>
    ```
    Same as with auto merge, script references do not get merged.

To prevent the merge of one particular script block, set `allow-merge='false'`.

### Duplicate script blocks
From version 0.3.1 `render` tag helper will automatically detect script blocks with identical `src` attributes and will omit duplicates.
This is useful when need to render multiple display templates or components on the same page that have a script reference.
For example, in the following case:
```html
<script capture="NoDuplicate" src="Duplicate.js"></script>
<script capture="NoDuplicate">
    console.log('NoDuplicate 1');
</script>
<script capture="NoDuplicate" src="Duplicate.js"></script>

/// Later in the code
<script render="NoDuplicate">
</script>
```
the output will be:
```html
<script src="Duplicate.js"></script>
<script>
    console.log('NoDuplicate 1');
</script>```
This can be disabled by setting `no-duplicate-source` to `false`:
```html
<script render="NoDuplicateDisabled" no-duplicate-source="false">
</script>
```
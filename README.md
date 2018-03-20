# ScriptCaptureTagHelper
[![Build status](https://ci.appveyor.com/api/projects/status/vwivx49nk3ofn0p7/branch/master?svg=true)](https://ci.appveyor.com/project/BerserkerDotNet/scriptcapturetaghelper/branch/master)

A set of Tag Helpers that can capture a script block and render it later in another place.

## Installing
1. Add a reference to a [package](https://www.nuget.org/packages/ScriptCaptureTagHelper):
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
    @addTagHelper *, TagHelperPack
    ```
    
## Usage:
Razor [sections](https://docs.microsoft.com/en-us/aspnet/core/mvc/views/layout#sections) does not work in partial views or in display templates. This TagHelper will capture a `script` block in the partial view or in the display template and render it later on the page.

To capture a script block add `capture` attribute to a `<script>` tag
```html
<script capture = '<UniqueID>'>
  ... JS code to capture
</script>
```
and to render this block in another file, add `render` attribute to an empty `<script>` block, specifying the same `UniqueID`:
```html
<script render = '<UniqueID>'>
</script>
```

## New in version  0.2.0 
1. Capture multiple blocks by passing the same ID.
```html
<script capture='Foo'>
 console.log('Foo 1');
</script>
<script capture='Foo'>
 console.log('Foo 2');
</script>
```
later it can be rendered via single `script` tag:
```html
<script render='Foo'></script>
```
which will expand into two captured tags.
2. Control the order in which captured scripts will be rendered by specifying `priority` attribute.
    This attribute is optional, if not specified block will be rendered as the last one.
3. `script` tag attributes are now preserved, which enables the scenario for capturing script reference:
```html
<script capture='InlineCapture' src='some good CDN' integrity='sha256-bla' crossorigin='anonymous'></script>
```
will be rendered with all attributes as it is.

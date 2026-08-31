This folder is the "trusted" plugin location the app checks LAST
(see Services/UpdateService.cs -> LoadPlugin).

For the DLL hijacking exercise:
1. Build a minimal .NET class library named ReportExporter.dll
   (any public class works -- put a MessageBox.Show or a file-write in
   its static constructor / a parameterless method invoked on load to
   prove code execution).
2. Do NOT place it here. Instead, copy it into the directory c03e.exe is
   *launched from* (its current working directory), which may differ
   from this Plugins\ folder depending on how the app is started
   (double-click vs. shortcut with a different "Start in" path, vs.
   launched from a shared/USB folder).
3. From the Dashboard, trigger a plugin load (or call
   UpdateService.LoadPlugin("ReportExporter.dll") from a debugger/test
   harness) and observe that your planted DLL loads instead of -- or in
   the absence of -- the real one in this folder.

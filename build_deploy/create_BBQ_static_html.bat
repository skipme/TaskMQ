@echo off
echo cleaning...
FOR /D %%p IN ("BBQ_static\*.*") DO rmdir "%%p" /s /q

if not exist "create_BBQ_static_html.exe" (
echo compiling tool...
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\csc.exe /reference:"System.dll" /reference:"System.Core.dll"^
 /reference:".\bin\RazorEngine\RazorEngine.dll" create_BBQ_static_html.cs

)
if not exist "RazorEngine.dll" (
copy ".\bin\RazorEngine\RazorEngine.dll" "RazorEngine.dll" 
)
if not exist "System.Web.Razor.dll" (
copy ".\bin\RazorEngine\System.Web.Razor.dll" "System.Web.Razor.dll" 
)
create_BBQ_static_html.exe
xcopy ..\BBQ\Scripts BBQ_static\Scripts\ /e /i
xcopy ..\BBQ\Content BBQ_static\Content\ /e /i
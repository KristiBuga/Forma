$ErrorActionPreference='Stop'
Push-Location $PSScriptRoot
try {
    $application=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
    $compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
    & $compiler /nologo /codepage:65001 /platform:x64 /target:exe /out:..\OcrWorker.exe /reference:..\Tesseract.dll OcrWorker.cs
    if($LASTEXITCODE -ne 0){throw 'OCR build failed'}
    $arguments=@('/nologo','/codepage:65001','/platform:x64','/target:winexe','/win32manifest:app.manifest','/win32icon:..\Forma.ico','/out:..\Forma.exe','/reference:System.Windows.Forms.dll','/reference:System.Drawing.dll','/reference:System.IO.Compression.dll','/reference:System.IO.Compression.FileSystem.dll','/reference:System.Xml.dll',('/reference:'+(Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\WPF\PresentationCore.dll')),('/reference:'+(Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\WPF\WindowsBase.dll')))
    $sources=Get-ChildItem -LiteralPath $PSScriptRoot -Filter '*.cs' | Where-Object {$_.Name -ne 'OcrWorker.cs'} | ForEach-Object {$_.FullName}
    & $compiler $arguments $sources
    if($LASTEXITCODE -ne 0){throw 'Forma build failed'}
} finally {Pop-Location}

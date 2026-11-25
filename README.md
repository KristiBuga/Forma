# Forma

Forma is a Windows desktop app for converting media and extracting text from documents. Processing runs locally on your PC.

## Features

- Convert audio, video and image formats
- Create and edit GIFs
- Extract text from PDF, DOCX, PPTX and XLSX
- OCR for Russian and English
- Compact, step-by-step and studio layouts
- Windows notification area menu

## Download

Download Forma-Plus-Windows.zip from the Releases page, extract the complete archive and run Forma.exe.

The source repository omits bundled runtime binaries and OCR language data. The Windows release archive includes the complete application.

## Build

The C# sources and build script are in source. Building the OCR worker requires its runtime dependencies. FFmpeg and OCR language data are distributed in the Windows release archive.

Third-party license and notice files are included.
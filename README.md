# Instalador de programas

Proyecto WinForms para Windows x64 con .NET 8. El manifiesto solicita elevación al iniciar la aplicación, por lo que Windows muestra un único UAC. Los procesos de instalación se ejecutan desde esa aplicación elevada.

## Archivos de instalación

Copiá los instaladores a estas ubicaciones, manteniendo sus nombres:

```text
programas/
├── Acrobat/Setup.exe
├── OpenVPN-2.5.7-I602-amd64.msi
├── ChromeSetup.exe
├── WinRAR.exe
├── Office2016.iso
└── config.xml
```

Para Chrome también se acepta `programas/p/ChromeSetup.exe`, por la ruta `p\ChromeSetup.exe` indicada. El `config.xml` incluido corresponde a Office 2016. La extensión correcta para el contenido XML es `.xml`.

## Comandos usados

- Acrobat: `Setup.exe /sPB`
- OpenVPN: `msiexec /i "OpenVPN-2.5.7-I602-amd64.msi" /norestart /passive`
- Chrome: `ChromeSetup.exe /silent`
- WinRAR: `WinRAR.exe /s` (se interpreta el `echo` escrito en el mensaje como separador; `echo` solo imprimiría texto y no instalaría el programa).
- Office 2016: monta la ISO, ejecuta `setup.exe /config "...\config.xml"`, espera el resultado y desmonta la ISO.

Las instalaciones se procesan en orden, aunque una falle. Al terminar aparece un resumen con el resultado de cada una.

## Compilar

Abrí `InstaladorProgramas.csproj` con Visual Studio 2022 (carga de trabajo .NET para escritorio) o compilá con el SDK .NET 8 instalado en Windows. El proyecto está configurado para producir una aplicación autocontenida x64:

```powershell
dotnet build -c Release
```

El ejecutable compilado se genera en `bin\Release\net8.0-windows\win-x64\`. El `config.xml` se copia automáticamente a `bin\Release\net8.0-windows\win-x64\programas\`. Copiá los instaladores en las ubicaciones correspondientes dentro de esa carpeta `programas` antes de usarlo. El proyecto no distribuye los instaladores comerciales.
